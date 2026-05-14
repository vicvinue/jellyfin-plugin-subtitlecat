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

    public override string Name => "SubMichi";
    public override Guid Id => new Guid("b3c4d5e6-f7a8-9012-cdef-012345678901");
    public override string Description => "Descarga subtítulos desde SubtitleCat (subtitlecat.com)";

    public static Plugin? Instance { get; private set; }
}
