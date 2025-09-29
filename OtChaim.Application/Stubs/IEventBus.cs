// Temporary stub for missing Yaref92.Events package to allow focusing on UserInfo functionality

namespace Yaref92.Events.Abstractions;

/// <summary>
/// Temporary stub interface for missing Yaref92.Events package
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : class;
}

/// <summary>
/// Temporary stub implementation for missing Yaref92.Events package
/// </summary>
public class EventBus : IEventBus
{
    public Task PublishAsync<T>(T @event) where T : class
    {
        // Stub implementation - just return completed task
        return Task.CompletedTask;
    }
}