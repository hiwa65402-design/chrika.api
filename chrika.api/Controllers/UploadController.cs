// Controllers/UploadController.cs
using Chrika.Api.Models;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/upload")] // ناوی Routeـەکە /api/upload دەبێت
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IFileService _fileService;

        public UploadController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // === تەنها یەک Endpointـی گشتگیر بۆ هەموو جۆرە فایلێک ===
        // POST: /api/upload
        [HttpPost]
        [Consumes("multipart/form-data")] 
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] FileType fileType)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var fileUrl = await _fileService.SaveFileAsync(file, fileType);

            return Ok(new { url = fileUrl });
        }
    }
}
