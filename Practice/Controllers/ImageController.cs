using IronPython.Runtime.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Configuration;
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
            var renderItem = await _context.Blender.FindAsync(id);
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
            string outputDir = @"C:\blender_render\";
            string outputPath = Path.Combine(outputDir, "rendered_image.png");

            try
            {
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Сохранение модели во временные файлы
                string tempGltfPath = Path.Combine(Path.GetTempPath(), "model.gltf");
                string tempBinPath = Path.Combine(Path.GetTempPath(), "model.bin");

                await System.IO.File.WriteAllBytesAsync(tempGltfPath, renderItem.Blender_model);
                await System.IO.File.WriteAllBytesAsync(tempBinPath, renderItem.Blender_bin);

                //logs
                if (!System.IO.File.Exists(tempGltfPath) || !System.IO.File.Exists(tempBinPath))
                {
                    return StatusCode(500, new { error = "GLTF или BIN файл не были созданы." });
                }

                // Сохранение текстуры
                string tempSkinPath = Path.Combine(Path.GetTempPath(), "skin.png");
                using (var stream = new FileStream(tempSkinPath, FileMode.Create))
                {
                    await skinFile.CopyToAsync(stream);
                }

                // Запуск Blender
                var start = new ProcessStartInfo
                {
                    FileName = blenderPath,
                    Arguments = $"-b -P \"{scriptPath}\" -- {angle} {lightEnergy} \"{tempGltfPath}\" \"{tempSkinPath}\" \"{outputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = start };
                process.Start();

                string outputLog = await process.StandardOutput.ReadToEndAsync();
                string errorLog = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0 || !System.IO.File.Exists(outputPath))
                {
                    return StatusCode(500, new { error = $"Blender завершился с ошибкой. Логи:\n{outputLog}\n{errorLog}" });
                }

                var renderedBytes = await System.IO.File.ReadAllBytesAsync(outputPath);

                // Удаление временных файлов
                System.IO.File.Delete(tempGltfPath);
                System.IO.File.Delete(tempBinPath);
                System.IO.File.Delete(tempSkinPath);
                System.IO.File.Delete(outputPath);

                return File(renderedBytes, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ошибка: {ex.Message}" });
            }
        }


        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            var models = await _context.Blender.Select(p => new Blender {Id = p.Id, Blender_model = p.Blender_model}).ToListAsync();

            return Ok(models);
        }

        [HttpPost("model")]
        public async Task<IActionResult> AddModel(IFormFile gltfFile, IFormFile binFile)
        {
            if (gltfFile == null || gltfFile.Length == 0 || binFile == null || binFile.Length == 0)
            {
                return BadRequest(new { error = "Необходимо загрузить оба файла: .gltf и .bin." });
            }

            try
            {
                // Сохранение .gltf файла во временный массив
                using var gltfStream = new MemoryStream();
                await gltfFile.CopyToAsync(gltfStream);
                byte[] gltfBytes = gltfStream.ToArray();

                // Сохранение .bin файла во временный массив
                using var binStream = new MemoryStream();
                await binFile.CopyToAsync(binStream);
                byte[] binBytes = binStream.ToArray();

                // Создание новой записи модели
                var newModel = new Blender
                {
                    Blender_model = gltfBytes,
                    Blender_bin = binBytes,
                };

                _context.Blender.Add(newModel);
                await _context.SaveChangesAsync();

                return Ok(new { id = newModel.Id, message = "Модель успешно добавлена." });
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
            var renderItem = await _context.Blender.FindAsync(id);
            if (renderItem == null)
            {
                return NotFound(new { error = "Модель с указанным идентификатором не найдена." });
            }

            try
            {
                _context.Blender.Remove(renderItem);
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
