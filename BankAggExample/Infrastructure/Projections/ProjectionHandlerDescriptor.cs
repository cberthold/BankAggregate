using System;
using System.Collections.Generic;
using System.Text;

namespace BankAggExample.Infrastructure.Projections
{
    public class ProjectionHandlerDescriptor
    {
        public Type EventType { get; }
        public bool HasHandler { get; }

        public ProjectionHandlerDescriptor(Type eventType, bool hasHandler)
        {
            EventType = eventType;
            HasHandler = hasHandler;
        }
    }
}
