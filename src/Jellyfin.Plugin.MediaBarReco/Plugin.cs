using Jellyfin.Plugin.MediaBarReco.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.MediaBarReco;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public static Plugin? Instance { get; private set; }

    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public override string Name => "Media Bar Recommendations";

    public override Guid Id => new Guid("c7a3f2e1-9b4d-4e8a-b6c0-d2e5f1a3b7c9");

    public override string Description =>
        "Scores unwatched movies from your watch history and auto-updates a playlist used by the Media Bar plugin.";

    public IEnumerable<PluginPageInfo> GetPages() =>
    [
        new PluginPageInfo
        {
            Name = "Media Bar Recommendations",
            EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.config.html"
        }
    ];
}
