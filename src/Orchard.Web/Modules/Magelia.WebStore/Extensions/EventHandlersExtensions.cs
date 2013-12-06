using Orchard.Events;
using System;
using System.Collections.Generic;

namespace Magelia.WebStore.Extensions
{
    public static class EventHandlersExtensions
    {
        public static void Trigger<TEventHandler>(this IEnumerable<TEventHandler> handlers, Action<TEventHandler> action)
            where TEventHandler : IEventHandler
        {
            foreach (TEventHandler handler in handlers)
            {
                action(handler);
            }
        }
    }
}