using System;
using System.Collections.Generic;
using System.Text;

namespace BankAggExample.Infrastructure.Projections
{
    public class ProjectionHandlerDescriptor
    {
        public Type EventType { get; }
        public bool HasHandler { get; }
        public Type EventInterfaceType { get; }

        public ProjectionHandlerDescriptor(Type eventType, bool hasHandler, Type eventInterfaceType)
        {
            EventType = eventType;
            HasHandler = hasHandler;
            EventInterfaceType = eventInterfaceType;
        }
    }
}
