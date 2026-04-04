# 🛡️ Valheim Reporter

<p align="center">
  <img src="https://i.ibb.co/yc6rbnCr/Valheim-Reporter.webp" alt="Valheim Reporter Logo" width="700">
</p>

<p align="center">
  <a href="https://github.com/n1h1lius/Valheim-Reporter/releases" target="_blank"><img src="https://img.shields.io/github/v/release/n1h1lius/Valheim-Reporter?style=for-the-badge&color=orange" alt="Release"></a>
  <a href="https://n1h1lius.orlog.workers.dev/static/sites/valheim-reporter/valheimReporter" target="_blank"><img src="https://img.shields.io/badge/Documentation-View%20Live-blue?style=for-the-badge&logo=gitbook&logoColor=white" alt="Docs"></a>
  <a href="https://github.com/n1h1lius/Valheim-Reporter/blob/main/LICENSE" target="_blank"><img src="https://img.shields.io/github/license/n1h1lius/Valheim-Reporter?style=for-the-badge" alt="License"></a>
</p>

---

### 📖 Overview
**Valheim Reporter** is a professional-grade, in-game reporting system for Valheim servers. It bridges the gap between players and staff by providing an organic way to report issues, bugs, or players directly to your **Discord** server with full visual evidence.

> [!IMPORTANT]
> **Looking for the full setup guide?** > Visit the [Official Documentation Website](https://n1h1lius.orlog.workers.dev/static/sites/valheim-reporter/valheimReporter) for deep technical details and tutorials.

---

### ✨ Key Features

* 📸 **Clean Screenshots:** Captures the game world without HUD or UI interference.
* 📍 **Map Tracking:** Automatically attaches a map screenshot with the player's exact coordinates.
* 🔐 **Secure Backend:** Supports **Advanced Encryption** via Cloudflare Workers to hide your Webhooks from malicious users.
* ⚙️ **Highly Customizable:** Define your own categories, subcategories, and mandatory requirements via JSON.
* 🚀 **Performance First:** Uses asynchronous Coroutines to ensure zero impact on game FPS during uploads.

---

### 🛠️ Operational Modes

| Mode | Target | Security |
| :--- | :--- | :--- |
| **Simple** | Small/Private Servers | Raw Webhooks in JSON (Shared with players). |
| **Advanced** | Public/Massive Communities | **Encrypted Webhooks** + External API (Max Privacy). |

---

### 🚀 Quick Installation

1.  **Download:** Get the latest `.dll` from [Thunderstore](https://valheim.thunderstore.io/), from [NexusMods](https://www.nexusmods.com/valheim/mods/3298) or from latest release.
2.  **Plugin:** Drop the file into your `BepInEx/plugins` folder.
3.  **Data:** Place your `categories.json` inside `ValheimReporter/Data`.
4.  **Launch:** Press `F10` (default) in-game to start reporting!

---

### 🛠️ Developer Resources

If you are an administrator looking to set up the **Advanced Method**, check these resources:

* 🌐 **[Category Generator](https://n1h1lius.orlog.workers.dev/static/sites/valheim-reporter/html/category_generator.html):** Tool to encrypt your Webhooks and build your JSON.
* ⚡ **[Backend API (index.js)](https://github.com/n1h1lius/Valheim-Reporter/tree/main/Valheim_Reporter_API):** The Cloudflare Worker script to handle secure requests.

---

### 🤝 Contributing & Support

* **Found a bug?** Open an [Issue](https://github.com/n1h1lius/Valheim-Reporter/issues).
* **Want to help?** Pull Requests are welcome! Check the [Contribution FAQ]([https://valheim-reporter.pages.dev/#faqs](https://n1h1lius.orlog.workers.dev/static/sites/valheim-reporter/valheimReporter#faqs)).
* **Support the project:** If this mod helps your community, consider [Buying me a coffee](https://ko-fi.com/n1h1lius).

---

<p align="center">
  Built with ☕ and ⚡ by <b>n1h1lius</b>
</p>
