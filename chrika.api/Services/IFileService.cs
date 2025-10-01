// Services/IFileService.cs

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IFileService
    {
        /// <summary>
        /// </summary>
        /// <param name="file">فایلەکە کە لە HTTP requestـەوە دێت.</param>
        /// <param name="subfolder">ناوی ژێر-فۆڵدەر لەناو 'uploads' (بۆ نموونە: "images", "videos").</param>
        /// <returns>URLـی گشتی فایلەکە.</returns>
        Task<string> SaveFileAsync(IFormFile file, string subfolder);
    }
}
