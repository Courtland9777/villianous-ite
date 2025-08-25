# GAME_KNOWLEDGE.md — Disney Villainous: Introduction to Evil
_Date: 2025-08-25_

## Villains & Objectives
- **Ursula**  
  - **Objective**: Control both the **Trident** and the **Crown** at **Ursula’s Lair**.  
  - **Notes**: Contracts can bind Heroes; be mindful of ally positioning when resolving.  

- **Captain Hook**  
  - **Objective**: Defeat **Peter Pan** at the **Jolly Roger**.  
  - **Notes**: Peter Pan is hidden at the bottom of Hook’s Fate deck until Never Land is unlocked.  

- **Maleficent**  
  - **Objective**: Have a **Curse** at each of her Realm’s four locations.  
  - **Notes**: Curses may be broken by Heroes/Fate cards; needs careful maintenance.  

- **Prince John**  
  - **Objective**: Start a turn with **20 or more Power** and **Robin Hood** imprisoned at **The Jail**.  
  - **Notes**: Intro to Evil update requires Robin Hood’s imprisonment (different from base).  

---

## Setup (Intro to Evil Edition)
- 2–4 players  
- Each starts with **2 Power**  
- Movers begin on their **portrait side**  
- The **Fate token is not used**  
- Objectives are checked **immediately**, not only at turn start  

---

## Components
- 4 Villain movers (Ursula, Hook, Maleficent, Prince John)  
- 4 Villain realms (each with 4 locations)  
- 120 Villain cards (30 per villain)  
- 60 Fate cards (15 per villain, themed per opponent)  
- 80 Power tokens  
- 4 Reference cards  
- 4 Villain guides  
- Rulebook (Intro to Evil, 2023 revision)  

---

## Core Rules Summary
- **Turns**: On each turn, move your Villain mover, then perform available actions at that location.  
- **Actions**: Gain Power, Play a Card, Activate, Fate, Move, Vanquish, Discard.  
- **Vanquish**: Discard Allies at the location whose combined Strength ≥ chosen Hero’s Strength, then discard that Hero.  
- **Fate**: Draw from opponent’s Fate deck, choose one card, and play it against them.  
- **Victory**: Achieve your villain’s unique objective.  

---

## Modeling Notes for Implementation
- **Hidden Information**:  
  - Opponent hands hidden.  
  - Fate deck contents hidden except top revealed when played.  
- **Prompts**:  
  - Some effects require player choice (targets, items, location).  
  - Represent as `PendingPrompt` in `GameState`.  
- **Determinism**:  
  - Use seeded RNG for shuffle order.  
  - Replays must produce identical state with same seed and commands.  
- **Special Cases**:  
  - Multiple Heroes at same location → vanquish one at a time.  
  - Ursula’s Contracts temporarily neutralize but do not remove Heroes.  
  - Hook must unlock Never Land before Peter Pan can appear.  
