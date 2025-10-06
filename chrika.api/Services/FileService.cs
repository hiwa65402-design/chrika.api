// Services/FileService.cs (وەشانی نوێ و ڕاستکراوە)
using Chrika.Api.Models;
using Microsoft.AspNetCore.Hosting; // ئەمە زیاد بکە
using Microsoft.AspNetCore.Http;
using System; // ئەمە زیاد بکە بۆ ArgumentException
using System.IO;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment; // زیادکرا

        // IWebHostEnvironment لێرە وەردەگرین
        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveFileAsync(IFormFile file, FileType fileType)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            // === گۆڕانکاری سەرەکی: بەکارهێنانی wwwroot ===
            // ڕێڕەوی wwwroot بە شێوەیەکی ستاندارد وەردەگرین
            var wwwRootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(wwwRootPath))
            {
                // ئەگەر wwwroot بوونی نەبوو، خۆمان دروستی دەکەین
                wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(wwwRootPath))
                {
                    Directory.CreateDirectory(wwwRootPath);
                }
            }

            var baseFolder = "Uploads";
            string subfolder = GetSubfolderForFileType(fileType);

            // ڕێڕەوی تەواو بۆ پاشەکەوتکردن لەناو wwwroot
            var targetFolder = Path.Combine(wwwRootPath, baseFolder, subfolder);
            // ==========================================================

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // ناوی فایلەکە وەک خۆی دەمێنێتەوە
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // === گۆڕانکاری: URLـەکە بەبێ wwwroot دروست دەکەین ===
            // چونکە سێرڤەر خۆی لە wwwroot دەگەڕێت
            var fileUrl = $"/{baseFolder}/{subfolder}/{uniqueFileName}".Replace("\\", "/");
            // =====================================================

            return fileUrl;
        }

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
                _ => "general"
            };
        }
    }
}
