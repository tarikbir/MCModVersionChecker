# MCModVersionChecker

Windows desktop tool for people who run their own Minecraft modpacks. Given a list of mods and a target Minecraft version + loader family, it checks Modrinth or CurseForge and shows which mods already have a published file for that filter — so you can decide whether migrating the pack is worth it.

This is an **availability check**, not a pack updater and not a mod-to-mod compatibility tester. It only answers “has this mod shipped for that Minecraft version (and loader type) yet?”

## Features

- **Modrinth** and **CurseForge** as sources (switch from the Source menu)
- Lookup by **mod ID**, **name**, or **slug**
- Filter by Minecraft version and loader **type** (**Forge**, **Fabric**, **NeoForge**, **Quilt**) — not a specific loader build/version
- Import a mods folder (`File → Import Folder`) — fills the list from jar file names
- Shows whether each mod has a matching release, plus its latest known game version
- Opens the mod page from the results list
- Remembers preferred source, lookup mode, version, and loader
- CurseForge API key is stored in Settings (Modrinth needs no key)

## Requirements

- Windows 10/11 **x64**
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (Windows Desktop)

Framework-dependent publishes need that runtime installed. Self-contained publishes do not.

## CurseForge API key

Only required when Source is CurseForge.

1. Sign in at the [CurseForge API Console](https://console.curseforge.com/#/api-keys)
2. Register an organization (any name is fine)
3. Generate an API key
4. Paste it under **Settings** in the app

## Usage

1. Pick **Modrinth** or **CurseForge**, then lookup mode (ID / name / slug)
2. Paste one entry per line, or import a mods folder
3. Set the Minecraft version and loader
4. Click **Check Mods**

## Releases

Prebuilt Windows binaries are on the [Releases](https://github.com/tarikbir/MCModVersionChecker/releases) page.

Typical release asset: `MCModVersionChecker-vX.Y.Z-win-x64.zip` containing the executable (and config if needed). See release notes for whether the build is framework-dependent or self-contained.

## Planned

- [ ] Auto-detect latest Minecraft version on startup
- [ ] Richer mod details (changelog, all available versions)
- [ ] Query older / non-latest file indexes when the public APIs allow it

## Not planned

These are intentionally out of scope:

- Checking whether mods work with each other (dependency / soft-compat graphs)
- Matching a specific Forge/Fabric/NeoForge/Quilt **version** (authors rarely publish that reliably on the listing APIs)
- Updating or rebuilding a modpack from this app
- Keeping a modpack continuously up to date
- Automatically downloading mods or updates from Modrinth / CurseForge
- Ads, tracking, or monetization overlays

The goal stays narrow: help you answer “do enough of my mods even exist for this Minecraft version yet?” — especially if you maintain a personal pack.

## AI disclosure

Parts of this project (code, docs, and tooling) were written or revised with assistance from AI coding tools. Human review and intent still define the scope and direction of the app.
