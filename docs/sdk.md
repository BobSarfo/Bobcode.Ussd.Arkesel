# SDK Architecture Documentation

# OpenUSSD SDK - Internal Architecture

A comprehensive guide to the OpenUSSD SDK architecture, design patterns, and internal structure.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Architecture](#2-architecture)
3. [Folder Structure](#3-folder-structure)
4. [Core Components](#4-core-components)
5. [Design Patterns](#5-design-patterns)
6. [Data Flow](#6-data-flow)
7. [Extension Points](#7-extension-points)
8. [Class Diagrams](#8-class-diagrams)

---

# **1. Overview**

OpenUSSD is a .NET 8.0 SDK that provides a framework for building USSD (Unstructured Supplementary Service Data) applications. The SDK is designed with the following principles:

- **Type Safety**: Strongly-typed APIs using generics and enums
- **Extensibility**: Plugin architecture for custom session stores and handlers
- **Separation of Concerns**: Clear separation between menu structure, business logic, and session management
- **Dependency Injection**: Full DI support using Microsoft.Extensions.DependencyInjection
- **Fluent APIs**: Builder pattern for intuitive menu construction
- **Convention over Configuration**: Attribute-based auto-discovery with sensible defaults

### Session persistence default

`AddUssdSdk(Menu, …)` registers **`IUssdSessionStore`** as **`MemorySessionStore`** (backed by **`IMemoryCache`**). Sessions live **in the app process** and are **not** shared across instances.

**Redis is not the default.** For multiple servers or restarts that must keep sessions, register **`IConnectionMultiplexer`** (StackExchange.Redis) and use **`AddUssdSdk<RedisSessionStore>(Menu, …)`**.

---

# **2. Architecture**

## 2.1 High-Level Architecture

The SDK follows a layered architecture:

```
┌─────────────────────────────────────────────────────┐
│              Application Layer                       │
│  (User's USSD Application - Controllers, Handlers)  │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│               SDK Core Layer                         │
│  (UssdApp, Menu Processing, Navigation)              │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│            Infrastructure Layer                      │
│  (Session Store, DI Container, Extensions)           │
└─────────────────────────────────────────────────────┘
```

## 2.2 Component Interaction

```
UssdRequest → UssdApp → Menu → MenuNode → MenuOption
                 ↓                           ↓
            SessionStore              ActionHandler
                 ↓                           ↓
            UssdSession ← UssdContext ← UssdStepResult
                 ↓
            UssdResponse
```

---

# **3. Folder Structure**

The SDK source code is organized into the following directories:

```
OpenUSSD/src/
├── Abstractions/          (Reserved for future abstractions)
├── Actions/               Action handlers and session management
│   ├── BaseActionHandler.cs
│   ├── IActionHandler.cs
│   ├── IUssdSessionStore.cs
│   ├── MemorySessionStore.cs
│   ├── RedisSessionStore.cs
│   ├── MenuBuilder.cs
│   └── UssdContext.cs
├── Attributes/            Custom attributes for metadata
│   └── UssdActionAttribute.cs
├── Builders/              Fluent builders for menu construction
│   └── UssdMenuBuilder.cs
├── Core/                  Core SDK functionality
│   ├── UssdApp.cs
│   └── UssdSdkExtensions.cs
├── Extensions/            Extension methods
│   └── EnumExtensions.cs
├── Utilities/             Helper utilities
│   └── PaginationHelper.cs
└── models/                Domain models and DTOs
    ├── IMenuNode.cs
    ├── Menu.cs
    ├── MenuNode.cs
    ├── MenuOption.cs
    ├── SessionKey.cs
    ├── UssdOptions.cs
    ├── UssdRequest.cs
    ├── UssdResponse.cs
    ├── UssdSession.cs
    ├── UssdStep.cs
    └── UssdStepResult.cs
```

### Directory Responsibilities

- **Actions/**: Contains action handler interfaces, base classes, and session store implementations
- **Attributes/**: Custom attributes for marking and configuring SDK components
- **Builders/**: Fluent API builders for constructing menus
- **Core/**: Main SDK engine and dependency injection setup
- **Extensions/**: Extension methods for enums, collections, etc.
- **Utilities/**: Helper classes for pagination, formatting, etc.
- **models/**: Data models, DTOs, and domain entities

---

# **4. Core Components**

## 4.1 UssdApp

**Location**: `Core/UssdApp.cs`

The main orchestrator that processes USSD requests and coordinates between menus, sessions, and handlers.

**Responsibilities**:
- Request processing and routing
- Session lifecycle management
- Menu rendering
- Navigation handling (back, home, next)
- Session resumption logic
- Action handler execution

**Key Methods**:
```csharp
public async Task<UssdResponseDto> HandleRequestAsync(UssdRequestDto request)
private UssdStepResult RenderMenu(UssdSession session)
private async Task<UssdStepResult> ProcessRequest(UssdSession session, string input)
```

**Dependencies**:
- `Menu`: The menu structure
- `IUssdSessionStore`: Session persistence
- `UssdOptions`: Configuration options
- `IServiceProvider`: DI container for resolving handlers
- `ILogger<UssdApp>`: Logging

---


## 4.2 Menu System

### Menu

**Location**: `models/Menu.cs`

Represents the complete menu structure for a USSD application.

**Properties**:
```csharp
public string Id { get; init; }
public Dictionary<string, MenuNode> Nodes { get; }
public string RootId { get; set; }
```

**Methods**:
```csharp
public MenuNode GetNode(string id)
public bool TryGetNode(string id, out MenuNode? node)
public bool HasNode(string id)
```

### MenuNode

**Location**: `models/MenuNode.cs`

Represents a single node/screen in the menu hierarchy.

**Properties**:
```csharp
public string Id { get; init; }
public string Title { get; set; }
public IList<MenuOption> Options { get; }
public bool IsTerminal { get; set; }
public bool IsPaginated { get; set; }
public int ItemsPerPage { get; set; }
```

### MenuOption

**Location**: `models/MenuOption.cs`

Represents a selectable option within a menu node.

**Properties**:
```csharp
public string Input { get; set; }          // User input to select this option
public string Label { get; set; }          // Display text
public string? TargetStep { get; set; }    // Navigation target
public string? ActionKey { get; set; }     // Handler to execute
public bool IsWildcard { get; set; }       // Accept any input
```

---

## 4.3 Session Management

### UssdSession

**Location**: `models/UssdSession.cs`

Represents a user's USSD session with strongly-typed data access.

**Properties**:
```csharp
public string SessionId { get; init; }
public string Msisdn { get; init; }
public string UserId { get; init; }
public string Network { get; init; }
public int Level { get; set; }
public int Part { get; set; }
public string CurrentStep { get; set; }
public DateTime ExpireAt { get; set; }
public IDictionary<string, object?> Data { get; }
public bool AwaitingResumeChoice { get; set; }
public string? PreviousStep { get; set; }
```

**Methods**:
```csharp
public T? Get<T>(SessionKey<T> key)
public void Set<T>(SessionKey<T> key, T value)
public bool Has<T>(SessionKey<T> key)
public void Remove<T>(SessionKey<T> key)
```

### SessionKey<T>

**Location**: `models/SessionKey.cs`

Provides type-safe session data access using the Phantom Type pattern.

**Usage**:
```csharp
public static SessionKey<string?> Recipient => new("recipient");
public static SessionKey<decimal?> Amount => new("amount");
```

### IUssdSessionStore

**Location**: `Actions/IUssdSessionStore.cs`

Interface for session persistence implementations.

**Methods**:
```csharp
Task<UssdSession?> GetAsync(string sessionId, CancellationToken ct = default);
Task SetAsync(UssdSession session, TimeSpan ttl, CancellationToken ct = default);
Task RemoveAsync(string sessionId, CancellationToken ct = default);
```

**Implementations**:
- `MemorySessionStore`: In-memory storage using `IMemoryCache` (**default** for `AddUssdSdk` without a session-store type parameter)
- `RedisSessionStore`: Redis-based distributed storage (**opt-in** via `AddUssdSdk<RedisSessionStore>`)

---

## 4.4 Action Handlers

### IActionHandler

**Location**: `Actions/IActionHandler.cs`

Interface for implementing business logic handlers.

**Contract**:
```csharp
public interface IActionHandler
{
    string Key { get; }
    Task<UssdStepResult> HandleAsync(UssdContext context);
}
```

### BaseActionHandler

**Location**: `Actions/BaseActionHandler.cs`

Abstract base class providing helper methods for common operations.

**Helper Methods**:
```csharp
protected UssdStepResult Continue(string message, string? nextStep = null)
protected UssdStepResult End(string message)
protected UssdStepResult GoHome()
protected UssdStepResult GoTo<TNode>(TNode node) where TNode : struct, Enum
protected UssdStepResult GoTo(string nodeId)
protected T? Get<T>(UssdContext context, SessionKey<T> key)
protected void Set<T>(UssdContext context, SessionKey<T> key, T value)
protected bool Has<T>(UssdContext context, SessionKey<T> key)
protected void Remove<T>(UssdContext context, SessionKey<T> key)
```

### UssdContext

**Location**: `Actions/UssdContext.cs`

Provides context information to action handlers.

**Properties**:
```csharp
public UssdRequestDto Request { get; set; }
public UssdSession Session { get; set; }
public string? ContextActionKey { get; set; }
```

---

## 4.5 Menu Builders

### UssdMenuBuilder<TNode>

**Location**: `Builders/UssdMenuBuilder.cs`

Fluent API for building strongly-typed menus using enums.

**Key Methods**:
```csharp
public UssdMenuBuilder<TNode> Root(TNode rootNode)
public UssdMenuBuilder<TNode> Node(TNode node, Action<NodeBuilder<TNode>> configure)
public Menu Build()
```

**NodeBuilder Methods**:
```csharp
public NodeBuilder<TNode> Message(string message)
public NodeBuilder<TNode> Option(string input, string label)
public NodeBuilder<TNode> GoTo(TNode targetNode)
public NodeBuilder<TNode> Action<THandler>() where THandler : IActionHandler
public NodeBuilder<TNode> Input()
public NodeBuilder<TNode> OptionList<T>(IEnumerable<T> items, ...)
```

---

## 4.6 Dependency Injection

### UssdSdkExtensions

**Location**: `Core/UssdSdkExtensions.cs`

Extension methods for registering SDK services with the DI container.

**Session store:** non-generic **`AddUssdSdk`** overloads register **`MemorySessionStore`**. Generic **`AddUssdSdk<TSessionStore>`** registers your **`TSessionStore`** (for example **`RedisSessionStore`**) as **`IUssdSessionStore`**.

**Registration Methods**:
```csharp
// Register SDK with default session store
public static IServiceCollection AddUssdSdk(
    this IServiceCollection services,
    Menu menu,
    UssdOptions? options = null)

// Register SDK with inline configuration
public static IServiceCollection AddUssdSdk(
    this IServiceCollection services,
    Menu menu,
    Action<UssdOptions> configureOptions)

// Register SDK with custom session store
public static IServiceCollection AddUssdSdk<TSessionStore>(
    this IServiceCollection services,
    Menu menu,
    UssdOptions? options = null)
    where TSessionStore : class, IUssdSessionStore

// Register SDK with custom session store and inline configuration
public static IServiceCollection AddUssdSdk<TSessionStore>(
    this IServiceCollection services,
    Menu menu,
    Action<UssdOptions> configureOptions)
    where TSessionStore : class, IUssdSessionStore

// Auto-discover action handlers from assembly
public static IServiceCollection AddUssdActionsFromAssembly(
    this IServiceCollection services,
    Assembly assembly)

// Auto-discover action handlers from calling assembly
public static IServiceCollection AddUssdActionsFromCallingAssembly(
    this IServiceCollection services)
```

---


# **5. Design Patterns**

The SDK employs several well-established design patterns:

## 5.1 Builder Pattern

**Used in**: `UssdMenuBuilder<TNode>`, `NodeBuilder<TNode>`

Provides a fluent API for constructing complex menu structures step-by-step.

**Benefits**:
- Readable, declarative menu definitions
- Compile-time type safety with generics
- Method chaining for clean syntax
- Separation of construction logic from representation

**Example**:
```csharp
var menu = new UssdMenuBuilder<BankMenuNode>("bank")
    .Root(BankMenuNode.Main)
    .Node(BankMenuNode.Main, n => n
        .Message("Welcome")
        .Option("1", "Balance").Action<BalanceHandler>()
    )
    .Build();
```

---

## 5.2 Strategy Pattern

**Used in**: `IActionHandler` implementations

Encapsulates business logic algorithms in interchangeable handler classes.

**Benefits**:
- Decouples business logic from menu structure
- Easy to add new handlers without modifying existing code
- Testable in isolation
- Runtime handler selection based on menu configuration

**Example**:
```csharp
public interface IActionHandler
{
    Task<UssdStepResult> HandleAsync(UssdContext context);
}

[UssdAction]
public class BalanceCheckHandler : IActionHandler { ... }

[UssdAction]
public class TransferHandler : IActionHandler { ... }
```

---

## 5.3 Template Method Pattern

**Used in**: `BaseActionHandler`

Defines the skeleton of handler operations with customizable steps.

**Benefits**:
- Reusable helper methods across all handlers
- Consistent session management patterns
- Reduced boilerplate code
- Enforces common structure

**Example**:
```csharp
public abstract class BaseActionHandler : IActionHandler
{
    public abstract Task<UssdStepResult> HandleAsync(UssdContext context);

    protected UssdStepResult Continue(string message) { ... }
    protected UssdStepResult End(string message) { ... }
    protected T? Get<T>(UssdContext context, SessionKey<T> key) { ... }
}
```

---

## 5.4 Phantom Type Pattern

**Used in**: `SessionKey<T>`

Provides compile-time type safety for session data without runtime overhead.

**Benefits**:
- Type-safe session data access
- Prevents type mismatches at compile time
- No runtime performance cost
- Self-documenting code

**Example**:
```csharp
public static SessionKey<decimal?> Amount => new("amount");

// Type-safe access
decimal? amount = session.Get(SessionKeys.Amount);  // Returns decimal?
session.Set(SessionKeys.Amount, 100m);              // Accepts decimal?
```

---

## 5.5 Dependency Injection Pattern

**Used in**: Throughout the SDK

Inverts control of dependency creation to the DI container.

**Benefits**:
- Loose coupling between components
- Easy testing with mocks
- Centralized configuration
- Lifetime management

**Example**:
```csharp
services.AddSingleton<Menu>(menu);
services.AddSingleton<IUssdSessionStore, MemorySessionStore>();
services.AddScoped<UssdApp>();
```

---

## 5.6 Repository Pattern

**Used in**: `IUssdSessionStore`

Abstracts session persistence behind an interface.

**Benefits**:
- Swappable storage implementations
- Testable with in-memory stores
- Consistent data access API
- Separation of concerns

**Implementations**:
- `MemorySessionStore`: In-memory using IMemoryCache
- `RedisSessionStore`: Distributed using Redis

---

## 5.7 Attribute-Based Programming

**Used in**: `[UssdAction]` attribute

Marks classes for auto-discovery and configuration.

**Benefits**:
- Convention over configuration
- Reduced boilerplate registration code
- Metadata-driven behavior
- Reflection-based discovery

**Example**:
```csharp
[UssdAction]  // Auto-discovered and registered
public class BalanceCheckHandler : BaseActionHandler { ... }

[UssdAction("custom_key")]  // Custom action key
public class CustomHandler : BaseActionHandler { ... }
```

---

# **6. Data Flow**

## 6.1 Request Processing Flow

```
1. HTTP Request arrives at endpoint
   ↓
2. Deserialized to UssdRequestDto
   ↓
3. UssdApp.HandleRequestAsync() called
   ↓
4. Session retrieved from IUssdSessionStore
   ↓
5. Check for session resumption scenario
   ↓
6. If new session → Render root menu
   If existing → Process user input
   ↓
7. Match input to MenuOption
   ↓
8. If option has ActionKey → Execute IActionHandler
   If option has TargetStep → Navigate to node
   ↓
9. Handler returns UssdStepResult
   ↓
10. Process navigation (GoHome, NextStep)
    ↓
11. Update session state
    ↓
12. Save session to IUssdSessionStore
    ↓
13. Return UssdResponseDto
    ↓
14. Serialize to HTTP response
```

---

## 6.2 Menu Rendering Flow

```
1. Get current MenuNode from Menu.Nodes
   ↓
2. Build message from node.Title
   ↓
3. If paginated → Apply PaginationHelper
   ↓
4. Append options to message
   ↓
5. Add navigation hints (Back: 0, Home: #)
   ↓
6. Return UssdStepResult with message
```

---

## 6.3 Session Management Flow

```
1. Request arrives with SessionID
   ↓
2. IUssdSessionStore.GetAsync(sessionId)
   ↓
3. If session exists → Load from store
   If not → Create new UssdSession
   ↓
4. Process request and update session.Data
   ↓
5. Update session.CurrentStep, session.Level
   ↓
6. IUssdSessionStore.SetAsync(session, ttl)
   ↓
7. Session persisted with expiration
```

---

## 6.4 Action Handler Execution Flow

```
1. User input matches MenuOption with ActionKey
   ↓
2. Resolve IActionHandler from DI container
   ↓
3. Create UssdContext with request and session
   ↓
4. Call handler.HandleAsync(context)
   ↓
5. Handler accesses session data via context
   ↓
6. Handler performs business logic
   ↓
7. Handler returns UssdStepResult
   ↓
8. UssdApp processes result (Continue/End/Navigate)
```

---

# **7. Extension Points**

The SDK provides several extension points for customization:

## 7.1 Custom Session Store

Implement `IUssdSessionStore` for custom persistence:

```csharp
public class DatabaseSessionStore : IUssdSessionStore
{
    public async Task<UssdSession?> GetAsync(string sessionId, CancellationToken ct)
    {
        // Load from database
    }

    public async Task SetAsync(UssdSession session, TimeSpan ttl, CancellationToken ct)
    {
        // Save to database
    }

    public async Task RemoveAsync(string sessionId, CancellationToken ct)
    {
        // Delete from database
    }
}

// Register
builder.Services.AddUssdSdk<DatabaseSessionStore>(menu, options);
```

---

## 7.2 Custom Action Handlers

Create handlers by implementing `IActionHandler` or extending `BaseActionHandler`:

```csharp
[UssdAction]
public class CustomHandler : BaseActionHandler
{
    private readonly IMyService _service;

    public CustomHandler(IMyService service)
    {
        _service = service;  // DI supported
    }

    public override async Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        // Custom logic
        var result = await _service.DoSomethingAsync();
        return End($"Result: {result}");
    }
}
```

---


## 7.3 Custom Menu Builders

Extend the builder pattern for specialized menu types:

```csharp
public class CustomMenuBuilder<TNode> : UssdMenuBuilder<TNode>
    where TNode : struct, Enum
{
    public CustomMenuBuilder(string menuId) : base(menuId) { }

    // Add custom builder methods
    public CustomMenuBuilder<TNode> WithAnalytics()
    {
        // Add analytics configuration
        return this;
    }
}
```

---

## 7.4 Middleware Integration

Add custom middleware for request/response processing:

```csharp
app.Use(async (context, next) =>
{
    // Pre-processing
    await next();
    // Post-processing
});

app.MapPost("/ussd", async (UssdRequestDto request, UssdApp ussdApp) =>
{
    var response = await ussdApp.HandleRequestAsync(request);
    return Results.Ok(response);
});
```

---

# **8. Class Diagrams**

## 8.1 Core Components Diagram

```
┌─────────────────┐
│   UssdApp       │
├─────────────────┤
│ - _menu         │
│ - _sessionStore │
│ - _options      │
│ - _services     │
├─────────────────┤
│ + HandleRequestAsync()
│ - RenderMenu()  │
│ - ProcessRequest()
└────────┬────────┘
         │ uses
         ├──────────────┐
         │              │
         ▼              ▼
┌─────────────┐  ┌──────────────────┐
│    Menu     │  │ IUssdSessionStore│
├─────────────┤  ├──────────────────┤
│ + Id        │  │ + GetAsync()     │
│ + Nodes     │  │ + SetAsync()     │
│ + RootId    │  │ + RemoveAsync()  │
└─────────────┘  └──────────────────┘
         │                  △
         │                  │
         │                  │ implements
         ▼                  │
┌─────────────┐  ┌──────────────────┐
│  MenuNode   │  │ MemorySessionStore│
├─────────────┤  ├──────────────────┤
│ + Id        │  │ - _cache         │
│ + Title     │  └──────────────────┘
│ + Options   │
└─────────────┘  ┌──────────────────┐
         │       │ RedisSessionStore│
         │       ├──────────────────┤
         ▼       │ - _redis         │
┌─────────────┐  └──────────────────┘
│ MenuOption  │
├─────────────┤
│ + Input     │
│ + Label     │
│ + TargetStep│
│ + ActionKey │
└─────────────┘
```

---

## 8.2 Action Handler Hierarchy

```
┌──────────────────┐
│  IActionHandler  │ (interface)
├──────────────────┤
│ + Key            │
│ + HandleAsync()  │
└────────△─────────┘
         │
         │ implements
         │
┌────────┴──────────┐
│ BaseActionHandler │ (abstract)
├───────────────────┤
│ + Key             │
│ + HandleAsync()   │ (abstract)
├───────────────────┤
│ # Continue()      │
│ # End()           │
│ # GoHome()        │
│ # GoTo()          │
│ # Get()           │
│ # Set()           │
│ # Has()           │
│ # Remove()        │
└────────△──────────┘
         │
         │ extends
         │
    ┌────┴────┬──────────────┬─────────────┐
    │         │              │             │
    ▼         ▼              ▼             ▼
┌─────────┐ ┌──────────┐ ┌────────┐ ┌──────────┐
│Balance  │ │Transfer  │ │Voting  │ │  Custom  │
│Check    │ │Recipient │ │Action  │ │  Handler │
│Handler  │ │Handler   │ │Handler │ │          │
└─────────┘ └──────────┘ └────────┘ └──────────┘
```

---

## 8.3 Session Management Diagram

```
┌──────────────┐
│ UssdSession  │
├──────────────┤
│ + SessionId  │
│ + Msisdn     │
│ + UserId     │
│ + Network    │
│ + CurrentStep│
│ + Data       │
├──────────────┤
│ + Get<T>()   │
│ + Set<T>()   │
│ + Has<T>()   │
│ + Remove<T>()│
└──────┬───────┘
       │ uses
       ▼
┌──────────────┐
│SessionKey<T> │
├──────────────┤
│ + Key        │
└──────────────┘
       △
       │ typed by
       │
┌──────┴───────────────────┐
│ Application SessionKeys  │
├──────────────────────────┤
│ + Recipient: SessionKey<string?>
│ + Amount: SessionKey<decimal?>
│ + VoteChoice: SessionKey<string?>
└──────────────────────────┘
```

---

## 8.4 Menu Builder Diagram

```
┌──────────────────────────┐
│ UssdMenuBuilder<TNode>   │
├──────────────────────────┤
│ - _menu: Menu            │
│ - _menuId: string        │
├──────────────────────────┤
│ + Root(TNode)            │
│ + Node(TNode, Action)    │
│ + Build(): Menu          │
└────────┬─────────────────┘
         │ creates
         ▼
┌──────────────────────────┐
│ NodeBuilder<TNode>       │
├──────────────────────────┤
│ - _node: MenuNode        │
│ - _menu: Menu            │
├──────────────────────────┤
│ + Message(string)        │
│ + Option(string, string) │
│ + GoTo(TNode)            │
│ + Action<THandler>()     │
│ + Input()                │
│ + OptionList<T>(...)     │
└──────────────────────────┘
```

---

## 8.5 Request/Response Flow Diagram

```
┌──────────────┐
│ HTTP Request │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│UssdRequestDto│
├──────────────┤
│ + SessionID  │
│ + UserID     │
│ + Msisdn     │
│ + NewSession │
│ + UserData   │
│ + Network    │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│  UssdApp     │
│ HandleRequest│
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ UssdContext  │
├──────────────┤
│ + Request    │
│ + Session    │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│IActionHandler│
│ HandleAsync  │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│UssdStepResult│
├──────────────┤
│ + Message    │
│ + Continue   │
│ + NextStep   │
│ + GoHome     │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│UssdResponseDto│
├──────────────┤
│ + SessionID  │
│ + UserID     │
│ + Msisdn     │
│ + Message    │
│ + Continue   │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ HTTP Response│
└──────────────┘
```

---

# **9. Key Architectural Decisions**

## 9.1 Enum-Based Navigation

**Decision**: Use enums instead of strings for menu node IDs.

**Rationale**:
- Compile-time safety prevents typos
- IDE autocomplete support
- Refactoring-friendly
- Self-documenting code

**Trade-off**: Requires defining enums upfront, less dynamic than strings.

---

## 9.2 Strongly-Typed Session Keys

**Decision**: Use `SessionKey<T>` instead of string keys.

**Rationale**:
- Type safety prevents runtime errors
- Clear data contracts
- Better IntelliSense support
- Prevents type mismatches

**Trade-off**: Slightly more verbose than plain strings.

---

## 9.3 Attribute-Based Auto-Discovery

**Decision**: Use `[UssdAction]` attribute for handler registration.

**Rationale**:
- Reduces boilerplate code
- Convention over configuration
- Easy to add new handlers
- Centralized registration logic

**Trade-off**: Uses reflection (minimal performance impact at startup).

---

## 9.4 Scoped UssdApp Lifetime

**Decision**: Register `UssdApp` as scoped, not singleton.

**Rationale**:
- Ensures clean state per request
- Prevents concurrency issues
- Aligns with ASP.NET Core patterns
- Allows request-scoped dependencies

**Trade-off**: Slight overhead creating instance per request.

---

## 9.5 Immutable DTOs

**Decision**: Use `init` properties for DTOs.

**Rationale**:
- Prevents accidental mutations
- Thread-safe by design
- Clear intent (data transfer only)
- Aligns with functional programming principles

**Trade-off**: Requires creating new instances for modifications.

---

# **10. Performance Considerations**

## 10.1 Session Store Performance

- **MemorySessionStore**: O(1) lookups, limited to single server
- **RedisSessionStore**: Network latency, supports distributed scenarios
- **Recommendation**: Use Redis for production multi-server deployments

## 10.2 Menu Rendering

- Menu structure is built once at startup (singleton)
- Node lookups are O(1) dictionary operations
- Pagination is applied lazily only when needed

## 10.3 Handler Resolution

- Handlers registered at startup via DI
- Resolution is O(1) from DI container
- Auto-discovery uses reflection only at startup

---

# **11. Testing Strategy**

## 11.1 Unit Testing

Test individual components in isolation:

```csharp
[Fact]
public async Task BalanceCheckHandler_ReturnsCorrectBalance()
{
    // Arrange
    var handler = new BalanceCheckHandler();
    var context = CreateTestContext();

    // Act
    var result = await handler.HandleAsync(context);

    // Assert
    Assert.False(result.ContinueSession);
    Assert.Contains("balance", result.Message);
}
```

## 11.2 Integration Testing

Test complete request flows:

```csharp
[Fact]
public async Task UssdApp_ProcessesTransferFlow()
{
    // Arrange
    var ussdApp = CreateUssdApp();

    // Act - Step 1: Start session
    var response1 = await ussdApp.HandleRequestAsync(new UssdRequestDto
    {
        SessionID = "test-123",
        NewSession = true,
        UserData = ""
    });

    // Act - Step 2: Select transfer
    var response2 = await ussdApp.HandleRequestAsync(new UssdRequestDto
    {
        SessionID = "test-123",
        NewSession = false,
        UserData = "2"
    });

    // Assert
    Assert.True(response2.ContinueSession);
    Assert.Contains("recipient", response2.Message);
}
```

---

# **12. Summary**

The OpenUSSD SDK is architected with the following key characteristics:

1. **Layered Architecture**: Clear separation between application, core, and infrastructure layers
2. **Type Safety**: Extensive use of generics, enums, and phantom types
3. **Extensibility**: Plugin architecture for session stores and handlers
4. **Design Patterns**: Builder, Strategy, Template Method, Repository, DI
5. **Convention over Configuration**: Attribute-based auto-discovery
6. **Testability**: Dependency injection and interface-based design
7. **Performance**: Efficient lookups, lazy evaluation, singleton menu structure
8. **Developer Experience**: Fluent APIs, IntelliSense support, compile-time safety

For usage examples and implementation patterns, see:
- [Sample Project Documentation](sample.md)
- [Sample Project README](../Bobcode.Ussd.Arkesel.Sample/README.md)
