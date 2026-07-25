# Game Actions

## Capabilities

Game Actions dispatch typed action events and maintain deterministic 32-bit
action masks. Each action receives an explicit stable ID.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | explicit action ID registrations | `GameActionRegistry<TWorld>`, `GameActionsConfig` |
| Features | none | action-mask data and maintenance system |

## Usage

Closed action events are not auto-registered. Declare them in the owning
assembly registrar:

```csharp
types.Event<GameActionEvent<JumpAction>>();
types.Event<GameActionEvent<AttackAction>>();
```

After `GameActionsFeatureAsset` publishes the registry, assign stable IDs during
feature initialization:

```csharp
var registry = World<Main>.GetResource<GameActionRegistry<Main>>();
registry.Register<JumpAction>(0);
registry.Register<AttackAction>(1);
```

Concrete mask components and markers are auto-registered. Resources, systems,
groups, assets, converters, open generics, abstract types, and disabled feature
assemblies are not.

## Configuration

`GameActionsConfig` controls maintenance system installation and order. IDs must
be unique and within `0..31`. Dependent features await
`GameActionRegistry<Main>` through `Resource<T>.GetAsync(lifeTime)`. Waiting
defaults to 5 seconds in the Editor and 10 seconds in a Player.
