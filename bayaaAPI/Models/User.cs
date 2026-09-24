namespace bayaaAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        // Authentication
        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        // Profile
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Gender { get; set; }

        public DateOnly? BirthDate { get; set; }

        // Address
        public string? Division { get; set; }

        public string? District { get; set; }

        public string? Area { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Review> Reviews { get; set; }
            = new List<Review>();
    }
}
