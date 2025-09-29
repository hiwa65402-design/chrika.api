using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IFileService
    {
        // وێنەکە خەزن دەکات و ناونیشانی گشتی (URL) ـی وێنەکە دەگەڕێنێتەوە
        Task<string> SaveProfilePictureAsync(IFormFile imageFile);
    }
}
