using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using KhoyaPaya.Models;
namespace KhoyaPaya.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;
        public HomeController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _env = env;
        }
        public IActionResult Index()
        {
            List<ItemModel> items = new List<ItemModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT Id, Title, Description, Category, ImagePath, PostedBy, CreatedAt FROM items ORDER BY CreatedAt DESC";
                items = conn.Query<ItemModel>(sql).ToList();
            }

            return View(items);
        }
        [HttpPost]
        [Authorize]
        public IActionResult PostItem(string title, string description, string category, IFormFile imageFile)
        {
            string imagePath = "/images/logo.png";
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }

                imagePath = "/uploads/" + uniqueFileName;
            }

            string postedBy = User.Identity?.Name ?? "Anonymous";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO items (Title, Description, Category, ImagePath, PostedBy, CreatedAt) 
                               VALUES (@Title, @Description, @Category, @ImagePath, @PostedBy, @CreatedAt)";

                var parameters = new
                {
                    Title = title,
                    Description = description,
                    Category = string.IsNullOrEmpty(category) ? "LOST" : category,
                    ImagePath = imagePath,
                    PostedBy = postedBy,
                    CreatedAt = DateTime.Now
                };

                conn.Execute(sql, parameters);
            }

            return RedirectToAction("Index");
        }
    }
}