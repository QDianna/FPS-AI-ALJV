# FPS Enemy AI – ALJV Project

## Overview

This project implements an adaptive enemy AI system for a First-Person Shooter (FPS) game in Unity.

The architecture combines:
- **Behavior Trees (BT)** for high-level enemy behavior
- **Q-learning** for adaptive tactical combat decisions

The objective is to create an enemy capable of learning effective combat positioning and combat responses instead of relying entirely on scripted behavior.

---

## System Architecture

The AI is divided into two layers:

### Behavior Tree (Macro Layer)

The Behavior Tree controls the global enemy behavior flow:

- Patrol
- Chase
- Search
- Attack
- Retreat
- HitReact

State transitions are based on:
- line of sight
- distance to the player
- visibility memory
- recent damage received
- enemy health

This layer handles:
- navigation
- state transitions
- high-level combat flow

The BT is deterministic and acts as the decision framework around the reinforcement learning system.

---

### Reinforcement Learning (Combat Layer)

Inside the `Attack` state, the enemy uses **Q-learning** to select tactical combat actions.

Instead of hardcoding combat movement patterns, the agent learns:
- when to push aggressively
- when to retreat
- when to hold position for stable aim
- when to strafe under pressure

The combat behavior emerges from reward shaping and combat outcomes.

---

## Reinforcement Learning Design

### State Representation

The combat state is discretized using three contextual dimensions.

### Combat Advantage

Represents the current combat outcome tendency:

- `Winning`
- `Even`
- `Losing`

The value is computed using:
- damage dealt
- damage received
- current combat performance

---

### Distance State

Represents tactical engagement distance:

- `TooClose`
- `Close`
- `Medium`
- `Far`

The RL agent learns different movement strategies depending on engagement range.

---

### Pressure State

Represents combat pressure:

- `Safe`
- `UnderFire`

This state is triggered when the enemy receives recent damage and is used to model short-term combat pressure.

---

### Total State Space

The final RL state space contains:

```text
3 advantages × 4 distance states × 2 pressure states = 24 states
```

---

## Action Space

The agent can select the following tactical actions:

- `AggressivePush`
- `DefensiveRetreat`
- `HoldPosition`
- `StrafeLeft`
- `StrafeRight`

Actions are selected using an epsilon-greedy Q-learning policy.

---

## Valid Action Filtering

A major problem in the initial implementation was that the RL state did not contain information about world geometry.

This caused the agent to potentially learn invalid actions such as:
- strafing into walls
- retreating into obstacles
- selecting unreachable movement directions

To solve this, a valid-action filtering system was introduced using NavMesh validation.

The RL agent now selects actions only from movement options that are physically reachable in the current combat context.

---

## Aim and Accuracy System

Combat accuracy is simulated using a dynamic spread system.

Weapon spread depends on:
- engagement distance
- movement velocity
- current aiming stability

This creates a direct relationship between:
- movement
- positioning
- combat effectiveness

### Spread Behavior

- close range → lower spread
- long range → higher spread
- stationary aim → higher accuracy
- movement → reduced accuracy

This forces the RL system to balance:
- tactical repositioning
- aim stability

instead of rewarding movement alone.

---

## Reward Function

The reward system evaluates the outcome of combat decisions rather than rewarding predefined actions.

The reward combines:
- damage dealt
- damage received
- shot accuracy
- movement spread
- tactical repositioning quality

---

### Accuracy Reward

Accuracy is computed using:

```text
hits / shots fired
```

This rewards actions that produce effective shooting performance rather than random movement.

---

### Tactical Repositioning

The system evaluates whether movement improved tactical positioning.

Instead of checking only discrete state transitions such as:

```text
Far → Medium
```

the reward shaping also uses continuous distance evaluation:

```text
deltaDistance = oldDistance - newDistance
```

This allows the agent to detect meaningful tactical repositioning even when remaining inside the same discrete distance state.

Examples:
- pushing closer while `Far`
- creating distance while `Losing`
- avoiding overextension into `TooClose`

---

### Context-Aware Movement Evaluation

Movement penalties are not applied equally in every context.

Examples:
- `Winning + Far` → moving closer is encouraged
- `Losing + Close` → retreating becomes valuable

This allows movement spread penalties to be partially tolerated when repositioning improves tactical combat positioning.

---

## Q-Learning

The project uses a standard Q-learning update rule:

```text
Q(s,a) =Q(s,a) +α * (reward +γ * maxFutureQ -Q(s,a))
```

Where:
- `α` = learning rate
- `γ` = discount factor

This enables reward propagation across combat states.

Example:

```text
Far → Push → Medium → Hold → Successful Hits
```

Even if pushing initially reduces accuracy, the future combat advantage propagates backward through the Q-table.

---

## Training

Training is performed episodically inside the Unity environment.

### Phase 1 – Positioning Training

The player remains mostly stationary.

The enemy learns:
- to reduce distance when too far
- to stabilize aim at effective combat ranges
- to avoid ineffective positioning

---

### Phase 2 – Pressure Adaptation

The player actively shoots the enemy.

The enemy learns:
- when holding position becomes dangerous
- when strafing reduces incoming damage
- when retreating improves survival
- how combat pressure changes tactical behavior

---

## Debugging and Evaluation

The system includes detailed combat logging for debugging and analysis.

Logged information includes:
- current state
- selected action
- reward received
- damage dealt
- damage taken
- accuracy
- movement spread
- movement penalty
- tactical distance delta

This allows direct observation of:
- reward propagation
- tactical adaptation
- learned combat preferences

---

## Environment

The project is implemented in Unity and includes:

- FPS player controller
- NavMesh enemy navigation
- raycast-based shooting
- dynamic aiming system
- line-of-sight detection
- episodic RL combat training
- adaptive enemy combat AI

---

## Conclusion

The project demonstrates a hybrid AI architecture combining:
- deterministic Behavior Trees
- adaptive reinforcement learning

The final system is capable of learning tactical combat behaviors such as:
- aggressive repositioning
- defensive retreating
- pressure-based movement
- stable firing behavior

instead of relying entirely on hardcoded combat rules.