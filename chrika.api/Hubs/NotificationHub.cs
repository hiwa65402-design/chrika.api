using Microsoft.AspNetCore.SignalR;
using Chrika.Api.Helpers; // <-- ئەمە بۆ GetUserId() پێویستە
using System.Threading.Tasks;

namespace Chrika.Api.Hubs
{
    // : Hub میراتگری دەکات لە کڵاسی Hubـی SignalR
    public class NotificationHub : Hub
    {
        // ئەم فانکشنە کاتێک کار دەکات کە بەکارهێنەرێک پەیوەندی دەکات
        public override async Task OnConnectedAsync()
        {
            // دڵنیابە کە بەکارهێنەر لۆگینی کردووە
            if (Context.User.Identity.IsAuthenticated)
            {
                // IDی بەکارهێنەرەکە لە تۆکنەکەوە وەردەگرین
                var userId = Context.User.GetUserId().ToString();

                // بەکارهێنەرەکە دەخەینە ناو گرووپێکی SignalR کە ناوی IDی خۆیەتی
                // ئەمە وا دەکات کە بتوانین نامەی تایبەت تەنها بۆ ئەو بەکارهێنەرە بنێرین
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        // (بۆ دواتر) دەتوانین فانکشنی تریش لێرە زیاد بکەین، بۆ نموونە بۆ چات
    }
}
