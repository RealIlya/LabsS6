using System;
using System.Collections.Generic;
using System.Linq;

public static class DataStore
{
    private static List<Book> _books = new List<Book>();
    private static List<User> _users = new List<User>();
    private static List<BookReservation> _bookings = new List<BookReservation>();
    // [FIX 1] Список записей о списании — отсутствовал в оригинале
    private static List<WriteOffRecord> _writeOffRecords = new List<WriteOffRecord>();

    private static int _nextBookId = 1;
    private static int _nextUserId = 1;
    private static int _nextBookingId = 1;
    // [FIX 1] Счётчик ID для записей списания
    private static int _nextRecordId = 1;

    static DataStore()
    {
        _users.Add(new User
        {
            UserID = _nextUserId++,
            FullName = "Администратор",
            Email = "admin@test.com",
            PasswordHash = User.HashPassword("admin123"),
            Role = "Admin",
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.Now
        });
        _users.Add(new User
        {
            UserID = _nextUserId++,
            FullName = "Иванов Иван",
            Email = "user@test.com",
            PasswordHash = User.HashPassword("user123"),
            Role = "User",
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.Now
        });

        _books.Add(new Book
        {
            BookID = _nextBookId++,
            Title = "Преступление и наказание",
            Author = "Ф.М. Достоевский",
            ISBN = "978-5-17-000001-1",
            Publisher = "АСТ",
            Year = 2020,
            Genre = "fiction",
            PageCount = 672,
            TotalCount = 3,
            AvailableCount = 3,
            Status = "Available",
            AddedAt = DateTime.Now
        });
        _books.Add(new Book
        {
            BookID = _nextBookId++,
            Title = "Война и мир",
            Author = "Л.Н. Толстой",
            ISBN = "978-5-17-000002-2",
            Publisher = "АСТ",
            Year = 2019,
            Genre = "fiction",
            PageCount = 1200,
            TotalCount = 2,
            AvailableCount = 2,
            Status = "Available",
            AddedAt = DateTime.Now
        });
        _books.Add(new Book
        {
            BookID = _nextBookId++,
            Title = "Основы программирования на C#",
            Author = "Герберт Шилдт",
            ISBN = "978-5-9963-0001-3",
            Publisher = "Вильямс",
            Year = 2021,
            Genre = "education",
            PageCount = 896,
            TotalCount = 5,
            AvailableCount = 5,
            Status = "Available",
            AddedAt = DateTime.Now
        });
        _books.Add(new Book
        {
            BookID = _nextBookId++,
            Title = "Чистый код",
            Author = "Роберт Мартин",
            ISBN = "978-5-9963-0002-4",
            Publisher = "Питер",
            Year = 2022,
            Genre = "education",
            PageCount = 464,
            TotalCount = 1,
            AvailableCount = 1,
            Status = "Available",
            AddedAt = DateTime.Now
        });
    }

    // =========================================================
    // КНИГИ
    // =========================================================

    public static List<Book> GetAllBooks() =>
        _books.Where(b => b.Status != "WrittenOff").ToList();

    public static List<Book> GetArchivedBooks() =>
        _books.Where(b => b.Status == "WrittenOff").ToList();

    public static Book GetBookById(int id) =>
        _books.FirstOrDefault(b => b.BookID == id);

    public static List<Book> FindDuplicatesByISBN(string isbn)
    {
        if (string.IsNullOrEmpty(isbn)) return new List<Book>();
        return _books.Where(b => b.ISBN == isbn && b.Status != "WrittenOff").ToList();
    }

    public static List<Book> SearchBooks(string title, string author, string genre, int? year)
    {
        return _books.Where(b =>
            b.Status != "WrittenOff" &&
            (string.IsNullOrEmpty(title) ||
                b.Title.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0) &&
            (string.IsNullOrEmpty(author) ||
                b.Author.IndexOf(author, StringComparison.OrdinalIgnoreCase) >= 0) &&
            (string.IsNullOrEmpty(genre) || genre == "" || b.Genre == genre) &&
            (!year.HasValue || b.Year == year.Value)
        ).ToList();
    }

    public static bool AddBook(Book book)
    {
        var existing = _books.FirstOrDefault(b =>
            b.ISBN == book.ISBN && b.Status != "WrittenOff");
        if (existing != null)
        {
            existing.AvailableCount += book.AvailableCount;
            existing.TotalCount += book.AvailableCount;
            return true;
        }
        book.BookID = _nextBookId++;
        book.AddedAt = DateTime.Now;
        book.Status = "Available";
        book.TotalCount = book.AvailableCount;
        _books.Add(book);
        return true;
    }

