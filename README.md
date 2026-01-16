# 🎮 Collapse Mechanic – GJG Summer Internship Case Study

**Candidate:** Mehmet Sefa KARATAŞ  
**Role:** Game Developer Intern Candidate  
**Date:** January 2026  

---

## 🚀 Project Overview

This project is a high-performance, scalable implementation of a **Collapse / Blast** game mechanic in Unity. It is designed with a strong emphasis on **Memory Optimization**, **CPU Efficiency**, and **Clean Architecture**, strictly adhering to the requirements specified in the case study document.

The system features a dynamic grid, state-based visual feedback (Condition A, B, C), and a deterministic deadlock resolution system.

---

## 🏗 Architecture & Design Decisions

Moving away from the traditional *Monolithic Manager* approach, the project adopts a **Feature-Based Architecture** with strict **Separation of Concerns (SoC)**.

### 1. Core Logic (The Brain)

- **`DeadlockSolver`**  
  A static helper class responsible for algorithmic checks. It determines if the board is solvable, detects deadlocks, and calculates candidates for shuffling. It operates purely on data, independent of Unity’s update loop.

- **`MatchFinder`**  
  Implements a **Breadth-First Search (BFS)** algorithm to find connected tile groups efficiently.

- **`BoardContext` (Struct)**  
  To avoid passing 5–6 parameters (grid, width, height, lists, etc.) between methods, a lightweight `struct` is used as a Data Transfer Object (DTO). This ensures **stack allocation** (zero garbage collection) compared to using a class.

### 2. Management (The Conductor)

- **`BoardManager`**  
  Acts as the central controller. It manages the game loop, handles object pooling, and delegates specific tasks (input, logic, animation) to sub-systems. It does not contain game logic itself; it only executes it.

### 3. Presentation (The View)

- **`Tile`**  
  A smart `MonoBehaviour` that handles its own animations (spawn, move, explode) using **DOTween**, encapsulating visual logic away from the manager.

- **`BoardVisualizer`**  
  Responsible for updating the visual state of tiles (Icons A, B, C) based on group sizes, ensuring the visual representation always matches the logical state.

---

## ⚡ Performance Optimizations

As per the requirement *“emphasis on optimizing memory”*, the following techniques were implemented:

1. **Zero-Allocation Pathfinding**  
   The `MatchFinder` class uses `static readonly` collections (`Queue` and `HashSet`) as reusable buffers. They are cleared and reused every frame, resulting in **0 KB GC allocation** during the gameplay loop.

2. **Sprite Atlas (Draw Call Batching)**  
   All tile assets and icons are packed into a **Unity Sprite Atlas**, significantly reducing draw calls by allowing the GPU to render multiple tiles in a single batch—especially beneficial for mobile devices.

3. **Struct-Based Context**  
   Data transfer objects (DTOs) like `BoardContext` are defined as `struct` to utilize stack memory, preventing heap fragmentation and reducing GC pressure.

4. **Object Pooling**  
   Unity’s `UnityEngine.Pool` API is used for tiles. No `Instantiate` or `Destroy` calls occur during the gameplay loop, ensuring stable frame rates.

---

## 🧠 Smart Deadlock Resolution

Instead of *“blindly shuffling N times”*, the system uses a deterministic **three-phase resolution strategy** to guarantee playability:

1. **Phase 1 – Color Injection**  
   If the board is mathematically unsolvable (e.g., no color has at least two tiles remaining), the system injects a needed color into a neighboring tile to create a valid match.

2. **Phase 2 – Fisher–Yates Shuffle**  
   If the board is solvable but locked, tile positions are shuffled mathematically using a Fisher–Yates shuffle.

3. **Phase 3 – Force Match**  
   As a fail-safe, a potential match pair is identified and forcibly moved adjacent to each other, guaranteeing playability **100% of the time**.

---

## 📂 Project Structure
```
_GAME
├── ScriptableObjects
│   ├── GameConfig
│   └── Levels (Example 1, Example 2 Data)
├── Scripts
│   ├── Core
│   │   ├── Data (BoardConfig.cs)
│   │   └── Utils (MonoBehaviourSingleton.cs, LevelDebugger.cs)
│   ├── Gameplay
│   │   └── Board
│   │       ├── Logic (MatchFinder.cs, DeadlockSolver.cs, BoardContext.cs)
│   │       ├── BoardInputHandler.cs
│   │       ├── BoardVisualizer.cs
│   │       └── Tile.cs
│   └── Managers (BoardManager.cs)
```

---

## 🎮 How to Test (Debug Controls)

To facilitate the review process, a developer tool has been included (**active only in the Unity Editor**):

- **SPACE KEY**  
  Instantly switches between **Level 1** (10×12, 6 colors) and **Level 2** (5×8, 4 colors) configurations as described in the PDF.

- **Mouse Click**  
  Blast tile groups.

---

## 🛠 Third-Party Assets

- **DOTween (Demigiant)**  
  Used for efficient, pooled tweening animations (movement, scaling, color changes).

---

Thank you for reviewing my case study!

