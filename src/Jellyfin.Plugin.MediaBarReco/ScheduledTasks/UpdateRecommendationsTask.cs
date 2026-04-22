using System.Globalization;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.MediaBarReco.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Playlists;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediaBarReco.ScheduledTasks;

public class UpdateRecommendationsTask : IScheduledTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly IUserManager _userManager;
    private readonly IUserDataManager _userDataManager;
    private readonly IPlaylistManager _playlistManager;
    private readonly ILogger<UpdateRecommendationsTask> _logger;

    public UpdateRecommendationsTask(
        ILibraryManager libraryManager,
        IUserManager userManager,
        IUserDataManager userDataManager,
        IPlaylistManager playlistManager,
        ILogger<UpdateRecommendationsTask> logger)
    {
        _libraryManager = libraryManager;
        _userManager = userManager;
        _userDataManager = userDataManager;
        _playlistManager = playlistManager;
        _logger = logger;
    }

    public string Name => "Update Media Bar Recommendations";
    public string Key => "MediaBarUpdateRecommendations";
    public string Description => "Scores unwatched movies based on your watch history and updates the featured playlist used by the Media Bar plugin.";
    public string Category => "Media Bar Recommendations";

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance!.Configuration;

        if (config.UserId == Guid.Empty)
        {
            _logger.LogWarning("[MediaBarReco] No user configured. Open the plugin settings and select a user.");
            return;
        }

        var user = _userManager.GetUserById(config.UserId);
        if (user is null)
        {
            _logger.LogWarning("[MediaBarReco] Configured user not found.");
            return;
        }

        _logger.LogInformation("[MediaBarReco] Starting recommendation update for user '{User}'.", user.Username);
        progress.Report(5);

        // All movies visible to this user
        var allMovies = _libraryManager.GetItemList(new InternalItemsQuery(user)
        {
            IncludeItemTypes = [BaseItemKind.Movie],
            IsVirtualItem = false,
            Recursive = true
        });

        progress.Report(20);

        // Split watched / unwatched
        var watched = allMovies
            .Where(m => _userDataManager.GetUserData(user, m).Played)
            .ToList();

        var unwatched = allMovies
            .Where(m => !_userDataManager.GetUserData(user, m).Played)
            .ToList();

        _logger.LogInformation("[MediaBarReco] {Watched} watched, {Unwatched} unwatched movies found.", watched.Count, unwatched.Count);

        progress.Report(35);

        // Genre frequency from watch history
        var genreFreq = watched
            .SelectMany(m => m.Genres ?? [])
            .GroupBy(g => g, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var maxFreq = genreFreq.Values.DefaultIfEmpty(1).Max();

        // Score each unwatched movie
        var now = DateTime.UtcNow;
        var topIds = unwatched
            .Select(movie =>
            {
                double score = 0;

                // Community rating (0–10 → 0–1)
                if (movie.CommunityRating.HasValue)
                    score += (movie.CommunityRating.Value / 10.0) * config.RatingWeight;

                // Genre affinity: sum of relative freq of matching genres, capped at 1
                if (movie.Genres is { Length: > 0 })
                {
                    var genreScore = movie.Genres
                        .Where(g => genreFreq.ContainsKey(g))
                        .Sum(g => genreFreq[g] / (double)maxFreq);
                    score += Math.Min(genreScore, 1.0) * config.GenreWeight;
                }

                // Recency bonus: linear decay over RecencyDays
                var age = (now - movie.DateCreated).TotalDays;
                if (age is >= 0 and <= 365)
                {
                    var window = (double)config.RecencyDays;
                    if (age <= window)
                        score += (1.0 - age / window) * config.RecencyWeight;
                }

                return (Id: movie.Id, Score: score);
            })
            .OrderByDescending(x => x.Score)
            .Take(config.TopN)
            .Select(x => x.Id)
            .ToArray();

        progress.Report(65);

        // Find existing managed playlist by name
        var existing = _libraryManager.GetItemList(new InternalItemsQuery(user)
        {
            IncludeItemTypes = [BaseItemKind.Playlist],
            Name = config.PlaylistName,
            Recursive = true
        }).FirstOrDefault() as Playlist;

        if (existing is not null)
        {
            _logger.LogInformation("[MediaBarReco] Updating existing playlist '{Name}' ({Id}).", existing.Name, existing.Id);

            // Get current entry IDs and remove them all
            var entryIds = existing.LinkedChildren
                .Where(c => c.ItemId.HasValue)
                .Select(c => c.ItemId!.Value.ToString("N", CultureInfo.InvariantCulture))
                .ToList();

            if (entryIds.Count > 0)
                await _playlistManager.RemoveItemFromPlaylistAsync(existing.Id.ToString(), entryIds).ConfigureAwait(false);

            await _playlistManager.AddItemToPlaylistAsync(existing.Id, topIds, user.Id).ConfigureAwait(false);
        }
        else
        {
            _logger.LogInformation("[MediaBarReco] Creating new playlist '{Name}'.", config.PlaylistName);

            await _playlistManager.CreatePlaylist(new PlaylistCreationRequest
            {
                Name = config.PlaylistName,
                ItemIdList = topIds,
                UserId = user.Id,
                MediaType = MediaType.Video
            }).ConfigureAwait(false);
        }

        progress.Report(100);
        _logger.LogInformation("[MediaBarReco] Done — playlist '{Name}' now has {Count} recommendations.", config.PlaylistName, topIds.Length);
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() =>
    [
        new TaskTriggerInfo
        {
            Type = TaskTriggerInfoType.DailyTrigger,
            TimeOfDayTicks = TimeSpan.FromHours(3).Ticks
        }
    ];
}
