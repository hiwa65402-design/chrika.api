// Services/IFileService.cs

using Chrika.Api.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IFileService
    {
        // تەنها پێناسەی فانکشنەکە لێرە دەبێت. هیچ شتێکی تر نا.
        Task<string> SaveFileAsync(IFormFile file, FileType fileType);
    }
}
