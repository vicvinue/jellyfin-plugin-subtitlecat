using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SubtitleCat;

public class SubtitleCatProvider : ISubtitleProvider
{
    private readonly ILogger<SubtitleCatProvider> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string BaseUrl = "https://www.subtitlecat.com";

    private static readonly Regex EpisodeToken =
        new(@"\bS(\d{1,2})E(\d{1,3})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex YearToken =
        new(@"\b(?:19|20)\d{2}\b", RegexOptions.Compiled);
    private static readonly Regex MultiSpace =
        new(@"\s+", RegexOptions.Compiled);

    private static readonly Dictionary<string, string[]> LangMap = new(StringComparer.OrdinalIgnoreCase)
    {
        {"eng", new[]{"en"}},
        {"spa", new[]{"es", "es-419"}},
        {"por", new[]{"pt", "pt-BR", "pt-br"}},
        {"fra", new[]{"fr"}},
        {"deu", new[]{"de"}},
        {"ita", new[]{"it"}},
        {"jpn", new[]{"ja"}},
        {"kor", new[]{"ko"}},
        {"zho", new[]{"zh", "zh-CN", "zh-TW"}},
        {"ara", new[]{"ar"}},
        {"rus", new[]{"ru"}},
        {"nld", new[]{"nl"}},
        {"pol", new[]{"pl"}},
        {"swe", new[]{"sv"}},
        {"nor", new[]{"no"}},
        {"dan", new[]{"da"}},
        {"fin", new[]{"fi"}},
        {"hun", new[]{"hu"}},
        {"ces", new[]{"cs"}},
        {"tur", new[]{"tr"}},
        {"ind", new[]{"id"}},
    };

    private static readonly Dictionary<string, string> ReverseLangMap = new(StringComparer.OrdinalIgnoreCase)
    {
        {"en","eng"},{"es","spa"},{"es-419","spa"},{"pt","por"},{"pt-BR","por"},
        {"fr","fra"},{"de","deu"},{"it","ita"},{"ja","jpn"},{"ko","kor"},{"zh","zho"},
        {"zh-CN","zho"},{"zh-TW","zho"},{"ar","ara"},{"ru","rus"},{"nl","nld"},{"pl","pol"},
        {"sv","swe"},{"no","nor"},{"da","dan"},{"fi","fin"},{"hu","hun"},{"cs","ces"},
        {"tr","tur"},{"id","ind"},
    };

    public SubtitleCatProvider(ILogger<SubtitleCatProvider> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public string Name => "SubtitleCat";

    public IEnumerable<VideoContentType> SupportedMediaTypes =>
        new[] { VideoContentType.Movie, VideoContentType.Episode };

    private static string EncodeId(string url) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(url))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static string DecodeId(string id)
    {
        var padded = id.Replace('-', '+').Replace('_', '/');
        var pad = padded.Length % 4;
        if (pad != 0) padded += new string('=', 4 - pad);
        return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }

    // SubtitleCat indexes subtitles by release name, not by localized title or IMDB id,
    // so the media file name is the most reliable query. We fall back to the metadata
    // title (which Jellyfin may have localized, e.g. "Letras Robadas" for "Power Ballad").
    private static List<string> BuildQueries(SubtitleSearchRequest request)
    {
        var queries = new List<string>();

        var fileQuery = QueryFromFileName(request.MediaPath);
        if (!string.IsNullOrWhiteSpace(fileQuery))
            queries.Add(fileQuery);

        if (request.ContentType == VideoContentType.Episode
            && !string.IsNullOrWhiteSpace(request.SeriesName)
            && request.ParentIndexNumber.HasValue
            && request.IndexNumber.HasValue)
        {
            queries.Add($"{request.SeriesName} S{request.ParentIndexNumber:D2}E{request.IndexNumber:D2}");
        }
        else if (!string.IsNullOrWhiteSpace(request.Name))
        {
            queries.Add(request.ProductionYear.HasValue
                ? $"{request.Name} {request.ProductionYear}"
                : request.Name);
        }

        return queries
            .Where(q => !string.IsNullOrWhiteSpace(q))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // Turns "Power.Ballad.2026.1080p.AMZN.WEB-DL...mkv" into "Power Ballad 2026",
    // and "Show.Name.S01E02.1080p...mkv" into "Show Name S01E02".
    private static string? QueryFromFileName(string? mediaPath)
    {
        if (string.IsNullOrWhiteSpace(mediaPath))
            return null;

        var name = Path.GetFileNameWithoutExtension(mediaPath);
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var cleaned = name.Replace('.', ' ').Replace('_', ' ').Replace('-', ' ');
        cleaned = MultiSpace.Replace(cleaned, " ").Trim();

        var ep = EpisodeToken.Match(cleaned);
        if (ep.Success)
            return cleaned[..(ep.Index + ep.Length)].Trim();

        var yr = YearToken.Match(cleaned);
        if (yr.Success)
            return cleaned[..(yr.Index + yr.Length)].Trim();

        return cleaned;
    }

    public async Task<IEnumerable<RemoteSubtitleInfo>> Search(
        SubtitleSearchRequest request,
        CancellationToken cancellationToken)
    {
        var results = new List<RemoteSubtitleInfo>();

        var requestedIso = request.Language ?? "eng";
        if (!LangMap.TryGetValue(requestedIso, out var targetLangCodes))
            targetLangCodes = new[] { requestedIso.Length == 3 ? requestedIso[..2] : requestedIso };

        var queries = BuildQueries(request);
        if (queries.Count == 0)
            return results;

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var query in queries)
        {
            try
            {
                var found = await SearchQuery(client, query, targetLangCodes, requestedIso, cancellationToken)
                    .ConfigureAwait(false);
                foreach (var r in found)
                {
                    if (seenIds.Add(r.Id))
                        results.Add(r);
                }

                // First query that yields matches wins — avoids extra requests.
                if (results.Count > 0)
                    break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubtitleCat search failed for query: {Query}", query);
            }
        }

        _logger.LogDebug("SubtitleCat found {Count} subtitles ({Tried} queries tried)", results.Count, queries.Count);
        return results;
    }

    private async Task<List<RemoteSubtitleInfo>> SearchQuery(
        HttpClient client,
        string query,
        string[] targetLangCodes,
        string requestedIso,
        CancellationToken cancellationToken)
    {
        var searchUrl = $"{BaseUrl}/index.php?search={Uri.EscapeDataString(query)}";
        _logger.LogDebug("SubtitleCat search: {Url}", searchUrl);

        var searchHtml = await client.GetStringAsync(searchUrl, cancellationToken).ConfigureAwait(false);

        var pageLinks = Regex.Matches(searchHtml, @"href=""(subs/(\d+)/([^""]+)\.html)""")
            .Cast<Match>()
            .Select(m => (RelUrl: m.Groups[1].Value, Id: m.Groups[2].Value, File: m.Groups[3].Value))
            .DistinctBy(x => x.Id)
            .Take(8)
            .ToList();

        var tasks = pageLinks.Select(async link =>
        {
            try
            {
                var pageUrl = $"{BaseUrl}/{link.RelUrl}";
                var pageHtml = await client.GetStringAsync(pageUrl, cancellationToken).ConfigureAwait(false);

                var srtLinks = Regex.Matches(pageHtml, @"href=""(/subs/\d+/[^""]+\.srt)""")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ToList();

                var pageResults = new List<RemoteSubtitleInfo>();
                foreach (var srtUrl in srtLinks)
                {
                    var detectedLang = ExtractLangSuffix(srtUrl);
                    if (detectedLang == null) continue;

                    var matchesRequested = targetLangCodes.Any(code =>
                        string.Equals(detectedLang, code, StringComparison.OrdinalIgnoreCase));
                    if (!matchesRequested) continue;

                    var isoLang = ReverseLangMap.TryGetValue(detectedLang, out var iso) ? iso : requestedIso;
                    var displayName = Path.GetFileNameWithoutExtension(srtUrl);
                    var fullUrl = BaseUrl + srtUrl;

                    pageResults.Add(new RemoteSubtitleInfo
                    {
                        Id = EncodeId(fullUrl),
                        Name = displayName,
                        ThreeLetterISOLanguageName = isoLang,
                        Format = "srt",
                        ProviderName = Name,
                        Forced = false,
                        HearingImpaired = false,
                    });
                }
                return pageResults;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SubtitleCat: error fetching {Url}", link.RelUrl);
                return Enumerable.Empty<RemoteSubtitleInfo>();
            }
        });

        var allResults = await Task.WhenAll(tasks).ConfigureAwait(false);
        return allResults.SelectMany(r => r).ToList();
    }

    private static string? ExtractLangSuffix(string srtUrl)
    {
        var match = Regex.Match(srtUrl, @"-([a-z]{2}(?:-[a-zA-Z0-9]+)?)\.srt$", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    public async Task<SubtitleResponse> GetSubtitles(string id, CancellationToken cancellationToken)
    {
        var url = DecodeId(id);
        _logger.LogDebug("SubtitleCat downloading: {Url}", url);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        var langSuffix = ExtractLangSuffix(url);
        var language = langSuffix != null && ReverseLangMap.TryGetValue(langSuffix, out var iso) ? iso : string.Empty;

        var stream = await client.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);

        return new SubtitleResponse
        {
            Format = "srt",
            Language = language,
            Stream = stream,
        };
    }
}
