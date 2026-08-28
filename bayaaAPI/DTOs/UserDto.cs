namespace bayaaAPI.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public string? FullName { get; set; }

        public string? Gender { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? Division { get; set; }

        public string? District { get; set; }

        public string? Area { get; set; }

        public string? Address { get; set; }
    }
}
