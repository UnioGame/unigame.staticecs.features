# Damage

## Capabilities

Damage processes incoming damage through dodge, block, armor, critical, and
shield filters. It reports both the filtered requested amount and the actual
health delta after clamping.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | characteristic data | `DamageConfig`, `IDamageRng`, `DamageFilterChain<TWorld>` |
| Features | Characteristics | damage tags, events, and apply system |

`DamageOperations.Resolve<TWorld>(in IncomingDamageEvent)` runs the filter chain and applies the result at once, returning a
`DamageResolution` (cancel reason, critical flag, applied amount, killing blow); `ApplyDamageSystem` calls it for every
queued `IncomingDamageEvent`. Use it when the caller needs the outcome in the same tick.
`DamageOperations.RollCritical<TWorld>(source)` rolls the critical chance of a source once; set
`IncomingDamageEvent.CriticalRolled` (with `ForceCritical` as the result) so the critical filter does not roll again, which
lets one roll cover many targets.

`IncomingDamageEvent.AttackType` says how the damage is delivered, like `DamageType` says what it is. `Hit`, `Projectile`
and `AreaHit` run every hit check (dodge, block, critical); `Area` skips them, which suits the tick of a running status such
as poison. Armor and shield do not depend on it. The rule lives in `AttackTypeRules.RollsHitChecks`; the default `Hit` keeps
the behavior of an event that sets nothing. `AreaHit` lets an area or periodic effect roll like a strike, for example a
damage-over-time that can be dodged.

## Usage

Add Characteristics before Damage:

```csharp
var asset = ScriptableObject.CreateInstance<DamageFeatureAsset>();
asset.registerApplySystem = true;
asset.registerDefaultChain = true;
asset.applyOrder = DamageFeature.DefaultApplyOrder;
```

Concrete damage tags and events are auto-registered. Resources, systems, groups,
assets, converters, open and closed generic constructions, abstract types, and
disabled feature assemblies are not. Damage owns no closed-generic registrar.

## Configuration

The default Update order is `100`. Publish a custom RNG, configuration, or filter
chain with `SetResource`; systems read it with `GetResource<T>()`. Features await
asynchronous resources through `Resource<T>.GetAsync(lifeTime)` for up to 5
seconds in the Editor or 10 seconds in a Player.
