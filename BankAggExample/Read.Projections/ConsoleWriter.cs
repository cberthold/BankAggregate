using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BankAggExample.Domain.Events;
using BankAggExample.Infrastructure.Projections;
using CQRSlite.Events;

namespace BankAggExample.Read.Projections
{
    public class ConsoleWriter : BaseProjection<ConsoleWriter>, 
        IHandleProjectedEvent<AmountWithdrawn>,
        IHandleProjectedEvent<AmountDeposited>,
        IHandleProjectedEvent<AccountCreated>
    {
        /*
        /// <summary>
        /// This is the same as implementing the interfaces above
        /// </summary>
        /// <param name="events"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
        */
        public Task HandleEvent(AmountWithdrawn @event, CancellationToken cancellationToken)
        {
            Console.WriteLine($"CW - AmountWithdrawn: {@event.Amount}");
            return Task.FromResult(0);
        }

        public Task HandleEvent(AmountDeposited @event, CancellationToken cancellationToken)
        {
            Console.WriteLine($"CW - AmountDeposited: {@event.Amount}");
            return Task.FromResult(0);
        }

        public Task HandleEvent(AccountCreated @event, CancellationToken cancellationToken)
        {
            Console.WriteLine($"CW - AccountCreated with deposit amount: {@event.DepositAmount}");
            return Task.FromResult(0);
        }
    }
}
