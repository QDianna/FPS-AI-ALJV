# FPS Enemy AI – ALJV Project

## Overview

This project focuses on the design and implementation of an intelligent enemy agent for a First-Person Shooter (FPS) game, with an emphasis on adaptive decision-making and combat behavior optimization.

The proposed approach is hybrid and combines:
- **Behavior Trees (BT)** for high-level strategic decision-making
- **Reinforcement Learning (RL)** for low-level combat optimization

The main objective is to transition from static rule-based AI to an adaptive system capable of improving its combat strategies through interaction with the player.



## Motivation

FPS environments are dynamic, partially observable, and require fast decision-making under uncertainty.

The project is built upon an existing Unity prototype in which:
- the player can move, aim, and shoot
- enemy agents can navigate using NavMesh
- enemies can attack using a raycast-based shooting system
- current enemy behavior is entirely rule-based

This environment provides a controlled framework for integrating and evaluating learning-based AI techniques.



## Project Goals

The primary goal of the project is to design an AI agent capable of improving its combat effectiveness over time.

### Objectives

1. Implement a **Behavior Tree** for macro-level decision-making
2. Integrate **Q-learning** for combat behavior optimization
3. Replace static combat logic with adaptive decision-making mechanisms
4. Analyze how reward function design influences learned behavior
5. Compare the performance of rule-based AI against hybrid BT + RL AI



## System Architecture

The AI architecture is divided into two decision-making layers.

### 1. Behavior Tree (High-Level Control)

The Behavior Tree manages the global behavioral states of the agent:

- **Patrol** – no player detected
- **Chase** – player detected, but outside attack range
- **Search** – player lost from line of sight
- **Attack** – player visible and within attack range
- **HitReact** – player not visible, but the agent recently received damage
- **Retreat** – low health condition

State transitions are determined by:
- line of sight to the player
- distance between the agent and the player
- whether the agent recently received damage
- whether the player was seen in the last few seconds
- the current health level of the agent

This layer is deterministic and does not involve learning.



### 2. Reinforcement Learning (Combat Layer)

Inside the **Attack** state, the agent uses Reinforcement Learning to select combat actions.

The selected reinforced learning method is Q-learning.
The Q-learning method includes:
- a discrete state representation
- a reward function for evaluating combat behavior
- a Q-table used to store and update state-action values



## Reinforcement Learning Design

### State Representation (Discrete)

The continuous game state is discretized into the following parameters:

- distance to the player:
  - `close`
  - `medium`
  - `far`

- agent health:
  - `low`
  - `medium`
  - `high`

- player health:
  - `low`
  - `medium`
  - `high`

- recent combat events:
  - `tookDamage`
  - `gaveDamage`



### Action Space

The agent selects actions from a predefined combat action set:

- `StrafeLeftShoot`
- `StrafeRightShoot`
- `PushForwardShoot`
- `BackOffShoot`
- `MaintainDistanceShoot`
- `HoldPositionShoot`

Each action is executed for a short fixed duration.



### Reward Function

The reward function is designed to encourage efficient combat behavior by considering:

- damage dealt versus damage received, encouraging aggressive or defensive strategies depending on the combat situation
- distance to the player, encouraging maintenance of an effective combat range
- relative health levels, allowing the agent to adapt its behavior depending on combat advantage



### Learning Policy

The agent follows an **ε-greedy policy**:

- with probability `ε` → exploration (random action)
- otherwise → exploitation (best known action)



### Q-Update Rule

The state-action values are iteratively updated using the standard Q-learning update rule:

:contentReference[oaicite:0]{index=0}



## Environment Setup

The environment is implemented in **Unity** and includes:

- player controller (movement, aiming, shooting)
- enemy agents
- NavMesh navigation
- raycast-based shooting system
- line-of-sight visibility detection
- environmental obstacles affecting visibility
- enemy and player bases



## Evaluation Metrics

The AI performance is evaluated using the following metrics:

- damage dealt versus damage received
- survival time
- kill rate
- ability to maintain line of sight with the player

The comparison is performed between:
- baseline rule-based AI
- hybrid BT + RL AI



## Development Roadmap

### Milestone 1
- define and propose the project concept

### Milestone 2
- set up the development environment
- implement the core game mechanics

### Milestone 3
- integrate the Behavior Tree into the macro-level behavior logic
- integrate Q-learning into the micro-level combat logic
- define the representation of the current game state
- design the reward function for desired combat behavior

### Milestone 4
- refine the reward function
- improve state representation
- analyze learned behavior patterns

### Final Stage
- optimize the learned policy
- optionally extend the system using function approximation methods (e.g., neural networks)
- document and analyze experimental results



## Conclusion

This project demonstrates how combining classical AI techniques, such as Behavior Trees, with learning-based methods, such as Q-learning, can produce more adaptive and realistic enemy behavior in FPS games.

The hybrid architecture enables:
- structured strategic decision-making
- adaptive tactical behavior optimization

The proposed system aims to provide more dynamic and less predictable enemy interactions compared to traditional rule-based FPS AI systems.
