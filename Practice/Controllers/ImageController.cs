using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Models;
using System;
using System.Diagnostics;
using System.Text;

namespace Practice.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ImageController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpPost("{id}/render")]
        public async Task<IActionResult> RenderModel(int id, int angle, int lightEnergy, IFormFile skinFile)
        {
            var renderItem = await _context.Render.FindAsync(id);
            if (renderItem == null)
            {
                return NotFound(new { error = "Модель не найдена в базе данных." });
            }

            if (skinFile == null || skinFile.Length == 0)
            {
                return BadRequest(new { error = "Текстура не была загружена." });
            }

            string blenderPath = @"X:\BlenderFoundation\Blender4.3\blender.exe";
            string scriptPath = @"X:\BlenderFoundation\Blender4.3\script3.py";
            string outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

            try
            {
                // Сохранение модели и текстуры во временные файлы
                string tempModelPath = Path.Combine(Path.GetTempPath(), "model.gltf");
                await System.IO.File.WriteAllBytesAsync(tempModelPath, renderItem.Model);

                string tempSkinPath = Path.Combine(Path.GetTempPath(), "skin.png");
                using (var stream = new FileStream(tempSkinPath, FileMode.Create))
                {
                    await skinFile.CopyToAsync(stream);
                }

                // Запуск Blender
                var start = new ProcessStartInfo
                {
                    FileName = blenderPath,
                    Arguments = $"-b -P \"{scriptPath}\" -- {angle} {lightEnergy} \"{tempModelPath}\" \"{tempSkinPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = start };
                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    return StatusCode(500, new { error = "Blender завершился с ошибкой." });
                }

                var renderedBytes = await System.IO.File.ReadAllBytesAsync(outputPath);

                // Очистка временных файлов
                System.IO.File.Delete(tempModelPath);
                System.IO.File.Delete(tempSkinPath);
                System.IO.File.Delete(outputPath);

                return File(renderedBytes, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ошибка: {ex.Message}" });
            }
        }

        [HttpPost("{id}/model")]
        public async Task<IActionResult> AddModel(IFormFile modelFile)
        {
            if (modelFile == null || modelFile.Length == 0)
            {
                return BadRequest(new { error = "Файл модели не был загружен." });
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await modelFile.CopyToAsync(memoryStream);
                byte[] modelBytes = memoryStream.ToArray();

                var newRender = new Render
                {
                    Model = modelBytes,
                    Angle = 0,  // Можно задать дефолтные значения
                    Light = 0,
                    Skin = null
                };

                _context.Render.Add(newRender);
                await _context.SaveChangesAsync();

                return Ok(new { id = newRender.Id, message = "Модель успешно добавлена." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ошибка при добавлении модели: {ex.Message}" });
            }
        }

        // Метод для удаления модели из базы данных
        [HttpDelete("{id}/model")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            var renderItem = await _context.Render.FindAsync(id);
            if (renderItem == null)
            {
                return NotFound(new { error = "Модель с указанным идентификатором не найдена." });
            }

            try
            {
                _context.Render.Remove(renderItem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Модель успешно удалена." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ошибка при удалении модели: {ex.Message}" });
            }
        }

    }
}
