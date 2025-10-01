// Services/IFileService.cs
using Chrika.Api.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IFileService
    {
        // تەنها ئەم فانکشنەمان پێویستە
        Task<string> SaveFileAsync(IFormFile file, FileType fileType);
    }
}
