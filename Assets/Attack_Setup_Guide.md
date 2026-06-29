# Hollow Knight Style Attack Setup Guide

This guide walks you through setting up frame-perfect, Hollow Knight style combat. In this system, your sword swings, hitboxes, and pogo mechanics are perfectly synced to your animation frames using the Animator Timeline.

---

## 1. Hero Main GameObject Setup

Your main player character (`Hero`) must have the `HeroController` script attached.
Make sure you have created your Attack animation states in the Animator (e.g., `Attack_Forward`, `Attack_Up`, `Attack_Down`).

**For each Attack State in the Animator:**
1. Select the state.
2. Click **Add Behaviour**.
3. Select **`AttackStateBehaviour`**. 
*(This tells your HeroController exactly when the swing starts and ends, ensuring you can still move and jump freely during the attack!)*

---

## 2. Setting Up the Hitbox Child Objects

You need three separate child objects for your hitboxes: Forward, Up, and Down.

1. Right-click your main **Hero** GameObject -> **Create Empty**.
2. Create three empty objects and name them:
   * `MeleeWeapon_Forward`
   * `MeleeWeapon_Up`
   * `MeleeWeapon_Down`
3. Position each one appropriately (in front of the hero, above the hero's head, and below the hero's feet).

**For all THREE child objects, do the following:**
1. Attach a **`PolygonCollider2D`** (or `BoxCollider2D`).
   * **Check** the `Is Trigger` box.
   * **Check** the `PolygonCollider2D` component itself (keep it enabled).
   * Shape the collider to match your sword swing radius.
2. Attach the **`MeleeWeapon`** script.
   * **Uncheck** the `MeleeWeapon` script component at the top of the Inspector. **(It MUST be disabled by default!)**
   * Set your `Damage Amount` and `Knockback Force`.
3. Ensure the object is on your `Attack` Layer (so it hits enemies but not the player).

---

## 3. Linking Hitboxes to the HeroController

The game needs to know which hitbox to use based on your input.

1. Click on your main **Hero** GameObject.
2. Scroll down in the `HeroController` script to the **Combat - Weapons** section.
3. Drag and drop your three child objects into their respective slots:
   * **Forward Weapon:** `MeleeWeapon_Forward`
   * **Up Weapon:** `MeleeWeapon_Up`
   * **Down Weapon:** `MeleeWeapon_Down`

---

## 4. Animating the Hitboxes (Timeline)

To make the combat frame-perfect, we will turn the `MeleeWeapon` script ON and OFF during the actual animation clip.

1. Open the **Animation** window (`Window > Animation > Animation`).
2. Select your **Hero** GameObject in the hierarchy.
3. In the Animation window dropdown, pick one of your attack clips (e.g., your Forward Attack).
4. Hit the **Record** button (red circle).
5. Scrub the timeline to the frame where the sword swing *starts*:
   * In the hierarchy, click your `MeleeWeapon_Forward` object.
   * In the Inspector, **Enable** (Check) the `MeleeWeapon` script component.
6. Scrub to the frame where the sword swing *ends*:
   * **Disable** (Uncheck) the `MeleeWeapon` script component.
7. Stop Recording.
8. **Repeat** this process for your Upward Attack clip (animating the `MeleeWeapon_Up` script) and your Downward Attack clip (animating the `MeleeWeapon_Down` script).

> [!IMPORTANT]
> Do **NOT** animate the `PolygonCollider2D`. Animate the `MeleeWeapon` script instead. The script will automatically clear its hit list and turn on the collider for you.

---

## 5. Pogo Mechanics (Downward Attack)

The "Pogo" mechanic (bouncing off enemies when attacking downward in the air) is **already fully coded** into your `MeleeWeapon.cs` and `HeroController.cs` scripts!

As long as you have properly set up your `MeleeWeapon_Down` object, linked it in the `HeroController`, and animated its script turning on and off in your Downward Attack animation clip:
* When you jump and hold DOWN + ATTACK, the Downward Hitbox will activate.
* When it hits a `HealthManager` on an enemy or spikes, the `MeleeWeapon.cs` script will automatically tell the `HeroController` to trigger the upward Recoil (Pogo). You don't need to write any extra code!
