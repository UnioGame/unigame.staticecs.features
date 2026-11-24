# Effects

## Capabilities

Effects provide pending and active state, delayed activation, stacking, periodic
ticks, expiry, source back-references, and cleanup. The standard asset composes
heal-over-time, stun, immobilize, dash, and speed modification.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | `EcsTime`, matching handler/configuration | core registries and standard effect resources |
| Features | Characteristics; Stun for `StunEffect`; Immobilize for `ImmobilizeEffect` | effect data, events, and tick systems |

## Instant effects

An effect configured as `EffectDurationMode.Instant` has no lifetime. `EffectOperations.Apply` takes it through the
same path as any effect: it calls `OnApplied` and raises `EffectAppliedEvent`, then, as nothing is left of it, ends at
once (`OnRemoved` with `expired` true and `EffectRemovedEvent`). A new effect that is already spent is not written to the
target at all, so no `EffectComponent` or roster entry exists even for the length of the call. It never ticks, takes no
conflict group and keeps no back-reference to its source. Its closed
types (`EffectComponent<T>`, `PendingEffectComponent<T>` and the lifecycle events) are registered like those of any
effect. It shares the id registry, the configuration resource and `EffectOperations.Apply` with timed effects, so a
project applies both kinds the same way:

```csharp
var config = EffectConfig<TWorld, TEffect>.CreateInstant();
World<TWorld>.SetResource(config);
```

`Apply` returns an `EffectApplyResult`: `Applied`, `Refreshed`, `ConflictRejected`, `Rejected` (the handler refused
the target) or `Failed` (the target is gone or the parameters resolve to an empty effect). The package knows
nothing about requests, areas or positions; the caller resolves the target entity and passes the strength in
`EffectApplyParams.Magnitude`.

## Handler contract

`IEffectHandler<TWorld,TEffect>.CanApply` runs before anything changes, including conflict resolution, and
`OnApplied` receives an `EffectApplyContext` with the target, source, stacks, previous stacks, magnitude and source
name. An instant effect gets `CanApply`, `OnApplied` and `OnRemoved` in one call, never `OnTick`. It declares no `[EffectFlag]`.

`LinkedFeatures/` holds the state families that effects drive but other features also read (`Stun`, `Immobilize`): each keeps its own
asmdef, feature and tests, so GameActions, Ability or Aiming can use the state without depending on the effects.

## Movement-control effects

`ImmobilizeEffect` follows the Stun split: the state lives in `LinkedFeatures/Immobilize/` (multi-source counter, `ImmobilizeActiveTag`,
`ImmobilizeChangedEvent`, `ImmobilizeFeature`) and the effect only adds and removes one source entry. The entry is keyed by the
target itself, not by the applier, so a reapply from another source cannot leave an entry behind on expiry. The package decides
nothing about what an immobilized entity cannot do; the project movement reads the tag.

`DashEffect` keeps its state in the effect folder. The effect parameters have no direction, so the caller sets a
`MovementEffectComponent` with `MovementEffectOperations.Set` right before `EffectOperations.Apply`; the handler consumes it,
takes the distance from `EffectApplyParams.Magnitude` and the time from the effect duration, and writes a `DashComponent`. The movement of the project
removes that component once it has taken the dash over. A dash that is still running rejects a second one
(`EffectApplyResult.Rejected`). `EffectFlag.CrowdControl` is `Stun | Immobilize`; a dash is not crowd control.

`EffectFlag` also reserves `ProtectionNegative` and `ProtectionAll` for project-side protection statuses. The package gives
them no meaning: the project decides what a protection blocks and what a cleanse removes (`RemoveByMask` / `RemoveAll`).
`SigilOfRighteous` is reserved the same way for a project status whose charges the game spends itself.

`EffectOperations.TrySpendCharge<TWorld, TEffect>` spends one charge of a `Charges` effect on demand, for charges the game
uses rather than the tick system; the effect ends as expired when the last one is spent.

## Usage

Add Time, Characteristics, and Stun before `EffectsFeatureAsset`; the asset installs `ImmobilizeFeature` itself.

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
