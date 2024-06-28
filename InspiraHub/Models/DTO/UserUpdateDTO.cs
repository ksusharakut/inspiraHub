﻿namespace InspiraHub.Models.DTO
{
    public class UserUpdateDTO
    {
        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime UpdatedAt { get; set; }

        public string Name { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateOnly DateBirth { get; set; }

        public string Role { get; set; } = null!;
    }
}
