using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InspiraHub.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
    }
}
