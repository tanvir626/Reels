using System;
using Reels.Data;
using Microsoft.AspNetCore.Mvc;

namespace Reels.Controllers;

public class ReelsController : Controller
{
    private readonly AppDbContext _db;

    public ReelsController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var reels = _db.Reels
            .OrderByDescending(r => r.PublishedAt)
            .Take(20)
            .ToList();

        return View(reels);
    }
}
