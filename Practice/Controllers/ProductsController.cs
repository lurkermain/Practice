using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Helpers;
using Practice.Models;

namespace Practice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Get All")]
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

        [HttpGet("{id} Get Info")]
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

        [HttpGet("{id} Get Image")]
        public async Task<IActionResult> GetImage(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || product.Image == null)
            {
                return NotFound();
            }

            return File(product.Image, "image/postgresql");
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] ProductCreate productDto)
        {
            if (productDto.Image == null || productDto.Image.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            // Читаем изображение в байтовый массив
            var imageBytes = await FileHelper.ConvertToByteArrayAsync(productDto.Image);

            // Создаем объект Product
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                ModelType = productDto.ModelType,
                Image = imageBytes
            };

            // Добавляем в базу данных
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("Edit")]
        public async Task<IActionResult> Update([FromForm] ProductCreate productDto)
        {
            if (productDto.Id != productDto.Id) return BadRequest("Product ID mismatch");

            var existingProduct = await _context.Products.FindAsync(productDto.Id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Если изображение передано, обновляем его
            if (productDto.Image != null && productDto.Image.Length > 0)
            {
                // Читаем изображение в байтовый массив
                var imageBytes = await FileHelper.ConvertToByteArrayAsync(productDto.Image);
                existingProduct.Image = imageBytes; // Обновляем изображение
            }

            // Обновляем остальные поля
            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.ModelType = productDto.ModelType;

            // Помечаем сущность как измененную
            _context.Entry(existingProduct).State = EntityState.Modified;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("Delete")]
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
