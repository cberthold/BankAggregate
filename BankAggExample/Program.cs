using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using BankAggExample.Application.Service;
using BankAggExample.Command;
using BankAggExample.Domain;
using BankAggExample.Domain.Events;
using CQRSlite.Domain;
using CQRSlite.Events;
using MediatR;

namespace BankAggExample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = CreateContainer())
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var manager = container.Resolve<IBankManager>();

                        var accountId = await manager.CreateNewAccount();

                        await manager.DepositAmount(accountId, 20);

                        await manager.WithdrawAmount(accountId, 10);

                        await manager.WithdrawAmount(accountId, 5);

                        // should go negative here on this account
                        await manager.WithdrawAmount(accountId, 50);

                        var secondAccountId = await manager.CreateNewAccount();

                        await manager.DepositAmount(secondAccountId, 1000);

                        await manager.DepositAmount(secondAccountId, 50);

                        await manager.DepositAmount(secondAccountId, 95);

                        await manager.TransferFunds(secondAccountId, accountId, 50);


                        var counter = container.Resolve<WithdrawCounter>();
                        var bankTotal = container.Resolve<TotalBankValue>();

                        // should be 1
                        var counterValue = counter.Counter;
                        Console.WriteLine($"Withdrawn Events Counter value: {counterValue}");

                        // should be 1100
                        var totalValue = bankTotal.Value;
                        Console.WriteLine($"Total Bank Value: ${totalValue}");
                    }
                    catch (Exception ex)
                    {
                        var ex2 = ex;
                    }

                });

                task.Wait();

                Console.WriteLine("PRESS KEY YO");
                Console.ReadLine();
            }
        }

        private static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();

            // add MediatR to the container and have it look for any handlers
            // in the current assembly
            containerBuilder
                .AddMediatR(typeof(Program).Assembly)
                .RegisterRequestHandlers(typeof(Program).Assembly)
                .SingleInstance();

            containerBuilder
                .RegisterNotificationHandlers(typeof(Program).Assembly)
                .AsSelf()
                .SingleInstance();

            // in memory event store - only want one event store ever
            containerBuilder.RegisterType<InMemoryEventStore>().As<IEventStore>().SingleInstance();

            // register the repository with our generic event publisher - so projections can run
            containerBuilder.Register(ctx =>
            {
                var store = ctx.Resolve<IEventStore>();
                var publisher = ctx.Resolve<IEventPublisher>();

                return new Repository(store, publisher);
            }).As<IRepository>();

            // register our session
            containerBuilder.RegisterType<Session>().As<ISession>();

            // register the bank manager - this is where the commands are
            containerBuilder.RegisterType<BankManager>().As<IBankManager>();

            // register the generic event publisher to call the projections with each event that gets fired
            containerBuilder.RegisterType<GenericEventPublisher>().As<IEventPublisher>();

            // single instance - we only ever want one of these to exist - these are the projections
            //containerBuilder.RegisterType<WithdrawCounter>().AsSelf().AsImplementedInterfaces().SingleInstance();
            //containerBuilder.RegisterType<TotalBankValue>().AsSelf().AsImplementedInterfaces().SingleInstance();
            //containerBuilder.RegisterType<ConsoleWriter>().AsSelf().AsImplementedInterfaces().SingleInstance();

            var container = containerBuilder.Build();
            return container;
        }
    }

    #region projections

    public class GenericEventPublisher : IEventPublisher
    {
        private readonly IMediator mediator;

        public GenericEventPublisher(IMediator mediator)
        {
            this.mediator = mediator;
        }

        Task IEventPublisher.Publish<T>(T @event, CancellationToken cancellationToken)
        {
            var eventType = @event.GetType().Name;

            Console.WriteLine($"Running publishers for {eventType}");
            var events = new IEvent[] { @event };
            var batch = new ProjectionBatch(events);

            // completed normally
            return mediator.Publish(batch, cancellationToken);
        }
    }

    public sealed class ProjectionBatch : INotification
    {
        public IEnumerable<IEvent> Events { get; }

        public ProjectionBatch(IEnumerable<IEvent> events)
        {
            Events = events;
        }
    }

    public interface IHandleProjectedEvent<TEvent>
        where TEvent : IEvent
    {
        Task HandleEvent(TEvent @event, CancellationToken cancellationToken);
    }

    public abstract class BaseProjection<TProjection> : INotificationHandler<ProjectionBatch>
        where TProjection : BaseProjection<TProjection>
    {

        private readonly ConcurrentDictionary<Type, ProjectionHandlerDescriptor> descriptorCache = new ConcurrentDictionary<Type, ProjectionHandlerDescriptor>();

        public Task Handle(ProjectionBatch projection, CancellationToken cancellationToken)
        {
            return HandleEvents(projection.Events, cancellationToken);
        }

        protected virtual async Task HandleEvents(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        {
            foreach (var @event in events)
            {
                var descriptor = GetDescriptorForEvent(@event);
                if (descriptor.HasHandler)
                {
                    await HandleEvent(@event, descriptor, cancellationToken);
                }
            }
        }

        private async Task HandleEvent<TEvent>(TEvent @event, ProjectionHandlerDescriptor descriptor, CancellationToken cancellationToken)
            where TEvent : IEvent
        {
            var genericType = typeof(IHandleProjectedEvent<>).MakeGenericType(descriptor.EventType);
            var genericMethod = genericType.GetMethod("HandleEvent", BindingFlags.Public);
            var task = (Task)genericMethod.Invoke(this, new object[] { @event, cancellationToken });
            await task.ConfigureAwait(false);
        }

        private ProjectionHandlerDescriptor GetDescriptorForEvent<TEvent>(TEvent @event)
        {
            var eventType = @event.GetType();
            var projectionType = GetType();

            Func<Type, ProjectionHandlerDescriptor> valueFactory = (t) =>
            {
                var genericInterface = typeof(IHandleProjectedEvent<>);
                var genericInterfaceToFind = genericInterface.MakeGenericType(t);
                var hasHandler = false;

                if (projectionType.GetInterfaces().Any(b => b.Equals(genericInterfaceToFind)))
                {
                    hasHandler = true;
                }

                var descriptorBuilt = new ProjectionHandlerDescriptor(t, hasHandler);
                return descriptorBuilt;
            };

            var descriptor = descriptorCache.GetOrAdd(eventType, valueFactory);
            return descriptor;
        }

        private class ProjectionHandlerDescriptor
        {
            public Type EventType { get; }
            public bool HasHandler { get; }

            public ProjectionHandlerDescriptor(Type eventType, bool hasHandler)
            {
                EventType = eventType;
                HasHandler = hasHandler;
            }
        }
    }

    public class WithdrawCounter : BaseProjection<WithdrawCounter>
    {
        public int Counter { get; private set; }

        protected override Task HandleEvents(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        {
            CountFromEvents(events);
            return Task.FromResult(0);
        }

        protected void CountFromEvents(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                if (@event is AmountWithdrawn)
                {
                    Counter++;
                }
            }
        }
    }

    public class TotalBankValue : BaseProjection<TotalBankValue>
    {
        public decimal Value { get; private set; }

        protected override Task HandleEvents(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        {
            CountFromEvents(events);
            return Task.FromResult(0);
        }

        protected void CountFromEvents(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                switch (@event)
                {
                    case AmountWithdrawn aw:
                        Value -= aw.Amount;
                        break;
                    case AmountDeposited ad:
                        Value += ad.Amount;
                        break;
                    case AccountCreated ac:
                        Value += ac.DepositAmount;
                        break;
                }
            }

        }
    }

    public class ConsoleWriter : BaseProjection<ConsoleWriter>
    {
        protected override Task HandleEvents(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        {
            CountFromEvents(events);
            return Task.FromResult(0);
        }

        protected void CountFromEvents(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                Type eventType = @event.GetType();
                string typeName = eventType.Name;

                switch (@event)
                {
                    case AmountWithdrawn aw:
                        Console.WriteLine($"CW - AmountWithdrawn: {aw.Amount}");
                        break;
                    case AmountDeposited ad:
                        Console.WriteLine($"CW - AmountDeposited: {ad.Amount}");
                        break;
                    case AccountCreated ac:
                        Console.WriteLine($"CW - AccountCreated with deposit amount: {ac.DepositAmount}");
                        break;
                }
            }
        }
    }

    #endregion
}
