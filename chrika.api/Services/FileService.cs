// Services/FileService.cs (وەشانی گەرەنتی)
using Chrika.Api.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class FileService : IFileService
    {
        // === گۆڕانکاری: هیچ شتێکمان پێویست نییە لێرە ===
        public FileService() { }

        public async Task<string> SaveFileAsync(IFormFile file, FileType fileType)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            // === گۆڕانکاری: ڕاستەوخۆ لە ڕەگی پڕۆژەکە فۆڵدەر دروست دەکەین ===
            var baseFolder = "Uploads"; // ناوی فۆڵدەرە سەرەکییەکە
            string subfolder = GetSubfolderForFileType(fileType);
            // Path.Combine دڵنیادەبێتەوە کە / و \ بە دروستی بەکاردێن
            var targetFolder = Path.Combine(Directory.GetCurrentDirectory(), baseFolder, subfolder);
            // ==========================================================

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

            // === گۆڕانکاری: URLـەکە بە شێوەی relative دروست دەکەین ===
            // دڵنیادەبینەوە کە هەمیشە forward slash بەکاردێت
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
