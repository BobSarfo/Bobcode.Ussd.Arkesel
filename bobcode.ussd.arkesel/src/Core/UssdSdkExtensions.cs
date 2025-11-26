using System.Reflection;
using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Core;

/// <summary>
/// Extension methods for configuring USSD SDK services
/// </summary>
public static class UssdSdkExtensions
{
    /// <summary>
    /// Adds USSD SDK services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="menu">The menu structure for the USSD application</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUssdSdk(
        this IServiceCollection services,
        Menu menu,
        UssdOptions? options = null)
    {
        services.AddSingleton(menu);
        services.AddSingleton(options ?? new UssdOptions());
        services.AddSingleton<IUssdSessionStore, MemorySessionStore>();
        services.AddScoped<UssdApp>();
        services.AddMemoryCache();
        return services;
    }

    /// <summary>
    /// Adds USSD SDK services to the service collection with inline configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="menu">The menu structure for the USSD application</param>
    /// <param name="configureOptions">Action to configure USSD options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUssdSdk(
        this IServiceCollection services,
        Menu menu,
        Action<UssdOptions> configureOptions)
    {
        var options = new UssdOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(menu);
        services.AddSingleton(options);
        services.AddSingleton<IUssdSessionStore, MemorySessionStore>();
        services.AddScoped<UssdApp>();
        services.AddMemoryCache();
        return services;
    }

    /// <summary>
    /// Adds USSD SDK services with a custom session store
    /// </summary>
    /// <typeparam name="TSessionStore">The session store implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="menu">The menu structure for the USSD application</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUssdSdk<TSessionStore>(
        this IServiceCollection services,
        Menu menu,
        UssdOptions? options = null)
        where TSessionStore : class, IUssdSessionStore
    {
        services.AddSingleton(menu);
        services.AddSingleton(options ?? new UssdOptions());
        services.AddSingleton<IUssdSessionStore, TSessionStore>();
        services.AddScoped<UssdApp>();
        services.AddMemoryCache();
        return services;
    }

    /// <summary>
    /// Adds USSD SDK services with a custom session store and inline configuration
    /// </summary>
    /// <typeparam name="TSessionStore">The session store implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="menu">The menu structure for the USSD application</param>
    /// <param name="configureOptions">Action to configure USSD options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUssdSdk<TSessionStore>(
        this IServiceCollection services,
        Menu menu,
        Action<UssdOptions> configureOptions)
        where TSessionStore : class, IUssdSessionStore
    {
        var options = new UssdOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(menu);
        services.AddSingleton(options);
        services.AddSingleton<IUssdSessionStore, TSessionStore>();
        services.AddScoped<UssdApp>();
        services.AddMemoryCache();
        return services;
    }

    /// <summary>
    /// Registers an action handler
    /// </summary>
    /// <typeparam name="THandler">The action handler type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddActionHandler<THandler>(this IServiceCollection services)
        where THandler : class, IActionHandler
    {
        services.AddSingleton<IActionHandler, THandler>();
        return services;
    }

    /// <summary>
    /// Auto-discovers and registers all action handlers from the specified assembly.
    /// Handlers must implement IActionHandler and be marked with [UssdAction] attribute.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assembly">The assembly to scan for action handlers</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUssdActionsFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => typeof(IActionHandler).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<UssdActionAttribute>() != null);

        foreach (var handlerType in handlerTypes)
        {
            services.AddSingleton(typeof(IActionHandler), handlerType);
        }

        return services;
    }

    /// <summary>
    /// Auto-discovers and registers all action handlers from the calling assembly.
    /// Handlers must implement IActionHandler and be marked with [UssdAction] attribute.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUssdActionsFromCallingAssembly(this IServiceCollection services)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        return services.AddUssdActionsFromAssembly(callingAssembly);
    }
}
