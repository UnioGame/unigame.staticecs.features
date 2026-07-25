# Effects

## Capabilities

Effects provide pending and active state, delayed activation, stacking, periodic
ticks, expiry, source back-references, and cleanup. The standard asset composes
heal-over-time, stun, and speed modification.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | `EcsTime`, matching handler/configuration | core registries and standard effect resources |
| Features | Characteristics; Stun for `StunEffect` | effect data, events, and tick systems |

## Usage

Add Time, Characteristics, and Stun before `EffectsFeatureAsset`.

Ordinary effect markers, summaries, and links are auto-registered. Typed effect
components and lifecycle events are closed generic constructions and are not.
The Main assembly registrar installs the standard effects. Isolated worlds
register only the effect markers they exercise:

```csharp
EffectTypeRegistration.Register<TWorld, TestEffect>(types);
```

Resources, systems, groups, assets, converters, open generics, abstract types,
and disabled feature assemblies are not auto-registered.

## Configuration

Each effect uses `EffectConfig<TWorld, TEffect>` and
`IEffectHandler<TWorld, TEffect>` resources. The default Update order is `200`.
Features publish resources directly; parameterless systems read them directly.
Asynchronous resource waits use `Resource<T>.GetAsync(lifeTime)` with a 5-second
Editor timeout and a 10-second Player timeout.
