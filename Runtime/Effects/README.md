# Effects Feature

## Capabilities

Provides timed effect storage, stacking, ticking, source cleanup, healing-over-time, stun, and characteristic modification effects.

## Usage

Add `EffectsFeatureAsset` after time, characteristics, stun, and damage. Apply or remove effects through `EffectOperations` and the concrete effect helpers.

## Configuration

Every effect type has an explicit handler and flag. `EcsTimeFeatureAsset` must precede this feature because ticking reads the time resource. Effect handlers that bridge to other families require those families to be registered first.
