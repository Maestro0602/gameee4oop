# Animator-Driven Attack Setup Guide

This guide walks you through setting up the attack system using the standard Animator-driven approach (as seen in Chris Tutorials), where your attack logic and movement locks are tied directly to the animation clip timeline.

---

## 1. Hero Main GameObject Setup

The main player character GameObject (typically named **Hero**) holds the physical collider, Rigidbody2D, animator, and core controller logic.

### Required Components on **Hero**:
1. **Rigidbody2D**:
   - **Gravity Scale**: `0` *(Crucial: custom gravity calculations in `HeroController.cs` override Unity's default gravity for custom air dynamics).*
   - **Collision Detection**: `Continuous`
   - **Constraints**: Freeze Rotation `Z`
2. **Collider2D** (e.g., `BoxCollider2D` or `CapsuleCollider2D`):
   - Serves as the player's physical body.
3. **HeroController**:
   - Script that coordinates movement, jumps, and triggers the animator for attacks.
4. **Animator**:
   - Plays running, jumping, dashing, and attacking animations.

---

## 2. Attack Hitbox Child Object

Since we are animating the hitbox directly on the timeline, you need a pre-set physical trigger object attached to the Hero.

1. Right-click your main **Hero** GameObject -> **Create Empty**. Name it `AttackHitbox`.
2. Attach a **`PolygonCollider2D`** (or `BoxCollider2D`) to this child object.
   - **Check** the `Is Trigger` box.
   - Uncheck the component itself so it is **Disabled by default**.
   - Shape it to match your maximum sword swing radius.
3. Attach the **`DamageEnemies.cs`** script to it.
   - Set the damage values you want it to deal.

---

## 3. The Animator State Machine

We use a `StateMachineBehaviour` to lock the player's movement and track the attack state precisely through the Animator.

1. Open your **Animator** window and select your Attack animation state (e.g., `Hero_Attack`).
2. In the Inspector for that state, click **Add Behaviour**.
3. Select **`AttackStateBehaviour`**.
   - *(This script automatically tells `HeroController.instance.EndAttack()` to unlock movement the exact moment the animation finishes playing).*

---

## 4. Animating the Hitbox (Timeline)

You must manually turn the collider on when the sword swings, and turn it off when the swing is done.

1. Open the **Animation** window (`Window > Animation > Animation`).
2. Select your **Hero** GameObject in the hierarchy.
3. In the Animation window dropdown, pick your attack animation clip.
4. Hit the **Record** button (red circle).
5. Move the timeline scrubber to the frame where the sword first connects.
   - In the hierarchy, click the `AttackHitbox` child object.
   - In the Inspector, **Enable** the `PolygonCollider2D` (check the box).
6. Move the scrubber to the frame where the sword swing ends.
   - **Disable** the `PolygonCollider2D` (uncheck the box).
7. Stop Recording.

> [!TIP]
> This method guarantees frame-perfect combat. If you speed up or slow down the animation clip in the Animator, the hitbox perfectly scales with it.

---

## 5. Collision Layers
To ensure player slashes hit enemies without triggering self-damage:
1. Ensure the `AttackHitbox` child object is on an `Attack` layer.
2. Ensure enemies are on the `Enemy` layer.
3. In **Edit -> Project Settings -> Physics 2D**:
   - Check intersection between `Attack` and `Enemy`.
   - Uncheck intersection between `Attack` and `Player`.
