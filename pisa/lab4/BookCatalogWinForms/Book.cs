using System;

namespace BookCatalogWinForms
{
    public class Book
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }
        public int AvailableCount { get; set; }
        public string Status { get; set; }
        public DateTime AddedAt { get; set; }

        public bool IsAvailable => Status == "Available" && AvailableCount > 0;

        public override string ToString()
        {
            return $"{Title} — {Author} ({Year})";
        }
    }
}
