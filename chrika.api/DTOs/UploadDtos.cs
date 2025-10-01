// DTOs/UploadDtos.cs
using Chrika.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chrika.Api.DTOs
{
    public class FileUploadDto
    {
        // خودی فایلەکە
        [FromForm]
        public IFormFile? File { get; set; }

        // جۆری فایلەکە
        [FromForm]
        public FileType FileType { get; set; }
        public double Score { get; set; } // نمرەی پۆستەکە

    }
}
