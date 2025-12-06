# 🌌 Vintage Presence

### Advanced Discord Rich Presence for Vintage Story — now with full in-game configuration.

VintagePresence is a **client-side mod** for *Vintage Story* that sends rich, dynamic activity information to Discord.
The mod started simples, but it has grown into a flexible and customizable presence system with a proper UI, template support, dynamic assets and stable timestamp handling.

## ✅ Current Features

Everything listed below is **already implemented** in the latest builds:

### 🎨 In-Game Settings GUI

A full configuration window accessible via **Ctrl + Shift + P**, allowing you to modify presence behavior without touching JSONs.

Includes:

* Editable **Details** and **State** templates
* Selection of **large** and **small Discord assets**
* Custom tooltip text
* Debug logging toggle

### 🖼 Dynamic Asset Switching

You can now choose which Discord images to use — or hide them entirely.
Switching assets no longer resets the elapsed session timer thanks to internal timestamp persistence.

### ⏱ Stable Elapsed Timer

Presence time no longer resets when:

* assets change
* text changes
* settings update
* the mod sends periodic updates

Timer is only reset manually or when the user configures it that way.

### 📁 Persistent Config System

All settings are stored, validated, and hot-reloaded inside the game.
The GUI uses a **local clone** of the config, meaning:

* Save → applies the changes
* Reset → restores defaults
* Cancel → discards edits

No more unintended immediate changes to the live presence.

### 🧩 Improved Discord RPC Integration

The project ships with a patched version of the DiscordRPC library, including a fix for:

* `NullReferenceException` inside `Assets.Merge`
  (already submitted upstream as a PR)

## 🔮 Features in Active Development

### 🧷 Token-Based Template Engine

Planned tokens include:

```
{world} {mode} {players} {server} {deaths} {biome} {health} {temperature}
```

Customizable exactly how you want it.

### 🔐 Privacy Controls

Hide anything you want:

* world names
* server names
* server IPs (never shown regardless)
* player count
* death counter

### 🔘 Discord Buttons

Configurable clickable links on your Rich Presence card.

### 🧠 Advanced Contextual Presence (Post-launch)

* Biome-aware states
* Temperature / weather indicators
* Per-world presets
* Multiple profiles
* Streamer mode

## 🛠 Compatibility

Works on:

* Vintage Story **1.21+**
* Singleplayer & multiplayer
* Linux, Windows, Steam Deck
* 100% client-side
* Compatible with all other client-side mods

## 📸 Preview

<img width="301" height="115" alt="screenshot" src="https://github.com/user-attachments/assets/5e086c46-ca73-4d7c-bef6-261c3b673381" />

## 🧰 Technical Notes for Developers

* Presence updates occur on a periodic tick using cached config.
* Timestamp logic persists `_elapsedStart` safely across updates.
* Asset updates gracefully handle null transitions.
* Config window editing uses a **deep copy**, preventing accidental live mutation.
* Includes upstream-fix PR for the DiscordRPC library.

## 📜 License

MIT License — free to use, fork, modify, and experiment with.

## 💬 Feedback & Contributions

Testers, modders, nerds and curious souls are welcome to participate.
Open an issue or PR — especially if you find edge cases or want to suggest new presence tokens.

### **TL;DR:**

It works. It’s customizable. It’s stable. And yes — the good stuff is still coming.
