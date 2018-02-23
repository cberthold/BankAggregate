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
    public class TotalBankValue : BaseProjection<TotalBankValue>,
        IHandleProjectedEvent<AmountWithdrawn>,
        IHandleProjectedEvent<AmountDeposited>,
        IHandleProjectedEvent<AccountCreated>
    {
        public decimal Value { get; private set; }

        /*
        /// <summary>
        /// The Handleevents override is the same as handling the individual IHandleProjectedEvent interfaces above
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
        */

        public Task HandleEvent(AmountWithdrawn @event, CancellationToken cancellationToken)
        {
            Value -= @event.Amount;
            return Task.FromResult(0);
        }

        public Task HandleEvent(AmountDeposited @event, CancellationToken cancellationToken)
        {
            Value += @event.Amount;
            return Task.FromResult(0);
        }

        public Task HandleEvent(AccountCreated @event, CancellationToken cancellationToken)
        {
            Value += @event.DepositAmount;
            return Task.FromResult(0);
        }
    }
}
