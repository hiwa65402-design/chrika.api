namespace Chrika.Api.Services
{
    public interface IShareService
    {
        Task RecordShareAsync(int postId, int userId);
    }
}
