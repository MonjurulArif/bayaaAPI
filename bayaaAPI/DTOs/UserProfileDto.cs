namespace bayaaAPI.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public string? Gender { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? Division { get; set; }

        public string? District { get; set; }

        public string? Area { get; set; }

        public string? Address { get; set; }
    }
}
