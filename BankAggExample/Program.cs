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
using BankAggExample.Infrastructure;
using BankAggExample.Infrastructure.Projections;
using BankAggExample.Read.Projections;
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
}
