# Missing `Move(...)` Symbols — Where They Are Defined

This is a project-wide lookup for the symbols you listed (movement/combat only), with:

- exact file paths
- required `using`/namespace info
- code blocks for the definitions

- `rb2d`
- `cState`
- `extraAirMoveVelocities`
- `DecayingVelocity`
- `move_input`
- `GetWalkSpeed()`, `GetRunSpeed()`, `SetState(...)`, `ActorStates`
- plus required imports (`UnityEngine`, `System` note)

---

## 1) `rb2d` (movement physics)

- **Definition:** `private Rigidbody2D rb2d;`
- **Defined in:** [`Assembly-CSharp/HeroController.cs#L12005`](../Assembly-CSharp/HeroController.cs#L12005)
- **Type source:** Unity (`Rigidbody2D` from `UnityEngine`)
- **Import:** `using UnityEngine;` (in `Assembly-CSharp/HeroController.cs`)

```csharp
// Assembly-CSharp/HeroController.cs
private Rigidbody2D rb2d;
```

### Main usage in `Move(...)`
- [`HeroController.cs#L2832`](../Assembly-CSharp/HeroController.cs#L2832) `Vector2 vector = this.rb2d.linearVelocity;`
- [`HeroController.cs#L2887`](../Assembly-CSharp/HeroController.cs#L2887) `this.rb2d.linearVelocity = vector;`

```csharp
// Assembly-CSharp/HeroController.cs
Vector2 vector = this.rb2d.linearVelocity;
// ... modify vector.x
this.rb2d.linearVelocity = vector;
```

---

## 2) `cState` (movement/combat state flags)

- **Field definition in HeroController:** `public HeroControllerStates cState;`
  - [`Assembly-CSharp/HeroController.cs#L12023`](../Assembly-CSharp/HeroController.cs#L12023)
- **Initialization:** `this.cState = new HeroControllerStates();`
  - [`Assembly-CSharp/HeroController.cs#L10638`](../Assembly-CSharp/HeroController.cs#L10638)

### Type definition
- **Class:** `public class HeroControllerStates`
- **Defined in:** [`Assembly-CSharp/HeroControllerStates.cs#L8`](../Assembly-CSharp/HeroControllerStates.cs#L8)
- **Namespace:** (global)
- **Import needed in new file:** `using UnityEngine;` (uses `Debug`, `SerializeField`, etc.)

```csharp
// Assembly-CSharp/HeroController.cs
public HeroControllerStates cState;
```

```csharp
// Assembly-CSharp/HeroController.cs (init)
this.cState = new HeroControllerStates();
```

```csharp
// Assembly-CSharp/HeroControllerStates.cs
public class HeroControllerStates
{
    public bool facingRight;
    public bool onGround;
    public bool jumping;
    public bool falling;
    public bool dashing;
    public bool wallSliding;
    public bool attacking;
    public bool downAttacking;
    public bool downSpikeRecovery;
    public bool isTouchingSlopeLeft;
    public bool isTouchingSlopeRight;
    public bool inWalkZone;
    // ...many more flags
}
```

### Movement/combat flags used by `Move(...)`
- `onGround` [`HeroControllerStates.cs#L168`](../Assembly-CSharp/HeroControllerStates.cs#L168)
- `downSpikeRecovery` (field exists in `HeroControllerStates`)
- `isTouchingSlopeLeft` / `isTouchingSlopeRight` (fields exist in `HeroControllerStates`)
- `wallSliding` [`HeroControllerStates.cs#L225`](../Assembly-CSharp/HeroControllerStates.cs#L225)
- `inWalkZone` (field exists in `HeroControllerStates`)
- `facingRight` [`HeroControllerStates.cs#L165`](../Assembly-CSharp/HeroControllerStates.cs#L165)

---

## 3) `extraAirMoveVelocities` (movement)

- **Definition:** `private readonly List<HeroController.DecayingVelocity> extraAirMoveVelocities = new List<HeroController.DecayingVelocity>();`
- **Defined in:** [`Assembly-CSharp/HeroController.cs#L12890`](../Assembly-CSharp/HeroController.cs#L12890)
- **Import needed in new file:** `using System.Collections.Generic;`

```csharp
// Assembly-CSharp/HeroController.cs
private readonly List<HeroController.DecayingVelocity> extraAirMoveVelocities = new List<HeroController.DecayingVelocity>();
```

### Main usage in `Move(...)`
- foreach loop start: [`HeroController.cs#L2843`](../Assembly-CSharp/HeroController.cs#L2843)

```csharp
// Assembly-CSharp/HeroController.cs
foreach (HeroController.DecayingVelocity decayingVelocity in this.extraAirMoveVelocities)
{
    // SkipBehaviour checks...
    vector += decayingVelocity.Velocity;
}
```

---

## 4) `DecayingVelocity` (movement)

- **Definition:** `public struct DecayingVelocity`
- **Defined in:** [`Assembly-CSharp/HeroController.cs#L13151`](../Assembly-CSharp/HeroController.cs#L13151)
- **Namespace:** `HeroController` (nested)

```csharp
// Assembly-CSharp/HeroController.cs
public struct DecayingVelocity
{
    public Vector2 Velocity;
    public float Decay;
    public bool CancelOnTurn;
    public HeroController.DecayingVelocity.SkipBehaviours SkipBehaviour;

    public enum SkipBehaviours
    {
        None,
        WhileMoving,
        WhileMovingForward,
        WhileMovingBackward
    }
}
```

### Members
- `Velocity` [`HeroController.cs#L13154`](../Assembly-CSharp/HeroController.cs#L13154)
- `Decay` [`HeroController.cs#L13157`](../Assembly-CSharp/HeroController.cs#L13157)
- `CancelOnTurn` [`HeroController.cs#L13160`](../Assembly-CSharp/HeroController.cs#L13160)
- `SkipBehaviour` [`HeroController.cs#L13162`](../Assembly-CSharp/HeroController.cs#L13162)

### Nested enum
- `public enum SkipBehaviours` [`HeroController.cs#L13166`](../Assembly-CSharp/HeroController.cs#L13166)
  - `None`
  - `WhileMoving`
  - `WhileMovingForward`
  - `WhileMovingBackward`

### Where else used (project-wide)
- `HeroWaterController` adds velocities
- `SlideSurface` adds velocities
- PlayMaker actions:
  - `HeroAddExtraAirMoveVelocity.cs`
  - `HeroAddExtraAirMoveVelocityV2.cs`

---

## 5) `move_input` (movement input cache)

- **Definition:** `public float move_input;`
- **Defined in:** [`Assembly-CSharp/HeroController.cs#L11615`](../Assembly-CSharp/HeroController.cs#L11615)
- **Import needed in new file:** none (built-in float)

```csharp
// Assembly-CSharp/HeroController.cs
public float move_input;
```

### Input assignment
- `UpdateMoveInput()` sets it from input axis:
  - [`HeroController.cs#L2253`](../Assembly-CSharp/HeroController.cs#L2253)
  - [`HeroController.cs#L2255`](../Assembly-CSharp/HeroController.cs#L2255)
  - `this.move_input = this.inputHandler.inputActions.MoveVector.Vector.x;`

```csharp
// Assembly-CSharp/HeroController.cs
public void UpdateMoveInput()
{
    this.move_input = this.inputHandler.inputActions.MoveVector.Vector.x;
}
```

### Used in `Move(...)`
- Direction/speed decisions and SkipBehaviour checks:
  - [`HeroController.cs#L2850`](../Assembly-CSharp/HeroController.cs#L2850)
  - [`HeroController.cs#L2858`](../Assembly-CSharp/HeroController.cs#L2858)
  - [`HeroController.cs#L2863`](../Assembly-CSharp/HeroController.cs#L2863)
  - [`HeroController.cs#L2871`](../Assembly-CSharp/HeroController.cs#L2871)
  - [`HeroController.cs#L2876`](../Assembly-CSharp/HeroController.cs#L2876)

```csharp
// Assembly-CSharp/HeroController.cs
if (Math.Abs(this.move_input) > Mathf.Epsilon)
{
    continue;
}
```

---

## 6) `GetWalkSpeed()` / `GetRunSpeed()` (movement)

- **`GetRunSpeed` definition:** [`Assembly-CSharp/HeroController.cs#L10898`](../Assembly-CSharp/HeroController.cs#L10898)
- **`GetWalkSpeed` definition:** [`Assembly-CSharp/HeroController.cs#L10908`](../Assembly-CSharp/HeroController.cs#L10908)
- **Imports needed in new file:** `using UnityEngine;` (uses constants on the class)

```csharp
// Assembly-CSharp/HeroController.cs
public float GetRunSpeed()
{
    if (this.IsUsingQuickening)
    {
        return this.QUICKENING_RUN_SPEED;
    }
    return this.RUN_SPEED;
}

public float GetWalkSpeed()
{
    if (this.IsUsingQuickening)
    {
        return this.QUICKENING_WALK_SPEED;
    }
    return this.WALK_SPEED;
}
```

### Used in `Move(...)`
- Walk speed call: [`HeroController.cs#L2837`](../Assembly-CSharp/HeroController.cs#L2837)
- Run speed call: [`HeroController.cs#L2841`](../Assembly-CSharp/HeroController.cs#L2841)

```csharp
// Assembly-CSharp/HeroController.cs
if (this.cState.inWalkZone && this.cState.onGround)
{
    vector.x = moveDirection * this.GetWalkSpeed();
}
else
{
    vector.x = moveDirection * this.GetRunSpeed();
}
```

---

## 7) `SetState(...)` and `ActorStates` (movement/combat state machine)

### `SetState(...)`
- **Definition:** `private void SetState(ActorStates newState)`
- **Defined in:** [`Assembly-CSharp/HeroController.cs#L8324`](../Assembly-CSharp/HeroController.cs#L8324)
- **Imports needed in new file:** `using GlobalEnums;` (for `ActorStates`)

```csharp
// Assembly-CSharp/HeroController.cs
private void SetState(ActorStates newState)
{
    if (this.hero_state == ActorStates.no_input && !this.CanExitNoInput())
    {
        return;
    }
    switch (newState)
    {
    case ActorStates.grounded:
        newState = ((Mathf.Abs(this.move_input) > Mathf.Epsilon) ? ActorStates.running : ActorStates.idle);
        this.heroBox.HeroBoxNormal();
        break;
    case ActorStates.idle:
    case ActorStates.running:
    case ActorStates.airborne:
        if (!this.cState.wallSliding && !this.cState.wallClinging)
        {
            this.heroBox.HeroBoxNormal();
        }
        break;
    case ActorStates.previous:
        newState = this.prev_hero_state;
        break;
    }
    if (newState != this.hero_state)
    {
        this.prev_hero_state = this.hero_state;
        this.hero_state = newState;
        this.animCtrl.UpdateState(newState);
    }
}
```

### `ActorStates`
- **Enum definition file:** [`Assembly-CSharp/GlobalEnums/ActorStates.cs#L6`](../Assembly-CSharp/GlobalEnums/ActorStates.cs#L6)
- **Namespace:** `GlobalEnums`
- **Import needed in new file:** `using GlobalEnums;`

```csharp
// Assembly-CSharp/GlobalEnums/ActorStates.cs
namespace GlobalEnums
{
    public enum ActorStates
    {
        grounded,
        idle,
        running,
        airborne,
        wall_sliding,
        hard_landing,
        dash_landing,
        no_input,
        previous
    }
}
```

### Used in `Move(...)`
- [`HeroController.cs#L2818`](../Assembly-CSharp/HeroController.cs#L2818) `this.SetState(ActorStates.grounded);`

```csharp
// Assembly-CSharp/HeroController.cs
if (this.cState.onGround)
{
    this.SetState(ActorStates.grounded);
}
```

---

## 8) Imports needed for extracted `Move(...)` block

If you copy only the `Move(...)` block into another file, you need at least:

- `using UnityEngine;` (for `Rigidbody2D`, `Vector2`, `Mathf`)
- `using System;` (for `Math.Abs` in the loop)
- `using System.Collections.Generic;` (for `List<T>`)
- `using GlobalEnums;` (for `ActorStates`)

And the containing class must define/provide:

- `rb2d`
- `cState`
- `extraAirMoveVelocities`
- `move_input`
- `GetWalkSpeed()`
- `GetRunSpeed()`
- `SetState(ActorStates)`
- nested/accessible `DecayingVelocity`

---

## 9) Quick “owner” map

| Symbol | Owner type | Definition |
|---|---|---|
| `rb2d` | `HeroController` field | [`HeroController.cs#L12005`](../Assembly-CSharp/HeroController.cs#L12005) |
| `cState` | `HeroController` field (`HeroControllerStates`) | [`HeroController.cs#L12023`](../Assembly-CSharp/HeroController.cs#L12023) |
| `HeroControllerStates` | class | [`HeroControllerStates.cs#L8`](../Assembly-CSharp/HeroControllerStates.cs#L8) |
| `extraAirMoveVelocities` | `HeroController` field | [`HeroController.cs#L12890`](../Assembly-CSharp/HeroController.cs#L12890) |
| `DecayingVelocity` | nested struct in `HeroController` | [`HeroController.cs#L13151`](../Assembly-CSharp/HeroController.cs#L13151) |
| `move_input` | `HeroController` field | [`HeroController.cs#L11616`](../Assembly-CSharp/HeroController.cs#L11616) |
| `GetRunSpeed()` | `HeroController` method | [`HeroController.cs#L10898`](../Assembly-CSharp/HeroController.cs#L10898) |
| `GetWalkSpeed()` | `HeroController` method | [`HeroController.cs#L10908`](../Assembly-CSharp/HeroController.cs#L10908) |
| `SetState(ActorStates)` | `HeroController` method | [`HeroController.cs#L8324`](../Assembly-CSharp/HeroController.cs#L8324) |
| `ActorStates` | global enum | [`GlobalEnums/ActorStates.cs#L6`](../Assembly-CSharp/GlobalEnums/ActorStates.cs#L6) |

---

If you want, I can generate a **minimal compilable standalone `Move` demo class** that includes just enough stubs to compile these symbols cleanly.