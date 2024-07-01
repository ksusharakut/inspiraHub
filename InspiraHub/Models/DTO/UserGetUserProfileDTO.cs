namespace InspiraHub.Models.DTO
{
    public class UserGetUserProfileDTO
    {
        public long Id { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateOnly DateBirth { get; set; }

        public string Role { get; set; } = null!;

    }
}
