namespace InspiraHub.Models.DTO
{
    public class UserRegistrationDTO
    {
        public string Username { get; set; } 
        public string Email { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateBirth { get; set; }
        public string Password { get; set; } 
    }
}
