# Combat Quick Reference (Clickable)

Use this file as a fast index to jump to combat settings and code.

---

## Core Attack Data (size, position, timing)

### Attack definition script
- [`Assets/Scripts/Combat/AttackDefinition2D.cs`](../Assets/Scripts/Combat/AttackDefinition2D.cs)

Important fields in `AttackDefinition2D`:
- `startupTime`, `activeTime`, `recoveryTime`
- `cancelWindowStart`, `cancelWindowEnd`
- `comboWindow`
- `velocityEffect`
- `damage`
- `hitboxOffset` (**position**)
- `hitboxSize` (**size**)
- `hittableLayers`
- `bounceOnHit`, `bounceVelocity`
- `resourceCost`, `resourceGainOnHit`

---

## Actual attack assets you edit in Inspector

- Basic attack: [`Assets/Data/Combat/Attacks/BasicSlash.asset`](../Assets/Data/Combat/Attacks/BasicSlash.asset)
- Ground combo 2: [`Assets/Data/Combat/Attacks/Thrust.asset`](../Assets/Data/Combat/Attacks/Thrust.asset)
- Air down attack: [`Assets/Data/Combat/Attacks/DownwardStrike.asset`](../Assets/Data/Combat/Attacks/DownwardStrike.asset)
- Special attack: [`Assets/Data/Combat/Attacks/SpinAttack.asset`](../Assets/Data/Combat/Attacks/SpinAttack.asset)

Edit these values inside each asset:
- `hitboxOffset` (where attack appears)
- `hitboxSize` (how big the attack box is)
- `damage`
- timing values

---

## Where attacks are used and selected

- Player combat controller: [`Assets/Scripts/PlayerController2D.cs`](../Assets/Scripts/PlayerController2D.cs)

Main functions to check in that file:
- `SelectNextAttack()` ? picks ground/air/downward attack
- `TryStartAttack(...)` ? starts attack and applies velocity
- `ProcessHitbox()` ? does collision check and damage
- `UpdateAttackPhases()` ? startup/active/recovery flow
- `UpdateFacingDirection()` ? left/right direction

Serialized fields to assign in Inspector:
- `groundCombo`
- `airCombo`
- `specialAttack`
- `attackOrigin`

---

## Input keys (where to change)

- File: [`Assets/Scripts/PlayerController2D.cs`](../Assets/Scripts/PlayerController2D.cs)

Functions:
- `ReadAttackPressedThisFrame()`
- `ReadDashPressedThisFrame()`
- `ReadToolPressedThisFrame()`
- `ReadSpecialPressedThisFrame()`
- `ReadJumpPressedThisFrame()`

---

## Hitbox visualization / debug

- Constant viewer component: [`Assets/Scripts/Combat/Debug/CombatHitboxViewer2D.cs`](../Assets/Scripts/Combat/Debug/CombatHitboxViewer2D.cs)
- Runtime active hitbox (inside player): [`Assets/Scripts/PlayerController2D.cs`](../Assets/Scripts/PlayerController2D.cs)

Viewer options:
- `showHitboxes`
- `toggleKey` / `toggleKeyInputSystem`
- colors and line width

---

## Attack placeholder visual (shape on attack)

- File: [`Assets/Scripts/Combat/Visuals/AttackPlaceholderVfx2D.cs`](../Assets/Scripts/Combat/Visuals/AttackPlaceholderVfx2D.cs)

Main method:
- `PlayAttackShape(...)` ? spawns temporary shape using attack size/position data

---

## Test target (damage receiver)

- File: [`Assets/Scripts/Combat/TestDamageDummy2D.cs`](../Assets/Scripts/Combat/TestDamageDummy2D.cs)
- Interface: [`Assets/Scripts/Combat/IDamageable.cs`](../Assets/Scripts/Combat/IDamageable.cs)

---

## Tool ability placeholder

- Interface: [`Assets/Scripts/Combat/ICombatToolAbility.cs`](../Assets/Scripts/Combat/ICombatToolAbility.cs)
- Example component: [`Assets/Scripts/Combat/Tools/PlaceholderToolAbility2D.cs`](../Assets/Scripts/Combat/Tools/PlaceholderToolAbility2D.cs)

---

## Generator utility (auto create attack assets)

- File: [`Assets/Editor/Combat/CombatPlaceholderGenerator.cs`](../Assets/Editor/Combat/CombatPlaceholderGenerator.cs)
- Menu: `Tools > Combat > Generate Placeholder Attack Set`
