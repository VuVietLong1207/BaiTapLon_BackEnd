using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalBlog.Models;
using System.Security.Claims;

namespace PersonalBlog.Controllers
{
    public class PostController : Controller
    {
        private readonly BlogContext _context;
        public PostController(BlogContext context) { _context = context; }

        public IActionResult Index(string searchString)
        {
            var posts = from p in _context.Posts
                        select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(s => (s.Title != null && s.Title.Contains(searchString)) ||
                                         (s.Category != null && s.Category.Contains(searchString)));
            }

            return View(posts.OrderByDescending(p => p.CreatedAt).ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var comments = await _context.Comments
                .Where(c => c.PostId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var model = Tuple.Create(post, comments);
            return View(model); // ✅ đúng kiểu Tuple<Post, List<Comment>>
        }

        public IActionResult Create()
        {
            return View(new Post());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    post.ImageUrl = "/images/" + fileName;
                }

                post.CreatedAt = DateTime.Now;
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(post);
        }

        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);
            return post == null ? NotFound() : View(post);
        }

        [HttpPost]
        public IActionResult Edit(Post post)
        {
            if (ModelState.IsValid)
            {
                _context.Posts.Update(post);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }

        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            return post == null ? NotFound() : View(post);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var post = _context.Posts.Find(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.CreatedAt = DateTime.Now;
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = comment.PostId });
        }

    }
}
