using System;
public class Book
{
    public int BookID { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public string Publisher { get; set; }
    public int Year { get; set; }
    public string Genre { get; set; }
    public int PageCount { get; set; }
    /// <summary>
    /// Общее количество экземпляров (неизменяемый эталон)
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// Доступных для бронирования (уменьшается при брони, восстанавливается при отмене)
    /// </summary>
    public int AvailableCount { get; set; }
    public int WriteOffOriginalTotal { get; set; }
    /// <summary>
    /// Статус книги: "Available", "Booked", "WrittenOff"
    /// </summary>
    public string Status { get; set; }
    public DateTime AddedAt { get; set; }
    // Поля для сценария списания (сценарий 5)
    public string WriteOffReason { get; set; }
    public DateTime? WriteOffDate { get; set; }
    /// <summary>
    /// Возвращает true, если книга доступна для бронирования
    /// </summary>
    public bool IsAvailable => Status == "Available" && AvailableCount > 0;
    /// <summary>
    /// Книга может быть восстановлена из архива (п.14 сценария 5: в течение 24 часов)
    /// </summary>
    public bool CanBeRestored =>
        Status == "WrittenOff" &&
        WriteOffDate.HasValue &&
        (DateTime.Now - WriteOffDate.Value).TotalHours <= 24;
}

