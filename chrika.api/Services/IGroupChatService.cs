// Services/IGroupChatService.cs

using Chrika.Api.DTOs; // پێویستە DTOـەکان دروست بکەین
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IGroupChatService
    {
        /// <summary>
        /// لیستی ئەو گروپانە دەگەڕێنێتەوە کە بەکارهێنەر ئەندامە تێیاندا
        /// (بۆ پیشاندانیان لە لیستی چاتەکان)
        /// </summary>
        Task<IEnumerable<GroupConversationListItemDto>> GetMyGroupsAsync(int userId);

        /// <summary>
        /// هەموو نامەکانی ناو گروپێکی دیاریکراو دەگەڕێنێتەوە
        /// </summary>
        Task<IEnumerable<MessageDto>> GetGroupMessagesAsync(int groupId, int userId);

        /// <summary>
        /// نامەیەک بۆ گروپێکی دیاریکراو دەنێرێت
        /// </summary>
        Task<MessageDto> SendMessageToGroupAsync(int groupId, int senderId, SendMessageDto dto);
    }
}
