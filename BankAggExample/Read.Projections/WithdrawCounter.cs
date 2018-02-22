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
}
