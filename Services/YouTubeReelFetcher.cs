using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Reels.Data;
using Reels.Models;

namespace Reels.Services;

public class YouTubeReelFetcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _http;

    public YouTubeReelFetcher(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        IHttpClientFactory http)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _http = http;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Fetch();
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task Fetch()
    {
        var apiKey = _config["YouTube:ApiKey"];

        var url =
            "https://www.googleapis.com/youtube/v3/search" +
            "?part=snippet" +
            "&type=video" +
            "&videoDuration=short" +
            "&videoEmbeddable=true" +
            "&q=coding programming csharp sql php" +
            "&maxResults=25" +
            "&key=" + apiKey;

        var client = _http.CreateClient();
        var json = await client.GetStringAsync(url);

        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var item in items.EnumerateArray())
        {
            var videoId = item.GetProperty("id").GetProperty("videoId").GetString();
            var title = item.GetProperty("snippet").GetProperty("title").GetString();
            var publishedAt = item.GetProperty("snippet").GetProperty("publishedAt").GetDateTime();

            if (videoId == null) continue;

            if (await db.Reels.AnyAsync(r => r.VideoId == videoId))
                continue;

            db.Reels.Add(new Reel
            {
                VideoId = videoId,
                Title = title!,
                PublishedAt = publishedAt,
                Source = "YouTube"
            });
        }

        await db.SaveChangesAsync();
    }
}
