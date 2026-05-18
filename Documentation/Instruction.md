## this is for instruction to give AI 


You are designing a 2D action combat system inspired by Hollow Knight: Silksong.

The playable character is fast, agile, and attack-focused. Combat emphasizes FLOW, MOBILITY, and COMBO CHAINING rather than slow, single attacks.

Your task is to design and implement a modular attack system with the following properties:

----------------------------------
CORE DESIGN PRINCIPLES
----------------------------------

1. Movement-Integrated Combat
- Attacks must NOT lock the player in place.
- Most attacks modify velocity (forward lunge, aerial drift, recoil).
- Player should be able to move, jump, or dash during or immediately after attacks.

2. Fast Combat Loop
- Attacks have:
  - Startup frames (short)
  - Active frames
  - Recovery frames (cancelable)
- Recovery can be interrupted by:
  - Dash
  - Jump
  - Another attack (combo window)

3. Combo System
- Attacks can chain into each other:
  - Ground → Ground combo
  - Ground → Air combo
  - Air → Ground combo
- Use a "combo window timer" to allow chaining.
- Missing timing resets combo state.

4. Multi-Purpose Attacks
Each attack should:
- Deal damage
- Affect movement or positioning
- Enable follow-ups

Example:
- Forward slash → moves player slightly forward
- Downward attack → bounces player upward (pogo mechanic)

5. Air Combat Focus
- Player can attack freely in air.
- Air attacks maintain momentum.
- Downward attacks bounce off enemies or ground.

6. Resource System (Silk-like)
- Player generates resource by hitting enemies.
- Resource can be spent on:
  - Special attacks
  - Tools (secondary abilities)
  - Healing (high risk, consumes full bar)

(Reference: Silksong uses an aggressive resource loop where attacking fuels abilities.) :contentReference[oaicite:0]{index=0}

7. Tool / Ability Integration
- Player can equip abilities that modify combat:
  - Traps
  - Projectiles
  - Buffs
- These should integrate into attack chains.

8. Risk vs Reward
- Stronger attacks:
  - Longer recovery
  - Require commitment
- Faster attacks:
  - Lower damage
  - Safer

----------------------------------
TECHNICAL IMPLEMENTATION
----------------------------------

Use a STATE MACHINE:

States:
- Idle
- Move
- Jump
- Attack
- AirAttack
- Dash
- Recover

Each attack contains:
- startup_time
- active_time
- recovery_time
- cancel_window_start
- cancel_window_end
- velocity_effect (vector)
- hitbox data

----------------------------------
COMBAT FLOW LOGIC
----------------------------------

Loop:
1. Player observes enemy
2. Player positions (movement/dash)
3. Player executes attack
4. Attack modifies position
5. Player chains next action OR cancels

Combat should reward CONSTANT MOTION.
Standing still should be disadvantageous.

(Reference: Silksong combat encourages continuous movement and pressure.) :contentReference[oaicite:1]{index=1}

----------------------------------
EXAMPLE ATTACK DEFINITIONS
----------------------------------

Basic Slash:
- Fast startup
- Small forward movement
- Low recovery
- Chainable

Thrust Attack:
- Medium startup
- Strong forward lunge
- High damage
- Medium recovery

Downward Strike:
- Air-only
- Bounces player upward on hit
- Resets aerial options

Spin Attack:
- AoE around player
- Slower startup
- Defensive utility

----------------------------------
ADVANCED FEATURES
----------------------------------

- Attack buffering (queue next input)
- Hit pause (impact feedback)
- Enemy hitstun system
- Invincibility frames during dash
- Animation cancel system

----------------------------------
GOAL
----------------------------------

Produce:
1. Clean architecture (classes or components)
2. Example pseudocode or real code (Unity / Godot / generic)
3. Scalable system for adding new attacks and abilities
4. Emphasis on responsiveness and fluid combat

----------------------------------

Important:
Combat should feel like a continuous "dance" of movement and attacks, not isolated actions.