# FPS Enemy AI – ALJV Project

## Overview

This project focuses on designing and implementing an intelligent enemy agent for a First-Person Shooter (FPS) game, with an emphasis on decision-making and adaptive combat behavior.

The approach is **hybrid**:
- **Behavior Tree (BT)** for high-level decision making
- **Reinforcement Learning (RL)** for low-level combat optimization

The goal is to move from rule-based AI to a system capable of adapting its combat strategy based on interaction with the player.



## Motivation

FPS environments are dynamic, partially observable, and require fast decision-making under uncertainty.

The project builds on an existing Unity prototype where:
- the player can move, aim, and shoot
- enemies can navigate using NavMesh and shoot using raycasting
- current enemy behavior is rule-based

This provides a controlled environment to integrate and evaluate learning-based AI.



## Project Goals

The main objective is to design an AI agent that improves its combat behavior over time.

### Objectives

1. Implement a **Behavior Tree** for macro-level decisions
2. Integrate **Reinforcement Learning (Q-learning)** for combat behavior
3. Replace static rule-based combat with adaptive decision-making
4. Analyze how reward design influences learned behavior
5. Compare baseline AI vs hybrid AI performance



## System Architecture

The AI is structured on two decision layers:

### 1. Behavior Tree (High-Level Control)

The Behavior Tree manages global agent states:

- **Patrol** – no player detected
- **Chase** – player detected but outside attack range
- **Search** – lost line-of-sight to player
- **Attack** – player in range and visible
- **Retreat** – low health

Transitions are based on:
- distance to player
- line-of-sight (LOS)
- agent health

This layer is deterministic and does not use learning.



### 2. Reinforcement Learning (Combat Layer)

Inside the **Attack** state, the agent uses RL to select actions.

The learning algorithm used is:
- **Q-learning (tabular, discrete state space)**



## Reinforcement Learning Design

### State Representation (discrete)

The continuous game state is discretized into:

- distance: `close / mid / far`
- health: `low / high`
- player visible: `true / false`
- recent damage:
  - `tookDamage`
  - `gaveDamage`



### Action Space

The agent selects from predefined combat actions:

- `ShootStanding`
- `StrafeLeftShoot`
- `StrafeRightShoot`
- `PushForwardShoot`
- `BackOffShoot`
- `HoldPosition`

Each action is executed for a short fixed duration.



### Reward Function

The reward is designed to encourage effective combat behavior:

- +10 → successful hit
- -10 → damage received
- -1  → idle / ineffective action
- + small bonus → maintaining optimal distance (optional)



### Learning Policy

The agent uses an **ε-greedy policy**:

- with probability ε → explore (random action)
- otherwise → exploit (best known action)



### Q-Update Rule

The Q-values are updated using the standard Q-learning formula:

Q(s, a) = Q(s, a) + α * (reward + γ * max(Q(s', a')) - Q(s, a))



## Environment Setup

The environment is implemented in **Unity** and includes:

- player controller (movement, aiming, shooting)
- enemy agents
- NavMesh navigation
- raycast-based shooting system
- visibility detection (line-of-sight)

Experiments are conducted in controlled scenarios (e.g. 1v1 combat in a simple map).



## Evaluation Metrics

The AI performance is evaluated using:

- hit accuracy
- damage dealt vs damage received
- survival time
- kill rate
- behavioral consistency

Comparison is made between:
- baseline rule-based AI
- hybrid BT + RL agent



## Development Roadmap

### Milestone 3
- integrate Q-learning into Attack behavior
- basic state/action/reward implementation
- demonstrate learning loop

### Milestone 4
- refine reward function
- improve state representation
- analyze behavior patterns

### Final Stage
- optimize policy
- optional: extend to function approximation (e.g. neural networks)
- document experimental results



## Conclusion

This project demonstrates how combining classical AI techniques (Behavior Trees) with learning-based methods (Reinforcement Learning) can produce more adaptive and realistic enemy behavior in FPS games.

The hybrid approach allows:
- structured decision-making at a strategic level
- adaptive optimization at a tactical level
