using System;

namespace BookCatalogWinForms
{
    public class Booking
    {
        public int BookingID { get; set; }
        public int BookID { get; set; }
        public int UserID { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public string BookISBN { get; set; }
        public DateTime BookingDate { get; set; }

        public override string ToString()
        {
            return $"{BookTitle} — {BookAuthor} ({BookingDate:dd.MM.yyyy})";
        }
    }
}
