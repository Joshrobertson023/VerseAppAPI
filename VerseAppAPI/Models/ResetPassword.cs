namespace VerseAppAPI.Models
{
    public class ResetPassword
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int Id { get; set; }
        public string? PasswordHash { get; set; }
        public string? Token { get; set; }
    }
}
