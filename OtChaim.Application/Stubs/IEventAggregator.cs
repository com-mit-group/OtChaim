// Temporary stub for missing Yaref92.Events package to allow focusing on UserInfo functionality

using Yaref92.Events;

namespace Yaref92.Events.Abstractions
{
    /// <summary>
    /// Temporary stub interface for missing Yaref92.Events package
    /// </summary>
    public interface IEventAggregator
    {
        Task PublishAsync<T>(T @event) where T : class;
    }

    /// <summary>
    /// Temporary stub interface for missing Yaref92.Events package
    /// </summary>
    public interface IAsyncEventSubscriber<T> where T : IEvent
    {
        Task HandleAsync(T @event, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Temporary stub implementation for missing Yaref92.Events package
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        public Task PublishAsync<T>(T @event) where T : class
        {
            // Stub implementation - just return completed task
            return Task.CompletedTask;
        }
    }
}