namespace InspiraHub.Models.DTO
{
    public class UserGetAllUsersDTO
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
        public string Role { get; set; } = null!;
    }
}
