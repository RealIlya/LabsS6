using System;

public class WriteOffRecord
{
    public int RecordID { get; set; }
    public int BookID { get; set; }
    public string BookTitle { get; set; }
    public string BookAuthor { get; set; }
    public string BookISBN { get; set; }
    public int Count { get; set; }
    public string Reason { get; set; }
    public DateTime WriteOffDate { get; set; }

    // Восстановление возможно только в течение 24 часов после списания
    public bool CanBeRestored => (DateTime.Now - WriteOffDate).TotalHours <= 24;
}
