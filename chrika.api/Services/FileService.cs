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

            // ✅✅✅ گۆڕانکاری: ناوێکی جوانتر و مانادارتر بۆ فایلەکە دروست دەکەین ✅✅✅
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var randomPart = Guid.NewGuid().ToString().Substring(0, 8);
            var uniqueFileName = $"{timestamp}_{randomPart}{Path.GetExtension(file.FileName)}";

            var filePath = Path.Combine(targetFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // ✅✅✅ هەنگاوی دووەم: ناونیشانی گشتی (URL) بە شێوازی زیرەکانە دروست بکە ✅✅✅
            var request = _httpContextAccessor.HttpContext.Request;
            string host;

#if DEBUG
            // لە دۆخی دیبەگدا، IPی کۆمپیوتەرەکە بەکاربهێنە
            var ipAddress = UrlHelper.GetLocalIpAddress();
            var port = request.Host.Port;
            host = $"{ipAddress}:{port}";
#else
                // لە دۆخی Releaseدا، ناونیشانی سەرەکی بەکاربهێنە
                host = request.Host.ToString();
#endif

            var fileUrl = $"{request.Scheme}://{host}/uploads/{subfolder}/{uniqueFileName}";

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
