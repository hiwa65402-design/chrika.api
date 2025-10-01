// Controllers/UploadController.cs
using Chrika.Api.Models;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpPost]
        [Consumes("multipart/form-data")] // ئەمە بۆ دڵنیایی با بمێنێتەوە
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string fileType)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (!Enum.TryParse<FileType>(fileType, true, out var fileTypeEnum)) return BadRequest("Invalid fileType specified.");

            var fileUrl = await _fileService.SaveFileAsync(file, fileTypeEnum);
            return Ok(new { url = fileUrl });
        }
    }
}
