using System;
using System.Text.Json;
using Reels.Data;
using Reels.Models;
using Microsoft.EntityFrameworkCore;

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
            try
            {
                await Fetch();
            }
            catch (Exception ex)
            {
                Console.WriteLine("YouTube Fetch Error: " + ex.Message);
            }

            await Task.Delay(TimeSpan.FromHours(8), stoppingToken);
        }
    }

    private async Task Fetch()
    {
        var apiKey = _config["YouTube:ApiKey"];

        var url =
            $"https://www.googleapis.com/youtube/v3/search" +
            $"?part=snippet&type=video&videoDuration=short" +
            $"&q=coding programming sql csharp php" +
            $"&maxResults=25&key={apiKey}";

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

            if (await db.Reels.AnyAsync(r => r.VideoId == videoId))
                continue;

            if (!IsCoding(title))
                continue;

            db.Reels.Add(new Reel
            {
                VideoId = videoId,
                Title = title,
                Tags = "coding",
                PublishedAt = publishedAt,
                Source = "YouTube"
            });
        }

        await db.SaveChangesAsync();
    }

    private bool IsCoding(string title)
    {
        var t = title.ToLower();
        return t.Contains("code") || t.Contains("sql") ||
               t.Contains("c#") || t.Contains("php");
    }
}
