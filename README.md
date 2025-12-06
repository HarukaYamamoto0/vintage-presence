# 🌌 Vintage Presence
### Advanced Discord Rich Presence for Vintage Story — now with full in-game configuration and template engine.

VintagePresence is a **client-side mod** for *Vintage Story* that sends rich, dynamic activity information to Discord.
What started as a simple presence mod has evolved into a powerful, fully customizable system with an in-game UI, flexible template engine, dynamic assets, and rock-solid stability.

## ✅ Current Features
Everything listed below is **already implemented** in the latest builds:

### 🎨 In-Game Settings GUI
A full configuration window accessible via **Ctrl + Shift + P**, allowing you to modify presence behavior without touching JSONs.

Includes:
* Editable **Details** and **State** templates with live preview
* Selection of **large** and **small Discord assets**
* Custom tooltip text for images
* Debug logging toggle
* Save/Reset/Cancel workflow

### 🧷 Template Engine
**Fully implemented** token-based template system with rich contextual data:

**Available tokens:**
```
{gamemode}      - Current game mode (Creative, Survival, Spectator)
{day}           - Total elapsed days
{timeofday}     - Morning, Day, Evening, Night
{playername}    - Your character name
{health}        - Current health points
{healthpercent} - Current health points in percent
{deaths}        - Death counter
{players}       - Players on server
{temperature}   - Current temperature in °C
{weather}       - Snow, Rain, Cold, Clear
{season}        - Spring, Summer, Fall, Winter
{coords}        - Your coordinates (X, Y, Z)
{modversion}    - VintagePresence version
{gameversion}   - Vintage Story version
```

**Example templates:**
```
Details: "{gamemode} • Day {day} • {timeofday}"
State:   "{weather} • {temperature}°C • HP: {health}/{maxhealth}"
```

The engine renders templates in real-time (Actually, there's a 5-second delay, but I'm working on it.) with proper null-safety and validation.

### 🖼 Dynamic Asset Switching
Choose which Discord images to use — or hide them entirely.

Switching assets **never** resets your elapsed session timer thanks to internal timestamp persistence.

### ⏱ Stable Elapsed Timer
Presence time no longer resets when:
* Assets change
* Text changes
* Settings update
* The mod sends periodic updates

Timer persists across the entire session and only resets when you close the game.

### 📁 Persistent Config System
All settings are stored, validated, and hot-reloaded inside the game.

The GUI uses a **local clone** of the config, meaning:
* **Save** → applies changes and triggers immediate Discord update
* **Reset** → restores defaults
* **Cancel** → discards edits

No more unintended live changes to your presence.

### 🧩 Improved Discord RPC Integration
Ships with a patched version of the DiscordRPC library, including:
* Fixed `NullReferenceException` inside `Assets.Merge` (PR submitted upstream)
* Proper cleanup on game shutdown (presence clears correctly)
* 500ms grace period before disposal to ensure Discord processes the clear command

## 🔮 Planned Features

### 🔐 Enhanced Privacy Controls
Fine-grained control over what information is shared:
* Toggle world names
* Toggle server names
* Toggle player count
* Toggle death counter
* IP addresses are **never** shared (always private)

### 🔘 Discord Buttons
Configurable clickable links on your Rich Presence card:
* Join server button
* Custom website/community links
* Social media integration

### 🧠 Advanced Contextual Presence
* Biome-aware states (show current biome in presence)
* Activity detection (mining, farming, combat, crafting)
* Per-world presets (different templates per save)
* Multiple profiles (quick-switch between configs)
* Streamer mode (privacy-first preset)

## 🛠 Compatibility

Works on:
* Vintage Story **1.21+**
* Singleplayer & multiplayer
* Linux, Windows, macOS, Steam Deck
* 100% client-side
* Compatible with all other client-side mods

## 📸 Preview

<img width="301" height="115" alt="screenshot" src="https://github.com/user-attachments/assets/5e086c46-ca73-4d7c-bef6-261c3b673381" />
<img width="420" height="475" alt="screenshot_2" src="https://github.com/user-attachments/assets/b7ecbfc6-23c7-4c5e-82c8-aa4b0371a21a" />


## 🧰 Technical Notes for Developers

**Architecture:**
* Presence updates occur on a periodic tick (configurable interval) using a cached config snapshot
* Template engine uses regex-based token replacement with context injection
* Timestamp logic persists `_elapsedStart` safely across all updates
* Asset updates gracefully handle null transitions without timer reset
* Config window editing uses a **deep copy**, preventing accidental live mutation

**Discord Integration:**
* Includes upstream-fix PR for DiscordRPC library's `Assets.Merge` crash
* Proper disposal sequence: `ClearActivity()` → 500ms delay → `Dispose()`
* Non-static service instance to prevent conflicts on mod reload

**Template Engine:**
* Context-aware rendering with null-safe defaults
* Supports nested tokens and whitespace preservation
* Validates templates before applying to prevent crashes
* Easy to extend with new tokens via `PresenceContext` class

## 📜 License

MIT License — free to use, fork, modify, and experiment with.

## 💬 Feedback & Contributions

Testers, modders, nerds, and curious souls are welcome to participate.

Open an issue or PR — especially if you:
* Find edge cases or bugs
* Want to suggest new presence tokens
* Have ideas for privacy features
* Want to contribute translations

### **TL;DR:**
✅ It works.  
✅ It's customizable.  
✅ It's stable.  
✅ The template engine is done.  
✅ And yes — more good stuff is still coming.