    public static WriteOffResult WriteOffBook(
        int bookId, string reason, int count, out int writtenOff)
    {
        writtenOff = 0;
        var book = _books.FirstOrDefault(b => b.BookID == bookId);
        if (book == null) return WriteOffResult.NotFound;

        if (count <= 0) count = book.AvailableCount;

        int activeBookings = _bookings.Count(b =>
            b.BookID == bookId && b.Status == "Active");
        int freeCount = book.AvailableCount;

        if (count <= freeCount)
        {
            writtenOff = count;
            book.AvailableCount -= count;
            book.TotalCount -= count;

            // [FIX 1] Создаём запись WriteOffRecord при ЛЮБОМ списании,
            // а не только при полном. Именно отсюда архив и должен заполняться.
            _writeOffRecords.Add(new WriteOffRecord
            {
                RecordID = _nextRecordId++,
                BookID = book.BookID,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                BookISBN = book.ISBN,
                Count = count,
                Reason = reason,
                WriteOffDate = DateTime.Now
            });

            if (book.AvailableCount == 0 && activeBookings == 0)
            {
                book.Status = "WrittenOff";
                book.WriteOffReason = reason;
                book.WriteOffDate = DateTime.Now;
                book.WriteOffOriginalTotal = book.TotalCount + count;
                return WriteOffResult.Success;
            }
            else if (book.AvailableCount == 0 && activeBookings > 0)
            {
                book.Status = "Booked";
                return WriteOffResult.PartialSuccess;
            }
            else
            {
                return WriteOffResult.PartialSuccess;
            }
        }
        else
        {
            // [FIX 2] Этот путь теперь достижим, потому что WriteOff.aspx.cs
            // больше не блокирует count > AvailableCount на уровне UI.
            return WriteOffResult.HasActiveBookings;
        }
    }

    public static WriteOffResult WriteOffBook(int bookId, string reason, int count = 0)
    {
        return WriteOffBook(bookId, reason, count, out _);
    }

    public static List<int> CancelBookingsForBook(int bookId)
    {
        var bookings = _bookings
            .Where(b => b.BookID == bookId && b.Status == "Active")
            .ToList();

        var affectedUserIds = new List<int>();
        foreach (var booking in bookings)
        {
            booking.Status = "Cancelled";
            booking.CancelReason = "AdminCancelled";
            booking.ClosedAt = DateTime.Now;
            affectedUserIds.Add(booking.UserID);
        }

        var book = _books.FirstOrDefault(b => b.BookID == bookId);
        if (book != null)
        {
            book.AvailableCount += affectedUserIds.Count;
            if (book.Status == "Booked")
                book.Status = "Available";
        }

        return affectedUserIds;
    }

    // =========================================================
    // АРХИВ СПИСАНИЙ
    // =========================================================

    // [FIX 1] Метод для получения записей архива — отсутствовал в оригинале
    public static List<WriteOffRecord> GetWriteOffRecords() =>
        _writeOffRecords.OrderByDescending(r => r.WriteOffDate).ToList();

    // [FIX 1] Восстановление конкретной записи из архива по RecordID.
    // Оригинальный RestoreBook(bookId) работал только для полностью
    // списанных книг; частичные записи он найти не мог.
    public static bool RestoreFromRecord(int recordId)
    {
        var record = _writeOffRecords.FirstOrDefault(r => r.RecordID == recordId);
        if (record == null || !record.CanBeRestored) return false;

        var book = _books.FirstOrDefault(b => b.BookID == record.BookID);
        if (book == null) return false;

        book.AvailableCount += record.Count;
        book.TotalCount += record.Count;

        if (book.Status == "WrittenOff" || book.Status == "Booked")
            book.Status = "Available";

        // Сбрасываем поля книги, если она была полностью списана
        if (book.Status == "Available")
        {
            book.WriteOffReason = null;
            book.WriteOffDate = null;
            book.WriteOffOriginalTotal = 0;
        }

        _writeOffRecords.Remove(record);
        return true;
    }

    // Оставляем для обратной совместимости — используется в RestoreBook
    public static bool RestoreBook(int bookId)
    {
        var book = _books.FirstOrDefault(b => b.BookID == bookId);
        if (book == null || !book.CanBeRestored) return false;

        if (book.WriteOffOriginalTotal > 0)
        {
            book.AvailableCount = book.WriteOffOriginalTotal;
            book.TotalCount = book.WriteOffOriginalTotal;
        }
        else
        {
            book.AvailableCount = book.TotalCount;
        }

        book.Status = "Available";
        book.WriteOffReason = null;
        book.WriteOffDate = null;
        book.WriteOffOriginalTotal = 0;
        return true;
    }

    // =========================================================
    // ПОЛЬЗОВАТЕЛИ
    // =========================================================

    public static User AuthenticateUser(string email, string password)
    {
        var user = _users.FirstOrDefault(u => u.Email == email && u.IsActive);
        if (user != null && user.VerifyPassword(password)) return user;
        return null;
    }

    public static bool RegisterUser(string fullName, string email, string password)
    {
        if (_users.Any(u => u.Email == email)) return false;
        _users.Add(new User
        {
            UserID = _nextUserId++,
            FullName = fullName,
            Email = email,
            PasswordHash = User.HashPassword(password),
            Role = "User",
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.Now
        });
        return true;
    }

