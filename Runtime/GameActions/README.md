# Game Actions Feature

## Capabilities

Registers game-action data and the action-mask maintenance system.

## Usage

Add `GameActionsFeatureAsset` when ability or gameplay code uses action availability masks.

## Configuration

System registration is asynchronous and owned by the feature. Execution order is configured on the system entry, independently from feature startup order.
