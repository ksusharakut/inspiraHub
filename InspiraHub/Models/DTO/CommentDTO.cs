namespace InspiraHub.Models.DTO
{
    public class CommentDTO
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public long ContentId { get; set; }

        public string UserComment { get; set; } = null!;

        public DateTime CreateAt { get; set; }

        public string UserName { get; set; } = null!;
    }
}
