using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BankAggExample.Domain.Events;
using CQRSlite.Events;

namespace BankAggExample.Read.Projections
{
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
}
