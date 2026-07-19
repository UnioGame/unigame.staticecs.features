# Ability Feature

## Capabilities

Provides data-driven ability casts, step trees, wait/progression systems, AoE integration, effect dispatch, and ScriptableObject authoring. Ability Graph editor code is isolated in a family-local Editor-only assembly.

## Usage

Add `AbilityFeatureAsset` after its shared data dependencies. Register authored `AbilityDatabase` content from a game feature or register code-defined definitions directly in `AbilityRegistry<TWorld>`.

## Configuration

Time is required for waits, target selection for AoE steps, effects for effect dispatch, and game actions for action gating used by the game composition. Custom closed generic steps, handlers, and registries are registered explicitly.
