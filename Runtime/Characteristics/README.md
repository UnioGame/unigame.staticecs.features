# Characteristics Feature

## Capabilities

Registers characteristic values, modifiers and source back-references, plus the standard health, shield, mana, and speed families. The feature owns mana regeneration system registration and provides Mono, asset, and inline serializable authoring variants.

## Usage

Add a `CharacteristicsFeatureAsset` before features that read combat or movement characteristics. Use `CharacteristicOperations` and modifier extensions for runtime changes. Prefer `AllCharacteristicsSerializableConverter`, an individual `*SerializableConverter`, or `ManaRegenSerializableConverter` for new inline provider configuration and reusable presets.

## Configuration

Closed generic characteristic components are registered explicitly. Custom characteristic marker types require their matching `CharacteristicFeature<TWorld, TStat>` registration and a Main-default alias when exposed publicly. Existing Mono converters remain supported for compatibility and component-oriented authoring.
