// Controllers/FilesController.cs

using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// وێنەیەک بار دەکات و URLـەکەی دەگەڕێنێتەوە.
        /// </summary>
        [HttpPost("upload/image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            // فایلەکە لە فۆڵدەری 'images' هەڵدەگرین
            var fileUrl = await _fileService.SaveFileAsync(file, "images");
            return Ok(new { url = fileUrl });
        }

        /// <summary>
        /// ڤیدیۆیەک بار دەکات و URLـەکەی دەگەڕێنێتەوە.
        /// </summary>
        [HttpPost("upload/video")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            // فایلەکە لە فۆڵدەری 'videos' هەڵدەگرین
            var fileUrl = await _fileService.SaveFileAsync(file, "videos");
            return Ok(new { url = fileUrl });
        }
    }
}
