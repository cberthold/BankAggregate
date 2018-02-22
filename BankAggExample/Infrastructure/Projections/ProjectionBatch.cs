using System;
using System.Collections.Generic;
using System.Text;
using CQRSlite.Events;
using MediatR;

namespace BankAggExample.Infrastructure.Projections
{
    public sealed class ProjectionBatch : INotification
    {
        public IEnumerable<IEvent> Events { get; }

        public ProjectionBatch(IEnumerable<IEvent> events)
        {
            Events = events;
        }
    }
}
