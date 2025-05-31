using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary.Models
{
    public class ReadingPlanModel
    {
        private int id;
        private int authorId;
        private string overview;
        private string description;
        private string passages;
        private int viewedBy;

        public int Id
        {
            get { return id; }
            set
            {
                if (value < -9999999999 || value > 9999999999)
                    throw new ArgumentException("Critical error setting id: Value was outside the allowed range.");

                id = value;
            }
        }

        public int AuthorId
        {
            get { return authorId; }
            set
            {
                if (value < -9999999999 || value > 9999999999)
                    throw new ArgumentException("Critical error setting authorId: Value was outside the allowed range.");

                authorId = value;
            }
        }

        public string Overview
        {
            get { return overview; }
            set
            {
                if (value.Length > OverviewMax)
                    throw new ArgumentException($"Overview is too long. Please keep it under {OverviewMax} characters.");

                overview = value;
            }
        }
        public int OverviewMax { get { return 100; } }

        public string Description
        {
            get { return description; }
            set
            {
                if (value.Length > DescriptionMax)
                    throw new ArgumentException($"Description is too long. Please keep it under {DescriptionMax} characters.");
            }
        }
        public int DescriptionMax { get { return 100; } }

        public string Passages
        {
            get { return passages; }
            set
            {
                if (value.Length > PassagesMax)
                    throw new ArgumentException($"Error: exceeded max number of passages.");
            }
        }
        public int PassagesMax { get { return 1000; } }

        public int ViewedBy
        {
            get { return viewedBy; }
            set
            {
                if (value >= 999)
                    viewedBy = 999;
                else
                    viewedBy = value;
            }
        }
    }
}
