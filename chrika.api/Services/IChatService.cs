// Services/IChatService.cs
using Chrika.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IChatService
    {
        Task<IEnumerable<ConversationListItemDto>> GetConversationsAsync(int userId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, int userId);
        Task<MessageDto> SendMessageAsync(int conversationId, int senderId, SendMessageDto dto);
        Task<MessageDto> StartOrGetConversationAndSendMessageAsync(int senderId, int receiverId, SendMessageDto dto);
    }
}