    public static bool UserExists(string email) =>
        _users.Any(u => u.Email == email);

    public static User GetUserById(int id) =>
        _users.FirstOrDefault(u => u.UserID == id);

    public static User GetUserByEmail(string email) =>
        _users.FirstOrDefault(u => u.Email == email);

    public static bool ResetPassword(int userId, string newPassword)
    {
        var user = _users.FirstOrDefault(u => u.UserID == userId);
        if (user == null) return false;
        user.PasswordHash = User.HashPassword(newPassword);
        return true;
    }

    public static List<User> GetAllUsers(int excludeAdminId)
    {
        return _users
            .Where(u => u.UserID != excludeAdminId)
            .OrderBy(u => u.CreatedAt)
            .ToList();
    }

    public static bool ToggleUserActive(int userId)
    {
        var user = _users.FirstOrDefault(u => u.UserID == userId);
        if (user == null) return false;
        user.IsActive = !user.IsActive;
        return true;
    }

    public static bool ConfirmUserEmail(int userId)
    {
        var user = _users.FirstOrDefault(u => u.UserID == userId);
        if (user == null || user.IsEmailConfirmed) return false;
        user.IsEmailConfirmed = true;
        return true;
    }

    public static bool ToggleAdminRole(int userId)
    {
        var user = _users.FirstOrDefault(u => u.UserID == userId);
        if (user == null) return false;
        user.Role = user.Role == "Admin" ? "User" : "Admin";
        return true;
    }

    // =========================================================
    // БРОНИРОВАНИЯ
    // =========================================================

    public static BookingResult CreateBooking(int userId, int bookId)
    {
        var book = _books.FirstOrDefault(b => b.BookID == bookId);
        if (book == null) return BookingResult.NotFound;

        var user = _users.FirstOrDefault(u => u.UserID == userId);
        if (user == null) return BookingResult.NotFound;

        if (GetUserActiveBookingsCount(userId) >= user.MaxBookings)
            return BookingResult.LimitExceeded;

        if (!book.IsAvailable)
            return BookingResult.NotAvailable;

        _bookings.Add(new BookReservation
        {
            BookingID = _nextBookingId++,
            UserID = userId,
            BookID = bookId,
            BookingDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddDays(3),
            Status = "Active"
        });

        book.AvailableCount--;
        if (book.AvailableCount == 0)
            book.Status = "Booked";

        return BookingResult.Success;
    }

    public static bool AddToQueue(int userId, int bookId)
    {
        if (_bookings.Any(b => b.UserID == userId && b.BookID == bookId
                               && b.Status == "Queued"))
            return false;

        int pos = _bookings.Count(b => b.BookID == bookId && b.Status == "Queued") + 1;
        _bookings.Add(new BookReservation
        {
            BookingID = _nextBookingId++,
            UserID = userId,
            BookID = bookId,
            BookingDate = DateTime.Now,
            ExpiryDate = DateTime.MaxValue,
            Status = "Queued",
            QueuePosition = pos
        });
        return true;
    }

    public static bool CancelBooking(int bookingId, int userId)
    {
        var booking = _bookings.FirstOrDefault(b =>
            b.BookingID == bookingId && b.UserID == userId && b.Status == "Active");
        if (booking == null) return false;

        booking.Status = "Cancelled";
        booking.CancelReason = "UserCancelled";
        booking.ClosedAt = DateTime.Now;

        var book = _books.FirstOrDefault(b => b.BookID == booking.BookID);
        if (book != null)
        {
            book.AvailableCount++;
            if (book.Status == "Booked") book.Status = "Available";
        }
        return true;
    }

    public static void ExpireOldBookings()
    {
        var expired = _bookings.Where(b => b.IsExpired).ToList();
        foreach (var booking in expired)
        {
            booking.Status = "Expired";
            booking.CancelReason = "Expired";
            booking.ClosedAt = DateTime.Now;

            var book = _books.FirstOrDefault(b => b.BookID == booking.BookID);
            if (book != null)
            {
                book.AvailableCount++;
                if (book.Status == "Booked") book.Status = "Available";
            }
        }
    }

    public static int GetUserActiveBookingsCount(int userId) =>
        _bookings.Count(b => b.UserID == userId && b.Status == "Active");

    public static List<BookReservation> GetUserBookings(int userId) =>
        _bookings.Where(b => b.UserID == userId).ToList();

    public static List<BookReservation> GetActiveBookingsByUser(int userId) =>
        _bookings.Where(b => b.UserID == userId && b.Status == "Active").ToList();
}

// =========================================================
// ПЕРЕЧИСЛЕНИЯ
// =========================================================

public enum BookingResult
{
    Success,
    NotFound,
    NotAvailable,
    LimitExceeded
}

public enum WriteOffResult
{
    Success,
    PartialSuccess,
    NotFound,
    HasActiveBookings
}

public enum DeleteResult
{
    Success,
    NotFound,
    HasActiveBookings,
    NotWrittenOff
}
