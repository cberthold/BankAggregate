using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BankAggExample.Infrastructure.Projections;
using CQRSlite.Events;
using MediatR;

namespace BankAggExample.Infrastructure
{
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
}
