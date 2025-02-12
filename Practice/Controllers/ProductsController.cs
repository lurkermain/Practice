using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Configuration;
using Practice.Enums;
using Practice.Helpers;
using Practice.Models;

namespace Practice.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Преобразуем данные в DTO
            var productDto = new ProductUrl
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ModelType = product.ModelType,
                ImageUrl = Url.Action("GetImage", new { id = product.Id }) // Генерация ссылки на метод GetImage
            };

            return Ok(productDto); // Возвращаем JSON-объект
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetImage(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || product.Image == null)
            {
                return NotFound();
            }

            return File(product.Image, "image/jpg");
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string name, [FromForm] string description, [FromForm] ModelType modeltype, IFormFile? image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            // Читаем изображение в байтовый массив
            var imageBytes = await FileHelper.ConvertToByteArrayAsync(image);

            // Создаем объект Product
            var product = new Product
            {
                Name = name,
                Description = description,
                ModelType = modeltype.ToString(), // Преобразуем в строку для хранения
                Image = imageBytes
            };

            // Добавляем в базу данных
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] string name, [FromForm] string description, [FromForm] ModelType modeltype, IFormFile? image)
        {

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (image != null && image.Length > 0)
            {
                var imageBytes = await FileHelper.ConvertToByteArrayAsync(image);
                existingProduct.Image = imageBytes;
            }

            existingProduct.Name = name;
            existingProduct.Description = description;
            existingProduct.ModelType = modeltype.ToString();

            _context.Entry(existingProduct).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
