# SubMichi — Plugin de Subtítulos para Jellyfin

**Español** | [English](README.en.md)

Plugin proveedor de subtítulos para [Jellyfin](https://jellyfin.org) que descarga subtítulos desde [subtitlecat.com](https://www.subtitlecat.com).

- No requiere API key ni registro
- Compatible con películas y episodios de series
- Más de 30 idiomas, incluido español (incl. latinoamericano `es-419`), inglés, portugués, francés, alemán y más

## Cómo funciona

subtitlecat.com indexa los subtítulos por **nombre de release**, no por el título localizado ni por el id de IMDB. Como Jellyfin puede pasar un título localizado a los proveedores (por ejemplo *"Letras Robadas"* en lugar de *"Power Ballad"*), el plugin construye la búsqueda a partir del **nombre del archivo** — recortado hasta el año en películas (`Power Ballad 2026`) o hasta el token `SxxExx` en episodios (`Show Name S01E02`) — y recurre al título de los metadatos solo si el nombre del archivo no devuelve resultados.

## Instalación

### Vía repositorio de plugins (recomendado)

1. En Jellyfin ve a **Panel de control → Complementos → Repositorios**
2. Agrega esta URL:
   ```
   https://raw.githubusercontent.com/vicvinue/jellyfin-plugin-subtitlecat/main/manifest.json
   ```
3. Ve a **Catálogo**, busca **SubMichi** e instálalo
4. Reinicia Jellyfin

### Manual

1. Descarga el último zip desde [Releases](https://github.com/vicvinue/jellyfin-plugin-subtitlecat/releases)
2. Extrae en la carpeta de plugins de Jellyfin:
   ```
   <JellyfinData>/plugins/SubMichi_1.2.0.0/
   ```
3. Reinicia Jellyfin

## Requisitos

- Jellyfin 10.11 o superior
- Runtime de .NET 9 (incluido en la imagen Docker de Jellyfin)

## Licencia

MIT
