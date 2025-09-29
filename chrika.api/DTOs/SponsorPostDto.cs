namespace Chrika.Api.Dtos
{
    public class SponsorPostDto
    {
        // بۆ نموونە، چەند ڕۆژ دەتەوێت سپۆنسەری بکەیت
        public int NumberOfDays { get; set; }
        public string? TargetLocation { get; set; } // null واتە هەموو شوێنەکان

    }
}
