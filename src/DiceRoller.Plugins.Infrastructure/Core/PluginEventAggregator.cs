using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DiceRoller.Plugins.Infrastructure.Core
{
    /// <summary>
    /// Default implementation of plugin event aggregator
    /// </summary>
    public class PluginEventAggregator : IPluginEventAggregator
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
        private readonly object _lock = new();

        public void Publish<T>(T eventData) where T : class
        {
            if (eventData == null) return;

            var eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                var handlersCopy = handlers.ToList(); // Copy to avoid modification during iteration
                
                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        if (handler is Action<T> typedHandler)
                        {
                            typedHandler(eventData);
                        }
                    }
                    catch (Exception)
                    {
                        // Log error but don't throw to prevent one handler from breaking others
                        // In a real implementation, you'd use ILogger here
                    }
                }
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null) return;

            var eventType = typeof(T);
            
            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventType, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _handlers[eventType] = handlers;
                }
                
                handlers.Add(handler);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null) return;

            var eventType = typeof(T);
            
            lock (_lock)
            {
                if (_handlers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);
                    
                    if (handlers.Count == 0)
                    {
                        _handlers.TryRemove(eventType, out _);
                    }
                }
            }
        }
    }
}