// Services/FileService.cs
using Chrika.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Chrika.Api.Helpers; // ✅✅✅ هەنگاوی یەکەم: ئەمە زیاد بکە ✅✅✅

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

        // Services/FileService.cs

        public async Task<string> SaveFileAsync(IFormFile file, FileType fileType)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            // ١. دیاریکردنی فۆڵدەری ئامانج
            string subfolder = GetSubfolderForFileType(fileType);
            var targetFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // ٢. ناوێکی بێهاوتا بۆ فایلەکە دروست بکە
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            // ٣. فایلەکە خەزن بکە
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // ٤. ناونیشانی گشتی (URL) بە سادەترین شێوە دروست بکە
            //    ئەمە URLـێکی ڕێژەیی (relative) دروست دەکات کە هەمیشە کاردەکات
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
