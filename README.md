# TicketInventory

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      API Layer                             │
│  (Controllers/Endpoints, DTOs, Validators, Middleware)    │
│  Composition root: wires up Infrastructure's DI here      │
│         ↓                                                 │
│                   Application Layer                        │
│  (Commands, Queries, Handlers, Application Services)      │
│         ↓                                                 │
│                    Domain Layer                            │
│  (Entities, Value Objects, Domain Events, Interfaces)     │
│         ↑                                                 │
│               Infrastructure Layer                         │
│  (Repositories, DbContext, External Services)             │
└─────────────────────────────────────────────────────────────┘
```

## Layers
- **Api**
- **Domain**
- **Infrastructure**
- **Tests**

## Dependency Flow
- API → Domain (Services live in Domain; no separate Application project)
- API → Infrastructure (composition root only — Program.cs wiring, no business logic calls this directly)
- Infrastructure → Domain (implements its interfaces)
- Tests → All layers

## Setup
```bash
dotnet restore
dotnet build
cd TicketInventory.Api
dotnet run
```

## Tests
```bash
dotnet test
```
