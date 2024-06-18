namespace InspiraHub.Models.DTO
{
    public class ContentDTO
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public string Preview { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime CreateAt { get; set; }

        public string ContentType { get; set; } = null!;
    }
}
