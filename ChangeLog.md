  # 🛡️ Valheim Reporter - Changelog
  
  ## [1.1.0] - 2026-04-05
  
  ### 🔧 Fixed
  * **Input Conflicts (Ghost Actions):** Resolved a critical issue where player hotkeys and actions remained active while typing a report. Opening the interface now correctly blocks character movement, attacks, and hotbar usage (1-8 keys) to prevent accidental item consumption. The mouse cursor is also automatically released for a smoother UI experience.
  
  ### 🚀 Added
  * **Auto-Installation System (Embedded Assets):** Completely redesigned file management to ensure 100% compatibility with Thunderstore and r2modman. Essential files like AssetBundles, sounds, and images are now embedded directly inside the `.dll`.
  * **Dynamic Resource Extraction:** The mod now automatically detects and extracts missing files on the first launch, removing the need for manual folder placement in the `plugins` directory.
  
  ### 📂 Changed
  * **Migration to Config Folder:** Following Valheim modding best practices, the entire data structure (Data, Assets, Images, and Logs) has been moved to the official path: `BepInEx/config/ValheimReporter`. This fixes "File Not Found" errors caused by relative paths when using mod managers.
  * **Enhanced Logging:** Optimized internal loading to use dynamic absolute paths. Added detailed logging during the initialization process so administrators can easily verify that file extraction and category loading were successful.
  
  ---
  
  > **Note for Admins:** You can now safely delete the ValheimReporter folder from your plugins directory. The mod will handle everything from the config folder automatically from now on.
