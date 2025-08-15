using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VerseAppAPI.Enums;

namespace VerseAppAPI.Models
{
    public class User
    {
        public string Username { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string AuthToken { get; set; }
        public int CollectionsSort { get; set; }
        public int CurrentReadingPlan { get; set; }
        public bool AccountDeleted { get; set; }
        public string ReasonDeleted { get; set; }
        public Enums.Status Status { get; set; }
        public string CollectionsOrder { get; set; }
        public string HashedPassword { get; set; }

        public DateTime DateRegistered { get; set; }
        public DateTime LastSeen { get; set; }
        public string Description { get; set; }
        public bool Flagged { get; set; }
        public int ProfileVisibility { get; set; }

        public User(string username, string firstName, string lastName, string email, string hashedPassword, string token, Enums.Status status = Enums.Status.Active)
        {
            Username = username.Trim();
            FName = firstName.Trim();
            LName = lastName.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                Email = email.Trim();
            }
            HashedPassword = hashedPassword;
            AuthToken = token;
            this.Status = status;
            CollectionsOrder = "none";
            CollectionsSort = 0;
        }

        public User() { }

        public string FullName
        {
            get
            { // Get readable full name
                if (FName == null || LName == null)
                    return string.Empty;

                return (FName.Substring(0, 1).ToUpper() + FName.Substring(1) + " "
                        + LName.Substring(0, 1).ToUpper() + LName.Substring(1));
            }
        }
    }
}
