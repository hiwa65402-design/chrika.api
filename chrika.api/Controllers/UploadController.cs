// Controllers/UploadController.cs

using Chrika.Api.Models;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System; // <-- گرنگ: ئەمە زیاد بکە
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/upload")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IFileService _fileService;

        public UploadController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // === گۆڕانکارییەکە لێرەدایە ===
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string fileType)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // 1. گۆڕینی string بۆ enum
            // Enum.TryParse(string, ignoreCase, out result)
            if (!Enum.TryParse<FileType>(fileType, true, out var fileTypeEnum))
            {
                // ئەگەر بەکارهێنەر stringـێکی هەڵەی ناردبوو
                return BadRequest("Invalid fileType specified.");
            }

            // 2. فایلەکە پاشەکەوت دەکەین بە بەکارهێنانی enumـە گۆڕدراوەکە
            var fileUrl = await _fileService.SaveFileAsync(file, fileTypeEnum);

            return Ok(new { url = fileUrl });
        }
    }
}
