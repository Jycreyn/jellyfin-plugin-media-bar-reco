using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.MediaBarReco.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public Guid UserId { get; set; } = Guid.Empty;

    public string PlaylistName { get; set; } = "Featured Recommendations";

    public int TopN { get; set; } = 15;

    // Weights — don't need to sum to 1
    public double RatingWeight { get; set; } = 0.5;

    public double GenreWeight { get; set; } = 0.3;

    public double RecencyWeight { get; set; } = 0.2;

    // Movies added within this many days get a recency bonus
    public int RecencyDays { get; set; } = 90;
}
