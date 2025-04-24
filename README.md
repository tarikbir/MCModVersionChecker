# MCModVersionChecker

MCModVersionChecker is a tool designed for Minecraft mod enthusiasts who maintain their own custom modpacks. It simplifies the process of checking mod compatibility across major Minecraft versions using latest data from CurseForge.

When a new Minecraft version is released, updating a modpack can be challenging due to broken dependencies and missing updates. This tool helps by checking which mods in your pack have been updated and are compatible with the given game version.

## Features

- Queries CurseForge for mod availability using a list of mod IDs.
- Nothing else yet.

## 🔑 CurseForge API Key

To use MCModVersionChecker, you'll need a CurseForge API key.

1. Create an account or sign in at the [CurseForge API Console](https://console.curseforge.com/#/api-keys).
2. Register an organization (you can use any name).
3. Generate an API key.
4. Open the **Settings** modal in the app and paste your API key there.

> ⚠️ The key is required to authenticate your requests to the CurseForge API.

## Planned Features
- [ ] Scan a folder to find mods from curseforge rather than using an id list
- [ ] Grab latest minecraft version automatically on startup (currently it's just set to 1.21.1, but it's modifiable)
- [ ] Grab all files rather than just latest releases (bulk releases break this app or can't search for older versions as curse's api mod list query only returns recent files)
- [ ] Download updates (not the intended usage, but since we get all mods, why not?)
- [ ] Get all mod info, including changelogs, latest version, etc.
