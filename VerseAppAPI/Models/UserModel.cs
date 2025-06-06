using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerseAppAPI.Models
{
    public class UserModel
    {
        private int id;
        private string username;
        private string firstName;
        private string lastName;
        private string? email;
        private string passwordHash;
        private string status;
        private DateTime dateRegistered;
        private DateTime lastSeen;
        private string? description;
        private string? lastReadPassage;
        private int? currentReadingPlan;
        private int? lastPracticedVerse;
        private int isDeleted;
        private string? reasonDeleted;
        private int appTheme;
        private int showVersesSaved;
        private int showPopularHighlights;
        private int flagged;
        private int allowPushNotifications;
        private int followVerseOfTheDay;
        private int visibility;

        public UserModel(string username, string firstName, string lastName, string? email, string? securityQuestion, string? securityAnswer, string hashedPassword, string token, string status = "DEFAULT")
        {
            Username = username.Trim();
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                Email = email.Trim();
            }
            else
            {
                SecurityQuestion = securityQuestion.Trim();
                SecurityAnswer = securityAnswer.Trim();
            }
            PasswordHash = hashedPassword;
            AuthToken = token;
            Status = status;
        }

        public UserModel() { }

        public int Id
        {
            get { return id; }
            set
            {
                if (value < 0 || value > 9999999999)
                    throw new ArgumentException("Critical error setting id: Value was outside the allowed range.");

                id = value;
            }
        }
        public string AuthToken { get; set; }
        public string? SecurityQuestion { get; set; }
        public string? SecurityAnswer { get; set; }
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
            get { return email; }
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
        public string Status
        {
            get { return status; }
            set
            {
                if (value.Length > StatusMax)
                    throw new ArgumentException($"Critical error setting Status: value is too long.");

                status = value;
            }
        }
        public static int StatusMax { get { return 30; } }
        public DateTime DateRegistered
        {
            get { return dateRegistered; }
            set { dateRegistered = value; }
        }
        public DateTime LastSeen
        {
            get { return lastSeen; }
            set { lastSeen = value; }
        }
        public string? Description
        {
            get
            {
                    return description;
            }
            set
            {
                description = value;
            }
        }
        public static int DescriptionMax { get { return 50; } }
        public string? LastReadPassage
        {
            get
            {
                    return lastReadPassage;
            }
            set
            {
                lastReadPassage = value;
            }
        }
        public static int LastReadPassageMax { get { return 10; } }
        public int? CurrentReadingPlan
        {
            get
            {
                    return currentReadingPlan;
            }
            set
            {
                currentReadingPlan = value;
            }
        }
        public int? LastPracticedVerse
        {
            get
            {
                    return lastPracticedVerse;
            }
            set
            {
                lastPracticedVerse = value;
            }
        }
        public int IsDeleted
        {
            get { return isDeleted; }
            set
            {
                isDeleted = value;
            }
        }
        public string? ReasonDeleted
        {
            get
            {
                    return reasonDeleted;
            }
            set
            {
                reasonDeleted = value;
            }
        }
        public static int ReasonDeletedMax { get { return 100; } }
        public int AppTheme
        {
            get { return appTheme; }
            set
            {
                appTheme = value;
            }
        }
        public int ShowVersesSaved
        {
            get { return showVersesSaved; }
            set
            {
                showVersesSaved = value;
            }
        }
        public int ShowPopularHighlights
        {
            get { return showPopularHighlights; }
            set
            {
                showPopularHighlights = value;
            }
        }
        public int Flagged
        {
            get { return flagged; }
            set
            {
                flagged = value;
            }
        }
        public int AllowPushNotifications
        {
            get { return allowPushNotifications; }
            set
            {
                allowPushNotifications = value;
            }
        }
        public static int PushNotificationTypes { get { return 2; } }
        public int FollowVerseOfTheDay
        {
            get { return followVerseOfTheDay; }
            set
            {
                followVerseOfTheDay = value;
            }
        }
        public int Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
            }
        }
        public static int VisibilityTypes { get { return 3; } }
        public bool ForgotUsername { get; set; } // Ask users to change their username if this is true when logging in
    }
}
