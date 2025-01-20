using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Models;
using System;
using System.Diagnostics;
using System.Text;

namespace Practice.Controllers
{
    [ApiController]
    [Route("api/image")]
    public class ImageController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpPost("render")]
        public async Task<IActionResult> Render(int angle, int lightEnergy, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "Файл не был загружен." });
            }

            string uploadPath = Path.Combine("C:/blender_render/", file.FileName);
            string outputPath = "C:\\blender_render\\photo.png"; // Путь к выходному файлу
            try
            {
                // Сохраняем файл на сервере
                await using FileStream stream = new(uploadPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ошибка при сохранении файла: {ex.Message}" });
            }

            string blenderPath = @"X:\BlenderFoundation\Blender4.3\blender.exe";
            string scriptPath = @"X:\BlenderFoundation\Blender4.3\script3.py";

            var start = new ProcessStartInfo
            {
                FileName = blenderPath,
                Arguments = $"-b -P \"{scriptPath}\" -- {angle} {lightEnergy} \"{uploadPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = start };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) => outputBuilder.AppendLine(e.Data);
            process.ErrorDataReceived += (sender, e) => errorBuilder.AppendLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                return StatusCode(500, new { error = errorBuilder.ToString() });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(outputPath);
            return File(fileBytes, "image/png");
        }

        /*[HttpGet("products/{id}/rendered-image")]
        public IActionResult GetRenderedImage(int id)
        {
            //  изображения сохраняются в папке "C:\render"
            string imagePath = Path.Combine(@"C:\blender_render", $"left_{id}.png");

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound($"Rendered image for product {id} not found.");
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            return File(imageBytes, "image/png");
        }*/

        /*[HttpPost("products/{id}/render")]
        public async Task<IActionResult> RenderProductImage(int id, [FromQuery] float angle, [FromQuery] float light)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            string blenderExe = "\"X:\\BlenderFoundation\\Blender4.3\\blender.exe\"";
            string scriptPath = "\"X:\\BlenderFoundation\\Blender4.3\\script2.py\"";
            string filepath = $@"C:\blender_render\rendered_image_{id}.png";
            string modelpath = @"C:\blender_fruto\front_fruto_nyanya_half.gltf";
            string texturepath = @"C:\blender_fruto\front 1 bunny.png";

            string arguments = $"--background --python \"{scriptPath}\" -- --filepath \"{filepath}\" --modelpath \"{modelpath}\" --texturepath \"{texturepath}\" --angle {angle} --light {light}";


            var processStartInfo = new ProcessStartInfo
            {
                FileName = blenderExe,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            processStartInfo.Verb = "runas";


            // log
            Console.WriteLine($"Blender Path: {blenderExe}");
            Console.WriteLine($"Arguments: {arguments}");


            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return StatusCode(500, $"Error rendering image: {error}");
            }

            return Ok($"Image for product {id} rendered successfully.");
        }*/

    }
}
