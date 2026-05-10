using System;
using System.Collections.Generic;
using System.Linq;

namespace BookCatalogWinForms
{
    /// <summary>
    /// Результат операции списания
    /// </summary>
    public enum WriteOffResult
    {
        Success,
        PartialSuccess,
        HasActiveBookings,
        NotFound
    }

    public static class DataStore
    {
        private static List<Book> _books = new List<Book>();
        private static List<User> _users = new List<User>();
        private static List<WriteOffRecord> _writeOffRecords = new List<WriteOffRecord>();
        private static List<Booking> _bookings = new List<Booking>();
        private static int _nextBookId = 1;
        private static int _nextUserId = 1;
        private static int _nextRecordId = 1;
        private static int _nextBookingId = 1;

        // Текущий залогиненный пользователь
        public static User CurrentUser { get; private set; }

        static DataStore()
        {
            // Тестовые пользователи
            AddUser("Гость", "guest@test.com", "guest", UserRole.Guest);
            AddUser("Иван Иванов", "user@test.com", "user123", UserRole.User);
            AddUser("Админ", "admin@test.com", "admin123", UserRole.Admin);

            // Тестовые книги
            AddBook(new Book { Title = "Война и мир", Author = "Лев Толстой", ISBN = "978-5-17-090521-7", Year = 1869, Genre = "Художественная", PageCount = 1225, TotalCount = 5, AvailableCount = 3 });
            AddBook(new Book { Title = "Преступление и наказание", Author = "Фёдор Достоевский", ISBN = "978-5-17-080521-8", Year = 1866, Genre = "Художественная", PageCount = 672, TotalCount = 3, AvailableCount = 2 });
            AddBook(new Book { Title = "Мастер и Маргарита", Author = "Михаил Булгаков", ISBN = "978-5-17-070521-9", Year = 1967, Genre = "Художественная", PageCount = 480, TotalCount = 4, AvailableCount = 4 });
            AddBook(new Book { Title = "Краткая история времени", Author = "Стивен Хокинг", ISBN = "978-5-17-060521-0", Year = 1988, Genre = "Научная", PageCount = 256, TotalCount = 2, AvailableCount = 1 });
            AddBook(new Book { Title = "Алгебра 9 класс", Author = "Макарычев", ISBN = "978-5-17-050521-1", Year = 2020, Genre = "Учебная", PageCount = 320, TotalCount = 10, AvailableCount = 8 });
            AddBook(new Book { Title = "Колобок", Author = "Русская народная сказка", ISBN = "978-5-17-040521-2", Year = 1950, Genre = "Детская", PageCount = 12, TotalCount = 6, AvailableCount = 6 });
        }

        // === Пользователи ===

        private static void AddUser(string name, string email, string password, UserRole role)
        {
            _users.Add(new User
            {
                UserID = _nextUserId++,
                Name = name,
                Email = email,
                Password = password,
                Role = role
            });
        }

