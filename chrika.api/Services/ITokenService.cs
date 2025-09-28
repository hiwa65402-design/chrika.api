using Chrika.Api.Models;

namespace Chrika.Api.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
