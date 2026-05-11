# 2D Action Combat System

This project now includes a modular combat architecture inspired by high-mobility action combat.

## Added Scripts

- `Assets/Scripts/Combat/PlayerState2D.cs`
  - State machine enums: `Idle`, `Move`, `Jump`, `Attack`, `AirAttack`, `Dash`, `Recover`
  - Attack categories: `Ground`, `Air`, `Downward`, `Special`

- `Assets/Scripts/Combat/AttackDefinition2D.cs`
  - ScriptableObject-driven attack data
  - Contains startup/active/recovery, cancel windows, velocity effect, hitbox, damage, combo window, and resource values

- `Assets/Scripts/Combat/PlayerCombatResource2D.cs`
  - Silk-like resource meter
  - Gain resource on hit, spend for specials/tools, consume full bar for healing flow

- `Assets/Scripts/Combat/IDamageable.cs`
  - Interface for enemy damage reception

- `Assets/Scripts/Combat/ICombatToolAbility.cs`
  - Interface for equippable combat tools/abilities
  - Contract:
    - `int ResourceCost { get; }`
    - `bool TryUse(PlayerController2D controller)`

- `Assets/Scripts/PlayerController2D.cs` (upgraded)
  - Integrated movement + combat state machine
  - Combo chaining with combo timer
  - Attack buffering
  - Dash cancel during recovery cancel windows
  - Air-focused attacks including downward bounce behavior
  - Hitbox-based damage and resource gain
  - Runtime hitbox square debug drawing during active attack frames

- `Assets/Scripts/Combat/TestDamageDummy2D.cs`
  - Test target that implements `IDamageable`
  - Receives damage, flashes on hit, optional knockback, and can be destroyed on death

- `Assets/Scripts/Combat/Tools/PlaceholderToolAbility2D.cs`
  - Example tool ability implementing `ICombatToolAbility`
  - Consumes resource and applies a small movement boost

- `Assets/Scripts/Combat/Debug/CombatHitboxViewer2D.cs`
  - Component that shows equipped attack hitboxes constantly when enabled
  - Toggle key support (default `H`) to show/hide all hitboxes in play mode

- `Assets/Scripts/Combat/Visuals/AttackPlaceholderVfx2D.cs`
  - Spawns a temporary basic shape when an attack starts
  - Useful as a non-hitbox visual placeholder to preview attack feel in play mode

- `Assets/Editor/Combat/CombatPlaceholderGenerator.cs`
  - Editor utility menu: `Tools > Combat > Generate Placeholder Attack Set`
  - Creates placeholder attack assets in `Assets/Data/Combat/Attacks`
  - Auto-assigns generated attacks to `PlayerController2D` when one is found in scene

- `Assets/Scripts/FrameRateSettings.cs`
  - Optional FPS/vsync bootstrap:
    - `useVSync = true` -> sync to monitor (`vSyncCount = 1`)
    - `useVSync = false` -> custom FPS cap using `Application.targetFrameRate`

## Input Defaults

- Move: `A/D` or Arrow keys
- Jump: `Space` / `W` / Up Arrow
- Ground Attack: `C`
- Attack (general/air): `X` (or left mouse)
- Dash: `Left Shift` (or `K`)
- Tool: `E`
- Special: `Q`
- Heal (full resource only): `R`

## Setup Steps

1. Keep `PlayerController2D` on the player.
2. Add required player components:
   - `Rigidbody2D`
   - `Collider2D` (e.g. `BoxCollider2D`)
   - `SpriteRenderer`
   - `PlayerCombatResource2D`
3. Ensure `groundCheck` is assigned and `groundLayer` includes your ground colliders.
4. Create attack assets via:
   - `Create > Combat > Attack Definition 2D`
5. Assign attack assets into:
   - `groundCombo`
   - `airCombo`
   - `specialAttack`
6. Set each attack's:
   - timings (startup/active/recovery)
   - cancel windows
   - velocity effect
   - hitbox size/offset
   - damage and resource values
7. Ensure enemies implement `IDamageable` and are on attack `hittableLayers`.
8. Optional tools:
   - Create `MonoBehaviour` implementing `ICombatToolAbility`
   - Assign instances to `toolAbilityBehaviours`
9. Optional fast setup:
   - Run `Tools > Combat > Generate Placeholder Attack Set`
   - This creates and assigns sample attacks automatically
10. Optional smooth frame cap:
   - Add `FrameRateSettings` to a bootstrap object (or Player)
   - Configure `useVSync` / `targetFps`

## Runtime Hitbox Debug View

`PlayerController2D` includes runtime attack hitbox squares using a `LineRenderer`.

- Toggle in inspector:
  - `showRuntimeHitbox`
  - `runtimeHitboxColor`
  - `runtimeHitboxLineWidth`
- Behavior:
  - Draws only while an attack is in its **active** phase
  - Uses current `AttackDefinition2D.hitboxOffset` and `hitboxSize`
  - Flips horizontally by facing direction

This is intended for combat tuning and hitbox verification during play mode.

For constant always-on previews, use `CombatHitboxViewer2D` on the player.
- It renders all equipped attack hitboxes continuously when enabled.
- Toggle at runtime with `H` (configurable on the component).

## Test Dummy Setup

Use `TestDamageDummy2D` on any target object to validate damage/hitbox behavior.

Recommended components for dummy:
- `Collider2D` (required to be detected by attack overlap checks)
- `SpriteRenderer` (for hit flash feedback)
- `Rigidbody2D` (optional knockback)
- `TestDamageDummy2D`

Then place dummy layer inside each attack asset `hittableLayers`.

## Combat Behavior Notes

- Attacks do not root movement.
- Recovery is designed for cancel decisions (dash/jump/next attack).
- Missing combo timing resets combo index.
- Downward attacks can bounce and refresh air dash if configured.
- Standing still is mechanically weaker than active repositioning.
