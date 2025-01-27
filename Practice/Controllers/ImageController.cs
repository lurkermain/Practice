using IronPython.Runtime.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Configuration;
using Practice.Models;
using System;
using System.Diagnostics;
using System.Text;
using Practice.Helpers;
using Practice.Enums;

namespace Practice.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ImageController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpPut("{id}/render")]
        public async Task<IActionResult> RenderModel(int id, int angle, int lightEnergy)
        {
            /*var renderItem = await _context.Render.FindAsync(id);
            if (renderItem == null)
            {
                return NotFound(new { error = "Модель не найдена в базе данных." });
            }*/

            var skin = await _context.Products.FindAsync(id);
            if (skin == null)
            {
                return NotFound(new { error = "Не найдено"});
            }

            var renderedItem = new Render() {Angle = angle, Light = lightEnergy, Skin = skin.Image };


            var blend_file = await _context.Blender
                                           .FirstOrDefaultAsync(p => p.ModelType == skin.ModelType.ToString());

            var blend_bytes = blend_file.Blender_file;



            if (skin.Image == null || skin.Image.Length == 0)
            {
                return BadRequest(new { error = "Текстура не была загружена." });
            }

            string blenderPath = @"X:\BlenderFoundation\Blender4.3\blender.exe";
            string scriptPath = @"X:\BlenderFoundation\Blender4.3\script3.py";
/*          string blendFilePath = @"C:/Users/jenya/Downloads/Telegram Desktop/banka3ReadyToo.blend";*/

            try
            {
                /*if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }*/

                string outputPath = Path.Combine(Path.GetTempPath(), "rendered_image.png");

                // Сохранение текстуры
                string tempSkinPath = Path.Combine(Path.GetTempPath(), "skin.png");
                using (var stream = new FileStream(tempSkinPath, FileMode.Create))
                {
                    stream.Write(skin.Image);
                }

                string tempBlenderFilePath = Path.Combine(Path.GetTempPath(), "banka.blend");
                using (var stream = new FileStream(tempBlenderFilePath, FileMode.Create))
                {
                    stream.Write(blend_bytes);
                }

                // Запуск Blender с .blend файлом
                var start = new ProcessStartInfo
                {
                    FileName = blenderPath,
                    Arguments = $"-b \"{tempBlenderFilePath}\" -P \"{scriptPath}\" -- {angle} {lightEnergy} \"{tempSkinPath}\" \"{outputPath}\"",
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
            // Поиск записи по полю ModelType
            var list = await _context.Blender.ToListAsync();

            // Проверяем, найден ли объект
            if (list == null)
            {
                return NotFound(new { message = "Модель не найдена" });
            }

            return Ok(list);
        }

        [HttpPost("model")]
        public async Task<IActionResult> AddModel([FromForm] ModelType modeltype, IFormFile Blender_file)
        {
            if (Blender_file == null || Blender_file.Length == 0)
            {
                return BadRequest(new { error = "Необходимо загрузить .blend" });
            }

            try
            {
                // Сохранение .gltf файла во временный массив
                using var blender_filebytes = new MemoryStream();
                await Blender_file.CopyToAsync(blender_filebytes);
                byte[] fileBytes = blender_filebytes.ToArray();

                // Создание новой записи модели
                var newModel = new Blender
                {
                    ModelType = modeltype.ToString(),
                    Blender_file = fileBytes,
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