        public static User Login(string email, string password)
        {
            var user = _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            CurrentUser = user;
            return user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static User Register(string name, string email, string password)
        {
            if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                return null; // Email уже занят

            var user = new User
            {
                UserID = _nextUserId++,
                Name = name,
                Email = email,
                Password = password,
                Role = UserRole.User
            };
            _users.Add(user);
            CurrentUser = user;
            return user;
        }

        public static bool EmailExists(string email)
        {
            return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        // === Книги ===

        public static void AddBook(Book book)
        {
            book.BookID = _nextBookId++;
            book.Status = "Available";
            book.AddedAt = DateTime.Now;
            _books.Add(book);
        }

        public static List<Book> GetAllBooks()
        {
            return _books.Where(b => b.Status != "WrittenOff").ToList();
        }

        public static List<Book> SearchBooks(string title, string author, string genre, int? yearFrom, int? yearTo)
        {
            var query = _books.Where(b => b.Status != "WrittenOff");

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(b => b.Title.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(author))
                query = query.Where(b => b.Author.IndexOf(author, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(genre))
                query = query.Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));
            if (yearFrom.HasValue)
                query = query.Where(b => b.Year >= yearFrom.Value);
            if (yearTo.HasValue)
                query = query.Where(b => b.Year <= yearTo.Value);

            return query.ToList();
        }

        public static Book GetBookById(int id)
        {
            return _books.FirstOrDefault(b => b.BookID == id);
        }

        public static bool BookExists(string isbn)
        {
            return _books.Any(b => b.ISBN == isbn);
        }

        public static bool ReserveBook(int bookId)
        {
            if (CurrentUser == null || !CurrentUser.CanReserve) return false;
            var book = GetBookById(bookId);
            if (book == null || !book.IsAvailable) return false;

            book.AvailableCount--;

            _bookings.Add(new Booking
            {
                BookingID = _nextBookingId++,
                BookID = bookId,
                UserID = CurrentUser.UserID,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                BookISBN = book.ISBN,
                BookingDate = DateTime.Now
            });

            return true;
        }

        // === Бронирования ===

        public static List<Booking> GetUserBookings(int userId)
        {
            return _bookings.Where(b => b.UserID == userId).ToList();
        }

        /// <summary>
        /// Отменить бронирование — возвращает экземпляр книги в доступные
        /// </summary>
        public static bool CancelBooking(int bookingId)
        {
            var booking = _bookings.FirstOrDefault(b => b.BookingID == bookingId);
            if (booking == null) return false;

            var book = GetBookById(booking.BookID);
            if (book != null)
            {
                book.AvailableCount++;
                if (book.Status == "WrittenOff" && book.TotalCount > 0)
                    book.Status = "Available";
            }

            _bookings.Remove(booking);
            return true;
        }

        public static int GetBookingCountForBook(int bookId)
        {
            return _bookings.Count(b => b.BookID == bookId);
        }

        // === Списание ===

        public static WriteOffResult WriteOffBook(int bookId, string reason, int count, out int writtenOff)
        {
            writtenOff = 0;
            var book = GetBookById(bookId);
            if (book == null) return WriteOffResult.NotFound;

            // Сколько экземпляров забронировано
            int booked = book.TotalCount - book.AvailableCount;

            // Нельзя списать больше, чем всего экземпляров
            if (count > book.TotalCount)
                return WriteOffResult.NotFound;

            // Если есть брони и пытаемся списать больше, чем доступно — нужна отмена броней
            if (booked > 0 && count > book.AvailableCount)
                return WriteOffResult.HasActiveBookings;

            // Списываем
            int actualCount = Math.Min(count, book.TotalCount);
            book.TotalCount -= actualCount;
            book.AvailableCount = Math.Max(0, book.AvailableCount - actualCount);

            // Если все экземпляры списаны — помечаем книгу
            bool fullWriteOff = book.TotalCount <= 0;
            if (fullWriteOff)
                book.Status = "WrittenOff";

            // Создаём запись в архиве
            _writeOffRecords.Add(new WriteOffRecord
            {
                RecordID = _nextRecordId++,
                BookID = bookId,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                BookISBN = book.ISBN,
                Count = actualCount,
                Reason = reason,
                WriteOffDate = DateTime.Now
            });

            writtenOff = actualCount;
            return fullWriteOff ? WriteOffResult.Success : WriteOffResult.PartialSuccess;
        }

        /// <summary>
        /// Отменяет все брони для книги (возвращает AvailableCount = TotalCount)
        /// </summary>
        public static void CancelBookingsForBook(int bookId)
        {
            var book = GetBookById(bookId);
            if (book != null)
                book.AvailableCount = book.TotalCount;
        }

        public static List<WriteOffRecord> GetWriteOffRecords()
        {
            // Возвращаем в обратном порядке (новые сверху)
            var list = new List<WriteOffRecord>(_writeOffRecords);
            list.Reverse();
            return list;
        }

        /// <summary>
        /// Восстановление книги из архива (только если не прошло 24 часа)
        /// </summary>
        public static bool RestoreFromRecord(int recordId)
        {
            var record = _writeOffRecords.FirstOrDefault(r => r.RecordID == recordId);
            if (record == null || !record.CanBeRestored)
                return false;

            var book = _books.FirstOrDefault(b => b.BookID == record.BookID);
            if (book == null)
                return false;

            // Восстанавливаем экземпляры
            book.TotalCount += record.Count;
            book.AvailableCount += record.Count;
            book.Status = "Available";

            // Удаляем запись из архива
            _writeOffRecords.Remove(record);
            return true;
        }
    }
}
