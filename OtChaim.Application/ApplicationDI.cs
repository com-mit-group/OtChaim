using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OtChaim.Application.Common;
using OtChaim.Application.EmergencyEvents.Commands;
using OtChaim.Application.EmergencyEvents.EventSubscribers;
using OtChaim.Application.EmergencyEvents.Handlers;
using OtChaim.Application.Services;
using OtChaim.Application.Users.Commands;
using OtChaim.Application.Users.EventSubscribers;
using OtChaim.Application.Users.Handlers;
using OtChaim.Domain.EmergencyEvents.Events;
using OtChaim.Domain.Users.Events;
using Yaref92.Events;
using Yaref92.Events.Abstractions;
using Yaref92.Events.Serialization;
using Yaref92.Events.Transports;

namespace OtChaim.Application;

/// <summary>
/// Provides extension methods for configuring event aggregation and dependency injection.
/// </summary>
public static class ApplicationDI
{
    private static readonly int ListenPort = 9008;

    /// <summary>
    /// Adds and configures the event aggregator and related services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEventAggregator(this IServiceCollection services)
    {
        services.AddSingleton<IEventSerializer, JsonEventSerializer>();
        services.AddSingleton<IEventTransport, TCPEventTransport>(provider =>
        {
            IEventSerializer serializer = provider.GetRequiredService<IEventSerializer>();
            TCPEventTransport transport = new TCPEventTransport(ListenPort, serializer); // Choose your port
            return transport;
        });

        // Register logger
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>(); // TODO: put an actual logger factory
        services.AddSingleton<ILogger<EventAggregator>, Logger<EventAggregator>>();

        // Register the networked aggregator as a singleton
        services.AddSingleton<IEventAggregator>(provider =>
        {
            ILogger<EventAggregator>? logger = provider.GetService<ILogger<EventAggregator>>();
            EventAggregator localAggregator = new EventAggregator(logger);
            TCPEventTransport transport = (provider.GetRequiredService<IEventTransport>() as TCPEventTransport)!; // Choose your port
            NetworkedEventAggregator networkedAggregator = new NetworkedEventAggregator(localAggregator, transport!);

            // Start listening asynchronously with error handling
            Task.Run(async () =>
            {
                try
                {
                    await transport.StartListeningAsync();
                }
                catch (Exception ex)
                {
                    // Log the error but don't crash the application
                    System.Diagnostics.Debug.WriteLine($"Failed to start TCP transport on port {ListenPort}: {ex.Message}");
                }
            });

            // Optionally connect to peers here, or expose transport for runtime connections

            RegisterEventTypes(networkedAggregator);

            return networkedAggregator;
        });

        // Register event subscribers
        services.AddScoped<SubscriptionEventSubscriber>();
        services.AddScoped<EmergencyEventSubscriber>();

        // Register command handlers
        services.AddScoped<ICommandHandler<RequestSubscription>, RequestSubscriptionHandler>();
        services.AddScoped<ICommandHandler<ApproveSubscription>, ApproveSubscriptionHandler>();
        services.AddScoped<ICommandHandler<RejectSubscription>, RejectSubscriptionHandler>();
        services.AddScoped<ICommandHandler<StartEmergency>, StartEmergencyHandler>();
        services.AddScoped<ICommandHandler<EndEmergency>, EndEmergencyHandler>();
        services.AddScoped<ICommandHandler<MarkUserStatus>, MarkUserStatusHandler>();

        services.AddScoped<ICurrentUserProvider, PrimaryUserCurrentUserProvider>();

        return services;
    }
    public static void RegisterEventTypes(IEventAggregator eventAggregator)
    {
        eventAggregator.RegisterEventType<SubscriptionRequested>();
        eventAggregator.RegisterEventType<SubscriptionApproved>();
        eventAggregator.RegisterEventType<SubscriptionRejected>();
        eventAggregator.RegisterEventType<EmergencyStarted>();
        eventAggregator.RegisterEventType<EmergencyEnded>();
        eventAggregator.RegisterEventType<EmergencyPersisted>();
        eventAggregator.RegisterEventType<EmergencyAltered>();
        eventAggregator.RegisterEventType<EmergencyAlterationPersisted>();
        eventAggregator.RegisterEventType<UserStatusMarked>();
        eventAggregator.RegisterEventType<SubscriberNotified>();
    }

    public static void SubscribeEventHandlers(IServiceProvider provider)
    {
        IEventAggregator eventAggregator = provider.GetRequiredService<IEventAggregator>();

        SubscriptionEventSubscriber subscriptionSubscriber = provider.GetRequiredService<SubscriptionEventSubscriber>();
        eventAggregator.SubscribeToEventType<SubscriptionRequested>(subscriptionSubscriber);
        eventAggregator.SubscribeToEventType<SubscriptionApproved>(subscriptionSubscriber);
        eventAggregator.SubscribeToEventType<SubscriptionRejected>(subscriptionSubscriber);

        EmergencyEventSubscriber emergencySubscriber = provider.GetRequiredService<EmergencyEventSubscriber>();
        eventAggregator.SubscribeToEventType<EmergencyStarted>(emergencySubscriber);
        eventAggregator.SubscribeToEventType<EmergencyEnded>(emergencySubscriber);
        eventAggregator.SubscribeToEventType<UserStatusMarked>(emergencySubscriber);
        eventAggregator.SubscribeToEventType<SubscriberNotified>(emergencySubscriber);
    }
}
