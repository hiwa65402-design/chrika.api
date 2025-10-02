// Hubs/ChatHub.cs
using Chrika.Api.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace Chrika.Api.Hubs
{
    // ئەم Hubـە بۆ ئێستا بەتاڵە، بەڵام دواتر بۆ کاری پێشکەوتوو بەکاریدێنین
    // وەک "User is typing..."
    public class ChatHub : Hub
    {
        // کاتێک بەکارهێنەرێک دەست بە نووسین دەکات
        public async Task UserStartedTyping(string receiverId)
        {
            var senderId = Context.User.GetUserId().ToString(); // IDی ئەو کەسەی کە دەنووسێت
                                                                // ئاگادارکردنەوەی وەرگرەکە کە نێرەر خەریکی نووسینە
            await Clients.User(receiverId).SendAsync("ReceiveTypingStarted", senderId);
        }

        // کاتێک بەکارهێنەرێک لە نووسین دەوەستێت
        public async Task UserStoppedTyping(string receiverId)
        {
            var senderId = Context.User.GetUserId().ToString(); // IDی ئەو کەسەی کە وەستاوە
                                                                // ئاگادارکردنەوەی وەرگرەکە کە نێرەر وەستاوە لە نووسین
            await Clients.User(receiverId).SendAsync("ReceiveTypingStopped", senderId);
        }

        // لەناو ChatHub.cs

        // ======================================================
        // ===          فانکشنەکانی تایبەت بە WebRTC Signaling          ===
        // ======================================================

        // ١. کاتێک بەکارهێنەرێک داوای پەیوەندی دەکات
        public async Task SendCallRequest(string receiverId)
        {
            var caller = Context.User; // زانیاری ئەو کەسەی پەیوەندی دەکات
            var callerId = caller.GetUserId().ToString();
            var callerUsername = caller.Claims.FirstOrDefault(c => c.Type == "username")?.Value ?? "Someone";

            // ناردنی داواکاری پەیوەندی بۆ وەرگرەکە
            await Clients.User(receiverId).SendAsync("ReceiveCallRequest", new { callerId, callerUsername });
        }

        // ٢. کاتێک وەرگر پەیوەندییەکە ڕەت دەکاتەوە
        public async Task SendCallDeclined(string callerId)
        {
            var receiverId = Context.User.GetUserId().ToString();
            // ئاگادارکردنەوەی داواکار کە پەیوەندییەکە ڕەتکرایەوە
            await Clients.User(callerId).SendAsync("ReceiveCallDeclined", new { receiverId });
        }

        // ٣. گواستنەوەی WebRTC Offer
        public async Task SendWebRTCOffer(string receiverId, string offer)
        {
            var senderId = Context.User.GetUserId().ToString();
            // ناردنی offer بۆ وەرگرەکە
            await Clients.User(receiverId).SendAsync("ReceiveWebRTCOffer", new { senderId, offer });
        }

        // ٤. گواستنەوەی WebRTC Answer
        public async Task SendWebRTCAnswer(string receiverId, string answer)
        {
            var senderId = Context.User.GetUserId().ToString();
            // ناردنی answer بۆ داواکارەکە
            await Clients.User(receiverId).SendAsync("ReceiveWebRTCAnswer", new { senderId, answer });
        }

        // ٥. گواستنەوەی ICE Candidates
        public async Task SendWebRTCIceCandidate(string receiverId, string candidate)
        {
            var senderId = Context.User.GetUserId().ToString();
            // ناردنی ICE candidate بۆ لایەنی بەرامبەر
            await Clients.User(receiverId).SendAsync("ReceiveWebRTCIceCandidate", new { senderId, candidate });
        }

        // ٦. کاتێک بەکارهێنەرێک پەیوەندییەکە دادەخاتەوە
        public async Task SendCallEnded(string receiverId)
        {
            var senderId = Context.User.GetUserId().ToString();
            // ئاگادارکردنەوەی لایەنی بەرامبەر کە پەیوەندییەکە کۆتایی هات
            await Clients.User(receiverId).SendAsync("ReceiveCallEnded", new { senderId });
        }

    }

}
