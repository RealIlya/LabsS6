using System;

public class BookReservation  // было: Booking
{
    public int BookingID { get; set; }
    public int UserID { get; set; }
    public int BookID { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    // "Active" / "Queued" / "Completed" / "Cancelled" / "Expired"
    public string Status { get; set; }

    public int? QueuePosition { get; set; }
    public string CancelReason { get; set; }
    public DateTime? ClosedAt { get; set; }

    public bool IsExpired =>
        Status == "Active" && DateTime.Now > ExpiryDate;

    public bool IsEffectivelyActive =>
        Status == "Active" && !IsExpired;

    public bool IsQueued =>
        Status == "Queued" && QueuePosition.HasValue;
}

