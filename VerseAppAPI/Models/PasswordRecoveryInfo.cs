namespace VerseAppAPI.Models
{
    public class PasswordRecoveryInfo
    {
        public string Username { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
    }
}
