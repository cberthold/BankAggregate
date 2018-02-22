using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Events;

namespace BankAggExample.Infrastructure.Projections
{
    public interface IHandleProjectedEvent<TEvent>
        where TEvent : IEvent
    {
        Task HandleEvent(TEvent @event, CancellationToken cancellationToken);
    }
}
