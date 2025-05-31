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

        public UserModel(string username, string firstName, string lastName, string? email, string hashedPassword, string status = "DEFAULT")
        {
            Username = username.Trim();
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            if (email != "EMPTY")
                Email = email.Trim();
            PasswordHash = hashedPassword;
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
                if (string.IsNullOrEmpty(description))
                    throw new NullReferenceException("Error getting Description: description is null or empty.");
                else
                    return description;
            }
            set
            {
                if (value.Length > DescriptionMax)
                    throw new ArgumentException($"description is too long. Please enter a description under {DescriptionMax + 1} characters.");

                description = value;
            }
        }
        public static int DescriptionMax { get { return 50; } }
        public string? LastReadPassage
        {
            get
            {
                if (string.IsNullOrEmpty(lastReadPassage))
                    throw new NullReferenceException("Error getting lastReadPassage: lastReadPassage was null or empty.");
                else
                    return lastReadPassage;
            }
            set
            {
                if (value.Length > LastReadPassageMax)
                    throw new ArgumentException($"Critical error setting LastReadPassage: value is too long.");

                lastReadPassage = value;
            }
        }
        public static int LastReadPassageMax { get { return 10; } }
        public int? CurrentReadingPlan
        {
            get
            {
                if (currentReadingPlan == null)
                    throw new NullReferenceException("Error getting currentReadingPlan: value was null.");
                else
                    return currentReadingPlan;
            }
            set
            {
                if (value < 0 || value > 9999999999)
                    throw new ArgumentException("Critical error setting currentReadingPlan: Value was outside the allowed range.");

                currentReadingPlan = value;
            }
        }
        public int? LastPracticedVerse
        {
            get
            {
                if (lastPracticedVerse == null)
                    throw new NullReferenceException("Error getting lastPracticedVerse: value was null.");
                else
                    return lastPracticedVerse;
            }
            set
            {
                if (value < 0 || value > 9999999999)
                    throw new ArgumentException("Critical error setting lastPracticedVerse: Value was outside the allowed range.");

                lastPracticedVerse = value;
            }
        }
        public int IsDeleted
        {
            get { return isDeleted; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting isDeleted: value was outside the allowed range.");

                isDeleted = value;
            }
        }
        public string? ReasonDeleted
        {
            get
            {
                if (string.IsNullOrEmpty(reasonDeleted))
                    throw new NullReferenceException("Error getting reasonDeleted: reasonDeleted was null or empty.");
                else
                    return reasonDeleted;
            }
            set
            {
                if (value.Length > ReasonDeletedMax)
                    throw new ArgumentException($"Critical error setting reasonDeleted: value is too long.");

                reasonDeleted = value;
            }
        }
        public static int ReasonDeletedMax { get { return 100; } }
        public int AppTheme
        {
            get { return appTheme; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting appTheme: value was outside the allowed range.");

                appTheme = value;
            }
        }
        public int ShowVersesSaved
        {
            get { return showVersesSaved; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting showVersesSaved: value was outside the allowed range.");

                showVersesSaved = value;
            }
        }
        public int ShowPopularHighlights
        {
            get { return showPopularHighlights; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting showPopularHighlights: value was outside the allowed range.");

                showPopularHighlights = value;
            }
        }
        public int Flagged
        {
            get { return flagged; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting flagged: value was outside the allowed range.");

                flagged = value;
            }
        }
        public int AllowPushNotifications
        {
            get { return allowPushNotifications; }
            set
            {
                if (value < 0 || value > PushNotificationTypes)
                    throw new ArgumentException("Critical error setting allowPushNotifications: value was outside the allowed range.");

                allowPushNotifications = value;
            }
        }
        public static int PushNotificationTypes { get { return 2; } }
        public int FollowVerseOfTheDay
        {
            get { return followVerseOfTheDay; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting followVerseOfTheDay: value was outside the allowed range.");

                followVerseOfTheDay = value;
            }
        }
        public int Visibility
        {
            get { return visibility; }
            set
            {
                if (value < 0 || value > VisibilityTypes)
                    throw new ArgumentException("Critical error setting visibility: value was outside the allowed range.");

                visibility = value;
            }
        }
        public static int VisibilityTypes { get { return 3; } }
        public bool ForgotUsername { get; set; } // Ask users to change their username if this is true when logging in
    }
}
