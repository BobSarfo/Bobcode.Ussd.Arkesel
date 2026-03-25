# OpenUSSD (NuGet package)

**Package:** [OpenUSSD on NuGet](https://www.nuget.org/packages/OpenUSSD/)

Typed menus, sessions, and action handlers for **USSD** apps on **ASP.NET Core** (.NET 8+).

---

## Session storage (important)

- **`AddUssdSdk(menu, …)`** registers **`MemorySessionStore`** (in-memory, `IMemoryCache`). This is the **default**.
- **Redis is opt-in:** register StackExchange.Redis’s **`IConnectionMultiplexer`**, then use **`AddUssdSdk<RedisSessionStore>(menu, …)`**.

---

## Quick start

Use the repository **root [README.md](../README.md)** for a step-by-step tutorial (install → menu → DI → endpoint → handler).

---

## Deeper docs

| Document | Path |
|----------|------|
| SDK architecture and API | [docs/sdk.md](../docs/sdk.md) |
| Sample walkthrough | [docs/sample.md](../docs/sample.md) |
| Run the demo app | [Bobcode.Ussd.Arkesel.Sample/README.md](../Bobcode.Ussd.Arkesel.Sample/README.md) |

---

## Requirements

- .NET 8 or later  
- ASP.NET Core

## License

MIT — see the [LICENSE](../LICENSE) file in the repository root.
