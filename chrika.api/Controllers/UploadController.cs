// Controllers/UploadController.cs
using Chrika.Api.DTOs; // <-- گرنگ: ئەمە زیاد بکە
using Chrika.Api.Models;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto uploadDto)
        {
            if (uploadDto.File == null || uploadDto.File.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // ئێستا لەناو DTOـکەوە بەکاریاندەهێنین
            var fileUrl = await _fileService.SaveFileAsync(uploadDto.File, uploadDto.FileType);

            return Ok(new { url = fileUrl });
        }
    }
}
