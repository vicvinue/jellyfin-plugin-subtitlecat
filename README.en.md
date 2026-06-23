# SubMichi — Jellyfin Subtitle Plugin

[Español](README.md) | **English**

Subtitle provider plugin for [Jellyfin](https://jellyfin.org) that downloads subtitles from [subtitlecat.com](https://www.subtitlecat.com).

- No API key or registration required
- Supports movies and TV episodes
- 30+ languages including Spanish (incl. Latin American `es-419`), English, Portuguese, French, German and more

## How it works

subtitlecat.com indexes subtitles by **release name**, not by localized title or IMDB id. Because Jellyfin may pass a localized title to providers (e.g. *"Letras Robadas"* instead of *"Power Ballad"*), the plugin builds its search query from the media **file name** — trimmed to the year for movies (`Power Ballad 2026`) or to the `SxxExx` token for episodes (`Show Name S01E02`) — and falls back to the metadata title if the file name yields nothing.

## Installation

### Via Plugin Repository (recommended)

1. In Jellyfin go to **Dashboard → Plugins → Repositories**
2. Add this URL:
   ```
   https://raw.githubusercontent.com/vicvinue/jellyfin-plugin-subtitlecat/main/manifest.json
   ```
3. Go to **Catalog**, find **SubMichi** and install it
4. Restart Jellyfin

### Manual

1. Download the latest zip from [Releases](https://github.com/vicvinue/jellyfin-plugin-subtitlecat/releases)
2. Extract to your Jellyfin plugins folder:
   ```
   <JellyfinData>/plugins/SubMichi_1.2.0.0/
   ```
3. Restart Jellyfin

## Requirements

- Jellyfin 10.11 or later
- .NET 9 runtime (included in the Jellyfin Docker image)

## License

MIT
