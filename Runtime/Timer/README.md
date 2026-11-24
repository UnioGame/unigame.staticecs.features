# Timer and Cooldown

## Capabilities

`TimerFeature<TWorld>` runs two independent entity-owned counters. `Cooldown`
counts down from a duration; `Timer` counts elapsed time up. Both can coexist on
one entity. Cooldown supports automatic restart and optional entity destruction.
Timer can finish at an optional limit.
Removing either source component clears its remaining state and tags on the next update.

See the [shared Static ECS documentation](../../../../../docs/knowledge/static-ecs/).

| Contract | Required | Provided |
|---|---|---|
| Resources | `EcsTime` | None |
| Features | `EcsTimeFeature<TWorld>` and an Update group | Timer state, requests, events, and Update systems |

## Usage

Add `TimerFeature<TWorld>` after `EcsTimeFeature<TWorld>`. Configure a cooldown on
an entity by adding `CooldownComponent` with `DurationMilliseconds`, then send
`RestartCooldownRequest` with that entity's `EntityGID`. Alternatively, use
`CooldownSerializableConverter<TWorld>` for an initially active cooldown.

Read `CooldownRemainingComponent.RemainingMilliseconds` while `CooldownActiveTag` is present. Restart
creates or resets the remaining-time component. A completed
cooldown has `CooldownCompleteTag` and emits `CooldownFinishedEvent` once per run. The
event retains the entity GID even if the destruction option removes the entity
later in the same update.

## Configuration

`CooldownAutoRestartTag` starts the next cooldown run on the following update.
`CooldownDestroyOnCompleteTag` destroys the entity after completion and takes
precedence over automatic restart. Both counters advance by scaled
`EcsTime.DeltaTime` during their update systems, so pause and time scaling apply.
The fractional remainder below one millisecond is carried between updates.
An unsigned millisecond counter covers up to about 49.7 days. Longer unlimited
timers saturate at `uint.MaxValue`.
Requests are handled after the counters advance; a newly started counter begins
counting on the next update. Zero durations complete on the first update
after starting. Automatic restart never catches up missed periods.

For a count-up timer, add `TimerComponent` with optional
`LimitMilliseconds` (`0` means no limit). Call
`TimerOperations.Start<TWorld>(entity.GID)` or send an
`TimerControlRequest` with `Action = TimerAction.Start`.
Other actions are `Pause`, `Reset`, and `Restart`. Pause preserves accumulated
time accumulated through the last update, Start resumes it, Reset clears time
and leaves the timer paused, and
Restart clears time and starts it. A completed timer requires Reset or Restart
before it can start again.

`ElapsedMilliseconds` contains accumulated milliseconds and `TimerActiveTag`
marks a running timer. With no limit, the timer keeps counting until paused or
reset. At a positive limit, elapsed time is clamped, one
`TimerFinishedEvent` is sent with the entity GID, and
`TimerCompleteTag` replaces the active tag.

`TimerSerializableConverter<TWorld>` starts a count-up timer from
Inspector settings (`limitSeconds`), converted once to milliseconds. The client wrapper is
`ClientTimerSerializableConverter`. It uses scaled `EcsTime.DeltaTime`.
The game-level network adapter described in
[the timer guide](../../../../../docs/guides/timers.md) replicates server-owned
timers on network entities; the package itself remains transport-independent.
