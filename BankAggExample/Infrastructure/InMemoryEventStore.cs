using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Events;

namespace BankAggExample
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<Guid, List<IEvent>> _inMemoryDB = new Dictionary<Guid, List<IEvent>>();

        public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<IEvent> events;
            _inMemoryDB.TryGetValue(aggregateId, out events);
            var returnEvents = events?.Where(x => x.Version > fromVersion) ?? new List<IEvent>();
            return Task.FromResult(returnEvents);
        }

        public Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var @event in events)
            {
                List<IEvent> list;
                _inMemoryDB.TryGetValue(@event.Id, out list);
                if (list == null)
                {
                    list = new List<IEvent>();
                    _inMemoryDB.Add(@event.Id, list);
                }
                list.Add(@event);
            }

            return Task.FromResult(0);
        }
    }
}
