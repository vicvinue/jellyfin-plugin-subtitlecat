using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.SubtitleCat;

public class Plugin : BasePlugin<BasePluginConfiguration>
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public override string Name => "SubtitleCat";
    public override Guid Id => new Guid("a2b3c4d5-e6f7-8901-bcde-f01234567890");
    public override string Description => "Download subtitles from SubtitleCat (subtitlecat.com)";

    public static Plugin? Instance { get; private set; }
}
