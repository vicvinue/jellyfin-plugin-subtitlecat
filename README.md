# Jellyfin Plugin — SubtitleCat

Subtitle provider plugin for [Jellyfin](https://jellyfin.org) that downloads subtitles from [subtitlecat.com](https://www.subtitlecat.com).

- No API key required
- Supports movies and TV episodes
- 30+ languages including Spanish, English, Portuguese, French, German and more

## Installation

### Via Plugin Repository (recommended)

1. In Jellyfin go to **Dashboard → Plugins → Repositories**
2. Add this URL:
   ```
   https://raw.githubusercontent.com/vicvinue/jellyfin-plugin-subtitlecat/main/manifest.json
   ```
3. Go to **Catalog**, find **SubtitleCat** and install it
4. Restart Jellyfin

### Manual

1. Download the latest zip from [Releases](https://github.com/vicvinue/jellyfin-plugin-subtitlecat/releases)
2. Extract to your Jellyfin plugins folder:
   ```
   <JellyfinData>/plugins/SubtitleCat_1.0.0.0/
   ```
3. Restart Jellyfin

## Requirements

- Jellyfin 10.11 or later
- .NET 9 runtime (included in the Jellyfin Docker image)

## License

MIT
