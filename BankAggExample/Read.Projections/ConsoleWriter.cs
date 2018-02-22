using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BankAggExample.Domain.Events;
using CQRSlite.Events;

namespace BankAggExample.Read.Projections
{
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
}
