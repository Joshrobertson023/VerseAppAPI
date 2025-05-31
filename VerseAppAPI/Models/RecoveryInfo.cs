using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerseAppAPI.Models
{
    public class RecoveryInfo
    {
        private string username;
        private string firstName;
        private string lastName;
        private string? email;
        private string passwordHash;
        public int Id { get; set; }
        public string Username
        {
            get { return username; }
            set
            {
                if (value.Length > UsernameMax)
                    throw new ArgumentException($"Username is too long. Please enter a username under {UsernameMax + 1} characters.");

                username = value;
            }
        }
        public static int UsernameMax { get { return 18; } }
        public string FirstName
        {
            get { return firstName; }
            set
            {
                if (value.Length > NameMax)
                    throw new ArgumentException($"First name is too long. Please enter a name under {NameMax + 1} characters.");

                firstName = value.ToLower();
            }
        }
        public static int NameMax { get { return 15; } }
        public string LastName
        {
            get { return lastName; }
            set
            {
                if (value.Length > NameMax)
                    throw new ArgumentException($"Last name is too long. Please enter a name under {NameMax + 1} characters");

                lastName = value.ToLower();
            }
        }
        public string FullName
        {
            get
            {
                return (firstName.Substring(0, 1).ToUpper() + firstName.Substring(1) + " "
                        + lastName.Substring(0, 1).ToUpper() + lastName.Substring(1));
            }
        }
        public string? Email
        {
            get
            {
                if (string.IsNullOrEmpty(email))
                    throw new NullReferenceException("Error getting Email: email is null or empty.");
                else
                    return email;
            }
            set
            {
                if (value.Length > EmailMax)
                    throw new ArgumentException($"Email is too long. Please enter an email under {EmailMax + 1} characters.");

                email = value;
            }
        }
        public static int EmailMax { get { return 128; } }
        public string PasswordHash
        {
            get { return passwordHash; }
            set
            {
                if (value.Length > PasswordMax)
                    throw new ArgumentException($"Critical error setting PasswordHash: PasswordHash is too long.");

                passwordHash = value;
            }
        }
        public static int PasswordMax { get { return 128; } }
    }
}
