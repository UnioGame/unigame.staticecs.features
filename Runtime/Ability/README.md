# Ability

## Capabilities

Ability executes authored step graphs with waits, damage, effects, AoE selection,
conditions, repeats, and parallel branches. Cancellation recursively terminates
branch casts and emits one root completion event.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | Time and resources used by selected leaves | `AbilityConfig`, registries, activators, RNG |
| Features | Damage, Effects, Target Selection as used | cast data, lifecycle events, update systems |

## Usage

Add `AbilityFeatureAsset`, followed by `AbilityDatabaseFeatureAsset`:

```csharp
var ability = ScriptableObject.CreateInstance<AbilityFeatureAsset>();
ability.registerSystems = true;

var database = ScriptableObject.CreateInstance<AbilityDatabaseFeatureAsset>();
database.database = authoredDatabase;
```

Concrete ability components, tags, events, and multi markers are discovered
automatically. Resources, systems, groups, assets, converters, open generics,
required closed generic constructions, abstract types, and disabled feature
assemblies are not. Built-in Ability needs no closed-generic registrar.

## Configuration

`AbilityConfig` defaults to cast order `150`, wait order `155`, and progression
order `160`. Database initialization requires the Update group. Features publish
resources with `SetResource`; parameterless systems read them with
`GetResource<T>()`.

Use `Resource<T>.GetAsync(lifeTime)` for asynchronous startup dependencies.
Waiting defaults to 5 seconds in the Editor and 10 seconds in a Player.
Initialization-only services can access `StaticEcsContext.Get()`; systems should
consume typed ECS Resources.
