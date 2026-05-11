# Project Setup Notes

## What was added

### `Assets/Scripts/PlayerController2D.cs`
Basic 2D platformer controller:
- Left/right movement
- Jumping when grounded
- Ground detection with `groundCheck` + `groundLayer`
- Sprite facing direction using `SpriteRenderer.flipX`
- Input support for both:
  - New Input System (`A/D`, arrows, `Space`, `W`, up arrow)
  - Legacy Input Manager fallback

### `Assets/Scripts/SimpleShapeSprite.cs`
Simple temporary player visuals without art files:
- Generates a square texture in code
- Creates a sprite at runtime
- Applies it to `SpriteRenderer`

### `Assets/Scripts/CameraFollow2D.cs`
Basic smooth camera follow:
- Follows a target transform (player)
- Uses configurable offset
- Uses smooth damping in `LateUpdate`

---

## How each script works

### `PlayerController2D`
- `Update()`
  - Reads horizontal input
  - Checks if grounded using `Physics2D.OverlapCircle`
  - Applies jump velocity when jump key is pressed and grounded
- `FixedUpdate()`
  - Calculates target horizontal speed
  - Accelerates/decelerates toward target speed
  - Clamps max horizontal speed
  - Flips sprite left/right
- `OnDrawGizmosSelected()`
  - Draws ground-check radius in Scene view for debugging

### `SimpleShapeSprite`
- `Awake()`
  - Creates a `Texture2D`
  - Fills all pixels with one color
  - Converts texture to a sprite and assigns it

### `CameraFollow2D`
- `LateUpdate()`
  - Reads target position + offset
  - Smoothly moves camera toward that position

---

## Unity scene setup checklist

1. Create `Player` object.
2. Add components:
   - `SpriteRenderer`
   - `Rigidbody2D` (Dynamic)
   - `BoxCollider2D`
   - `PlayerController2D`
   - `SimpleShapeSprite`
3. Create child object `GroundCheck` under player and place at feet.
4. Assign `GroundCheck` to `groundCheck` in `PlayerController2D`.
5. Create ground object(s) with `BoxCollider2D`.
6. Put ground on a `Ground` layer and assign that layer in `groundLayer`.
7. Add `CameraFollow2D` to `Main Camera` and assign `Player` as target.

---

## Notes

- If movement input does not work, check **Project Settings > Player > Active Input Handling**.
- `PlayerController2D` currently supports both input systems at compile time.
- Keep `Main Camera` offset `z = -10` for 2D rendering.

---

## Combat upgrade

The player controller now includes a movement-integrated combat state machine.

See `Documentation/COMBAT_SYSTEM.md` for:
- attack definition setup
- combo windows and attack buffering
- dash cancel and recovery windows
- air combat bounce behavior
- resource and tool integration
