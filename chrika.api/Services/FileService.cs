// Services/FileService.cs
using Chrika.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileService(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        // === فانکشنە نوێ و گشتگیرەکە ===
        public async Task<string> SaveFileAsync(IFormFile file, FileType fileType)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            // 1. دیاریکردنی فۆڵدەری ئامانج بەپێی جۆری فایل
            string subfolder = GetSubfolderForFileType(fileType);
            var targetFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // 2. ناوێکی بێهاوتا بۆ فایلەکە دروست بکە
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            // 3. فایلەکە خەزن بکە
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 4. ناونیشانی گشتی (URL)ـی فایلەکە دروست بکە
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/uploads/{subfolder}/{uniqueFileName}";

            return fileUrl;
        }

        // فانکشنێکی یاریدەدەر بۆ وەرگرتنی ناوی فۆڵدەر
        private string GetSubfolderForFileType(FileType fileType)
        {
            return fileType switch
            {
                FileType.ProfilePicture => "profiles",
                FileType.PostImage => "posts/images",
                FileType.PostVideo => "posts/videos",
                FileType.ChatImage => "chat/images",
                FileType.ChatVideo => "chat/videos",
                FileType.ChatAudio => "chat/audios",
                _ => "general" // بۆ هەر حاڵەتێکی تر
            };
        }
    }
}
