# Damage Feature

## Capabilities

Provides damage and healing events, an ordered filter chain, damage application, combat characteristic integration, and `DeathPendingTag` emission.

## Usage

Add `DamageFeatureAsset` after characteristics, then raise requests through `DamageOperations.RaiseDamage` or `RaiseHealing`.

## Configuration

The default chain uses dodge, block, armor, critical, and shield data. Register a custom `IDamageRng` or filter chain before the feature when deterministic or game-specific behavior is required.
