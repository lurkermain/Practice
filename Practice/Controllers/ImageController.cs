using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practice.Models;
using System;
using System.Diagnostics;

namespace Practice.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("products/{id}/rendered-image")]
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
        }

        [HttpPost("products/{id}/render")]
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
        }
    }
}
