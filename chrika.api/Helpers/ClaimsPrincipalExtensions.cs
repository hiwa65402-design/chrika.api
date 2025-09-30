using System.Security.Claims;

namespace Chrika.Api.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            // ئەمە IDی بەکارهێنەر لە Tokenـەکە دەردەهێنێت
            // NameIdentifier هەمان شوێنە کە IDی بەکارهێنەر تێیدا هەڵگیراوە لە کاتی دروستکردنی Token
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);

            // دڵنیادەبینەوە کە بەهایەکی دروستە و دەیکەین بە ژمارە
            if (int.TryParse(userIdString, out var userId))
            {
                return userId;
            }

            // ئەگەر بە هەر هۆکارێک IDەکە نەدۆزرایەوە یان ژمارە نەبوو، هەڵەیەک دروست دەکەین
            // چونکە هەر Endpointـێک کە [Authorize]ـی هەبێت، دەبێت IDی بەکارهێنەری هەبێت
            throw new InvalidOperationException("User ID not found in token.");
        }
    }
}
