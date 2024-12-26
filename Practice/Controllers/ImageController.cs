using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Models;
using System;

namespace Practice.Controllers
{
    /*[ApiController]
    [Route("[Controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImageController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .Select(p => new ProductUrl
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ModelType = p.ModelType,
                    ImageUrl = Url.Action("GetImage", new { id = p.Id }) // Генерация ссылки на метод GetImage
                })
                .ToListAsync();

            return Ok(products);
        }
        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetImage(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || product.Image == null)
            {
                return NotFound();
            }

            return File(product.Image, "image/postgresql);
        }


    }*/
}
