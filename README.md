# Jellyfin Plugin — SubtitleCat

Plugin de subtítulos para [Jellyfin](https://jellyfin.org) que descarga subtítulos desde [subtitlecat.com](https://www.subtitlecat.com).

- Sin API key ni registro requerido
- Compatible con películas y episodios de series
- Más de 30 idiomas: español, inglés, portugués, francés, alemán y más

## Instalación

### Desde repositorio (recomendado)

1. En Jellyfin ve a **Panel de control → Complementos → Repositorios**
2. Agrega esta URL:
   ```
   https://raw.githubusercontent.com/vicvinue/jellyfin-plugin-subtitlecat/main/manifest.json
   ```
3. Ve al **Catálogo**, busca **SubtitleCat** e instálalo
4. Reinicia Jellyfin

### Manual

1. Descarga el zip más reciente desde [Releases](https://github.com/vicvinue/jellyfin-plugin-subtitlecat/releases)
2. Extrae el contenido en la carpeta de plugins de Jellyfin:
   ```
   <JellyfinData>/plugins/SubtitleCat_1.0.0.0/
   ```
3. Reinicia Jellyfin

## Requisitos

- Jellyfin 10.11 o superior
- .NET 9 (incluido en la imagen Docker oficial de Jellyfin)

## Licencia

MIT
