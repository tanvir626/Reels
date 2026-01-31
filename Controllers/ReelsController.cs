using Microsoft.AspNetCore.Mvc;
using Reels.Data;

namespace Reels.Controllers
{
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
                .Take(50)
                .ToList();

            return View(reels);
        }

        [HttpGet]
public IActionResult Load(int skip = 0, int take = 20)
{
    var reels = _db.Reels
        .OrderByDescending(r => r.PublishedAt)
        .Skip(skip)
        .Take(take)
        .Select(r => r.VideoId)
        .ToList();

    return Json(reels);
}
    }
}
