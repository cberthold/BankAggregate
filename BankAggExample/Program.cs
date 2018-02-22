using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
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
            containerBuilder.AddMediatR(typeof(Program).Assembly);

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
            containerBuilder.RegisterType<WithdrawCounter>().AsSelf().SingleInstance();
            containerBuilder.RegisterType<TotalBankValue>().AsSelf().SingleInstance();
            containerBuilder.RegisterType<ConsoleWriter>().AsSelf().SingleInstance();

            var container = containerBuilder.Build();
            return container;
        }
    }

    #region Bank manager

    public class BankManager : IBankManager
    {

        private readonly ISession session;
        private readonly IMediator mediator;
        public BankManager(ISession session, IMediator mediator)
        {
            this.session = session;
            this.mediator = mediator;
        }

        public async Task<Guid> CreateNewAccount()
        {
            var token = new CancellationToken();
            var command = new CreateNewAccountCommand();
            return await mediator.Send(command, token);
        }

        public async Task DepositAmount(Guid accountId, decimal amount)
        {
            var token = new CancellationToken();
            var command = new DepositAmountCommand(accountId, amount);
            await mediator.Send(command, token);
        }

        public async Task TransferFunds(Guid fromAccountId, Guid toAccountId, decimal amountToTransfer)
        {
            var token = new CancellationToken();
            var command = new TransferFundsCommand(fromAccountId, toAccountId, amountToTransfer);
            await mediator.Send(command, token);
        }

        public async Task WithdrawAmount(Guid accountId, decimal amount)
        {
            var token = new CancellationToken();
            var command = new WithdrawAmountCommand(accountId, amount);
            await mediator.Send(command, token);
        }
    }

    public interface IBankManager
    {
        Task<Guid> CreateNewAccount();
        Task DepositAmount(Guid accountId, decimal amount);
        Task WithdrawAmount(Guid accountId, decimal amount);
        Task TransferFunds(Guid fromAccountId, Guid toAccountId, decimal amountToTransfer);
    }

    #endregion

    #region projections

    public class GenericEventPublisher : IEventPublisher
    {
        private readonly WithdrawCounter counter;
        private readonly TotalBankValue totalBank;
        private readonly ConsoleWriter console;

        public GenericEventPublisher(WithdrawCounter counter, TotalBankValue totalBank, ConsoleWriter console)
        {
            this.counter = counter;
            this.totalBank = totalBank;
            this.console = console;
        }

        Task IEventPublisher.Publish<T>(T @event, CancellationToken cancellationToken)
        {
            var eventType = @event.GetType().Name;

            Console.WriteLine($"Running publishers for {eventType}");
            counter.CountFromEvents(@event);
            totalBank.CountFromEvents(@event);
            console.CountFromEvents(@event);

            // completed normally
            return Task.FromResult(0);
        }
    }

    public class WithdrawCounter
    {
        public int Counter { get; private set; }

        public void CountFromEvents(IEvent @event)
        {
            if (@event is AmountWithdrawn)
            {
                Counter++;
            }

        }
    }

    public class TotalBankValue
    {
        public decimal Value { get; private set; }

        public void CountFromEvents(IEvent @event)
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

    public class ConsoleWriter
    {
        public void CountFromEvents(IEvent @event)
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

    #endregion
}
