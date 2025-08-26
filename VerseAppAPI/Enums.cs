namespace VerseAppAPI
{
    public static class Enums
    {
        public enum Status
        {
            Active = 0,
            Inactive = 1,
            Deleted = 2
        };

        public enum CollectionsSort
        {
            Newest = 0,
            Title = 1,
            LastPracticed = 2,
            Completion = 3,
            Custom = 4
        };

        public enum NotificationType
        {
            Single = 0,
            AllUsers = 1,
            Admins = 2,
            VerseOfTheDay = 3,
        }

        public const string DefaultCollectionsOrder = "none";
        public const CollectionsSort DefaultCollectionsSort = CollectionsSort.Newest;

        //
    }
}
