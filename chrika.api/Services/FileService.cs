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

            string subfolder = GetSubfolderForFileType(fileType);
            var targetFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // === گرنگترین بەش ===
            // URLـێکی ڕێژەیی دروست دەکەین کە هەمیشە کاردەکات
            var fileUrl = $"/uploads/{subfolder}/{uniqueFileName}";

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
