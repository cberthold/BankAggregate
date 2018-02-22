using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Events;
using MediatR;

namespace BankAggExample.Infrastructure.Projections
{
    public abstract class BaseProjection<TProjection> : INotificationHandler<ProjectionBatch>
        where TProjection : BaseProjection<TProjection>
    {

        private readonly ConcurrentDictionary<Type, ProjectionHandlerDescriptor> descriptorCache = new ConcurrentDictionary<Type, ProjectionHandlerDescriptor>();

        public Task Handle(ProjectionBatch projection, CancellationToken cancellationToken)
        {
            return HandleEvents(projection.Events, cancellationToken);
        }

        protected virtual async Task HandleEvents(IEnumerable<IEvent> events, CancellationToken cancellationToken)
        {
            foreach (var @event in events)
            {
                var descriptor = GetDescriptorForEvent(@event);
                if (descriptor.HasHandler)
                {
                    await HandleEvent(@event, descriptor, cancellationToken);
                }
            }
        }

        private async Task HandleEvent<TEvent>(TEvent @event, ProjectionHandlerDescriptor descriptor, CancellationToken cancellationToken)
            where TEvent : IEvent
        {
            var genericType = typeof(IHandleProjectedEvent<>).MakeGenericType(descriptor.EventType);
            var genericMethod = genericType.GetMethod("HandleEvent", BindingFlags.Public);
            var task = (Task)genericMethod.Invoke(this, new object[] { @event, cancellationToken });
            await task.ConfigureAwait(false);
        }

        private ProjectionHandlerDescriptor GetDescriptorForEvent<TEvent>(TEvent @event)
        {
            var eventType = @event.GetType();
            var projectionType = GetType();

            Func<Type, ProjectionHandlerDescriptor> valueFactory = (t) =>
            {
                var genericInterface = typeof(IHandleProjectedEvent<>);
                var genericInterfaceToFind = genericInterface.MakeGenericType(t);
                var hasHandler = false;

                if (projectionType.GetInterfaces().Any(b => b.Equals(genericInterfaceToFind)))
                {
                    hasHandler = true;
                }

                var descriptorBuilt = new ProjectionHandlerDescriptor(t, hasHandler);
                return descriptorBuilt;
            };

            var descriptor = descriptorCache.GetOrAdd(eventType, valueFactory);
            return descriptor;
        }
    }
}
