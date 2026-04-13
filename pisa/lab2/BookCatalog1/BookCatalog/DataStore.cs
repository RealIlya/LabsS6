// DataStore.cs — корень проекта, глобальное пространство имён (без namespace)
// Все операции идут в SQL Server через DBHelper.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

// ── Используется в Booking.aspx.cs ───────────────────────────────────────────
public enum BookingResult
{
    Success,
    NotAvailable,
    LimitExceeded,
    NotFound
}

// ── Используется в WriteOff.aspx.cs ──────────────────────────────────────────
public enum WriteOffResult
{
    Success,
    PartialSuccess,
    HasActiveBookings,
    NotFound
}

public static class DataStore
{
    // =========================================================
    // ПОЛЬЗОВАТЕЛИ
    // =========================================================

    public static bool UserExists(string email)
    {
        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT 1 FROM Users WHERE Email = @Email",
            new SqlParameter("@Email", email));
        return dt.Rows.Count > 0;
    }

    public static bool RegisterUser(string fullName, string email, string password)
    {
        try
        {
            SqlParameter pUserID = new SqlParameter("@UserID", SqlDbType.Int)
            { Direction = ParameterDirection.Output };
            SqlParameter pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 255)
            { Direction = ParameterDirection.Output };

            DBHelper.ExecuteNonQuery("sp_RegisterUser",
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@Email", email),
                new SqlParameter("@PasswordHash", password),
                pUserID, pError);

            return pUserID.Value != DBNull.Value && (int)pUserID.Value > 0;
        }
        catch { return false; }
    }

    public static User AuthenticateUser(string email, string password)
    {
        try
        {
            SqlParameter pUserID = new SqlParameter("@UserID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            SqlParameter pFullName = new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
            SqlParameter pRole = new SqlParameter("@Role", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output };
            SqlParameter pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };

            DBHelper.ExecuteNonQuery("sp_AuthenticateUser",
                new SqlParameter("@Email", email),
                new SqlParameter("@PasswordHash", password),
                pUserID, pFullName, pRole, pSuccess);

            if (pSuccess.Value == DBNull.Value || !(bool)pSuccess.Value)
                return null;

            return GetUserByEmail(email);
        }
        catch { return null; }
    }

    public static User GetUserByEmail(string email)
    {
        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT UserID, FullName, Email, Role, IsActive, MaxBookings, CreatedAt " +
            "FROM Users WHERE Email = @Email",
            new SqlParameter("@Email", email));
        return dt.Rows.Count == 0 ? null : MapUser(dt.Rows[0]);
    }

    public static User GetUserById(int userID)
    {
        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT UserID, FullName, Email, Role, IsActive, MaxBookings, CreatedAt " +
            "FROM Users WHERE UserID = @UserID",
            new SqlParameter("@UserID", userID));
        return dt.Rows.Count == 0 ? null : MapUser(dt.Rows[0]);
    }

    // Алиас — на случай вызова с заглавной D
    public static User GetUserByID(int userID) => GetUserById(userID);

    // ManageUsers: GetAllUsers(currentAdmin.UserID) — без самого админа
    public static List<User> GetAllUsers(int excludeUserID = -1)
    {
        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT UserID, FullName, Email, Role, IsActive, MaxBookings, CreatedAt " +
            "FROM Users WHERE UserID <> @ExcludeUserID ORDER BY FullName",
            new SqlParameter("@ExcludeUserID", excludeUserID));

        var list = new List<User>();
        foreach (DataRow r in dt.Rows) list.Add(MapUser(r));
        return list;
    }

    public static void ToggleUserActive(int userID)
    {
        DBHelper.ExecuteQueryText(
            "UPDATE Users SET IsActive = CASE WHEN IsActive=1 THEN 0 ELSE 1 END WHERE UserID=@UserID",
            new SqlParameter("@UserID", userID));
    }

    public static void ToggleAdminRole(int userID)
    {
        DBHelper.ExecuteQueryText(
            "UPDATE Users SET Role = CASE WHEN Role='Admin' THEN 'User' ELSE 'Admin' END WHERE UserID=@UserID",
            new SqlParameter("@UserID", userID));
    }

    public static int GetUserActiveBookingsCount(int userID)
    {
        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT COUNT(*) AS Cnt FROM BookReservations WHERE UserID=@UserID AND Status='Active'",
            new SqlParameter("@UserID", userID));
        return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Cnt"]) : 0;
    }

    public static List<BookReservation> GetActiveBookingsByUser(int userID)
    {
        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT ReservationID, UserID, BookID, ReservedAt, ExpiresAt, Status " +
            "FROM BookReservations WHERE UserID=@UserID AND Status='Active' ORDER BY ReservedAt DESC",
            new SqlParameter("@UserID", userID));

        var list = new List<BookReservation>();
        foreach (DataRow r in dt.Rows) list.Add(MapReservation(r));
        return list;
    }

    // ForgotPassword.aspx.cs
    public static void ResetPassword(int userID, string newPassword)
    {
        DBHelper.ExecuteQueryText(
            "UPDATE Users SET PasswordHash=@Hash WHERE UserID=@UserID",
            new SqlParameter("@Hash", newPassword),
            new SqlParameter("@UserID", userID));
    }

    private static User MapUser(DataRow r)
    {
        return new User
        {
            UserID = (int)r["UserID"],
            FullName = r["FullName"].ToString(),
            Email = r["Email"].ToString(),
            Role = r["Role"].ToString(),
            IsActive = r["IsActive"] != DBNull.Value && (bool)r["IsActive"],
            MaxBookings = r["MaxBookings"] != DBNull.Value ? (int)r["MaxBookings"] : 3,
            CreatedAt = r["CreatedAt"] != DBNull.Value ? (DateTime)r["CreatedAt"] : DateTime.Now
        };
    }

    // =========================================================
    // КНИГИ
    // =========================================================

    public static List<Book> SearchBooks(string title = null, string author = null,
                                          string genre = null, int? year = null)
    {
        DataTable dt = DBHelper.ExecuteQuery("sp_SearchBooks",
            new SqlParameter("@Title", string.IsNullOrEmpty(title) ? (object)DBNull.Value : title),
            new SqlParameter("@Author", string.IsNullOrEmpty(author) ? (object)DBNull.Value : author),
            new SqlParameter("@Genre", string.IsNullOrEmpty(genre) ? (object)DBNull.Value : genre),
            new SqlParameter("@YearFrom", year.HasValue ? (object)year.Value : DBNull.Value),
            new SqlParameter("@YearTo", year.HasValue ? (object)year.Value : DBNull.Value));

        var list = new List<Book>();
        foreach (DataRow r in dt.Rows) list.Add(MapBook(r));
        return list;
    }

    public static Book GetBookById(int bookID)
    {
        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT BookID, Title, Author, ISBN, Publisher, Year, Pages, Genre, " +
            "       TotalCount, AvailableCount, IsArchived, CreatedAt " +
            "FROM Books WHERE BookID=@BookID",
            new SqlParameter("@BookID", bookID));
        return dt.Rows.Count == 0 ? null : MapBook(dt.Rows[0]);
    }

    public static Book GetBookByID(int bookID) => GetBookById(bookID);

    public static void AddBook(Book book)
    {
        SqlParameter pBookID = new SqlParameter("@BookID", SqlDbType.Int) { Direction = ParameterDirection.Output };
        SqlParameter pIsDuplicate = new SqlParameter("@IsDuplicate", SqlDbType.Bit) { Direction = ParameterDirection.Output };

        DBHelper.ExecuteNonQuery("sp_AddBook",
            new SqlParameter("@Title", book.Title),
            new SqlParameter("@Author", book.Author),
            new SqlParameter("@ISBN", string.IsNullOrEmpty(book.ISBN) ? (object)DBNull.Value : book.ISBN),
            new SqlParameter("@Publisher", string.IsNullOrEmpty(book.Publisher) ? (object)DBNull.Value : book.Publisher),
            new SqlParameter("@Year", book.Year),
            new SqlParameter("@Pages", book.PageCount > 0 ? (object)book.PageCount : DBNull.Value),
            new SqlParameter("@Genre", string.IsNullOrEmpty(book.Genre) ? (object)DBNull.Value : book.Genre),
            new SqlParameter("@Count", book.AvailableCount > 0 ? book.AvailableCount : 1),
            new SqlParameter("@CoverImageURL", DBNull.Value),
            pBookID, pIsDuplicate);

        if (pBookID.Value != DBNull.Value)
            book.BookID = (int)pBookID.Value;
    }

    public static List<Book> FindDuplicatesByISBN(string isbn)
    {
        if (string.IsNullOrEmpty(isbn)) return new List<Book>();

        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT BookID, Title, Author, ISBN, Publisher, Year, Pages, Genre, " +
            "       TotalCount, AvailableCount, IsArchived, CreatedAt " +
            "FROM Books WHERE ISBN=@ISBN AND IsArchived=0",
            new SqlParameter("@ISBN", isbn));

        var list = new List<Book>();
        foreach (DataRow r in dt.Rows) list.Add(MapBook(r));
        return list;
    }

    private static Book MapBook(DataRow r)
    {
        bool isArchived = r.Table.Columns.Contains("IsArchived")
                          && r["IsArchived"] != DBNull.Value
                          && (bool)r["IsArchived"];
        int available = r["AvailableCount"] != DBNull.Value ? (int)r["AvailableCount"] : 0;

        return new Book
        {
            BookID = (int)r["BookID"],
            Title = r["Title"].ToString(),
            Author = r["Author"].ToString(),
            ISBN = r["ISBN"] != DBNull.Value ? r["ISBN"].ToString() : "",
            Publisher = r["Publisher"] != DBNull.Value ? r["Publisher"].ToString() : "",
            Year = r["Year"] != DBNull.Value ? (int)r["Year"] : 0,
            PageCount = r.Table.Columns.Contains("Pages") && r["Pages"] != DBNull.Value
                             ? (int)r["Pages"] : 0,
            Genre = r["Genre"] != DBNull.Value ? r["Genre"].ToString() : "",
            TotalCount = r["TotalCount"] != DBNull.Value ? (int)r["TotalCount"] : 0,
            AvailableCount = available,
            Status = isArchived ? "WrittenOff" : (available > 0 ? "Available" : "Booked"),
            AddedAt = r.Table.Columns.Contains("CreatedAt") && r["CreatedAt"] != DBNull.Value
                             ? (DateTime)r["CreatedAt"] : DateTime.Now
        };
    }

    // =========================================================
    // БРОНИРОВАНИЯ
    // =========================================================

    // Booking.aspx.cs: возвращает BookingResult
    public static BookingResult CreateBooking(int userID, int bookID, int days = 3)
    {
        Book book = GetBookById(bookID);
        if (book == null || book.Status == "WrittenOff") return BookingResult.NotFound;
        if (!book.IsAvailable) return BookingResult.NotAvailable;

        User user = GetUserById(userID);
        if (user != null && GetUserActiveBookingsCount(userID) >= user.MaxBookings)
            return BookingResult.LimitExceeded;

        try
        {
            SqlParameter pReservationID = new SqlParameter("@ReservationID", SqlDbType.Int)
            { Direction = ParameterDirection.Output };
            SqlParameter pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 255)
            { Direction = ParameterDirection.Output };

            DBHelper.ExecuteNonQuery("sp_CreateBooking",
                new SqlParameter("@UserID", userID),
                new SqlParameter("@BookID", bookID),
                new SqlParameter("@BookingDays", days),
                pReservationID, pError);

            if (pReservationID.Value != DBNull.Value && (int)pReservationID.Value > 0)
                return BookingResult.Success;

            string err = pError.Value != DBNull.Value ? pError.Value.ToString() : "";
            if (err.Contains("лимит")) return BookingResult.LimitExceeded;
            if (err.Contains("недоступна")) return BookingResult.NotAvailable;
            return BookingResult.NotFound;
        }
        catch { return BookingResult.NotFound; }
    }

    public static List<BookReservation> GetUserBookings(int userID)
    {
        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT ReservationID, UserID, BookID, ReservedAt, ExpiresAt, Status " +
            "FROM BookReservations WHERE UserID=@UserID ORDER BY ReservedAt DESC",
            new SqlParameter("@UserID", userID));

        var list = new List<BookReservation>();
        foreach (DataRow r in dt.Rows) list.Add(MapReservation(r));
        return list;
    }

    // UserProfile.aspx.cs: отмена брони пользователем
    public static bool CancelBooking(int reservationID, int userID)
    {
        try
        {
            DBHelper.ExecuteQueryText(
                "DECLARE @BID INT = (SELECT BookID FROM BookReservations " +
                "  WHERE ReservationID=@ReservationID AND UserID=@UserID AND Status='Active'); " +
                "UPDATE BookReservations SET Status='Cancelled' " +
                "  WHERE ReservationID=@ReservationID AND UserID=@UserID AND Status='Active'; " +
                "IF @@ROWCOUNT > 0 " +
                "  UPDATE Books SET AvailableCount=AvailableCount+1 WHERE BookID=@BID;",
                new SqlParameter("@ReservationID", reservationID),
                new SqlParameter("@UserID", userID));
            return true;
        }
        catch { return false; }
    }

    // WriteOff.aspx.cs: отмена всех активных броней книги перед списанием
    public static void CancelBookingsForBook(int bookID)
    {
        DBHelper.ExecuteQueryText(
            "UPDATE Books SET AvailableCount = AvailableCount + " +
            "  (SELECT COUNT(*) FROM BookReservations WHERE BookID=@BookID AND Status='Active') " +
            "WHERE BookID=@BookID; " +
            "UPDATE BookReservations SET Status='Cancelled' WHERE BookID=@BookID AND Status='Active';",
            new SqlParameter("@BookID", bookID));
    }

    public static bool AddToQueue(int userID, int bookID)
    {
        DataTable check = DBHelper.ExecuteQueryText(
            "SELECT 1 FROM BookReservations WHERE UserID=@UserID AND BookID=@BookID AND Status='Queued'",
            new SqlParameter("@UserID", userID),
            new SqlParameter("@BookID", bookID));
        if (check.Rows.Count > 0) return false;

        DBHelper.ExecuteQueryText(
            "INSERT INTO BookReservations (UserID, BookID, ExpiresAt, Status) " +
            "VALUES (@UserID, @BookID, DATEADD(DAY,30,GETDATE()), 'Queued')",
            new SqlParameter("@UserID", userID),
            new SqlParameter("@BookID", bookID));
        return true;
    }

    public static void ExpireOldBookings()
    {
        try
        {
            DBHelper.ExecuteQueryText(
                "UPDATE Books SET AvailableCount = AvailableCount + " +
                "  (SELECT COUNT(*) FROM BookReservations r2 " +
                "   WHERE r2.BookID=Books.BookID AND r2.Status='Active' AND r2.ExpiresAt<GETDATE()); " +
                "UPDATE BookReservations SET Status='Expired' " +
                "WHERE Status='Active' AND ExpiresAt<GETDATE();");
        }
        catch { }
    }

    private static BookReservation MapReservation(DataRow r)
    {
        return new BookReservation
        {
            BookingID = (int)r["ReservationID"],
            UserID = (int)r["UserID"],
            BookID = (int)r["BookID"],
            BookingDate = r["ReservedAt"] != DBNull.Value ? (DateTime)r["ReservedAt"] : DateTime.Now,
            ExpiryDate = r["ExpiresAt"] != DBNull.Value ? (DateTime)r["ExpiresAt"] : DateTime.Now,
            Status = r["Status"].ToString()
        };
    }

    // =========================================================
    // СПИСАНИЯ — WriteOff.aspx.cs
    // Сигнатура: WriteOffBook(bookId, reason, count, out int writtenOff)
    // =========================================================

    public static WriteOffResult WriteOffBook(int bookID, string reason, int count, out int writtenOff, int adminID = 0)
    {
        writtenOff = 0;

        Book book = GetBookById(bookID);
        if (book == null) return WriteOffResult.NotFound;

        // Если хотим списать больше чем свободно — нужно отменять активные брони
        if (count > book.AvailableCount)
        {
            DataTable active = DBHelper.ExecuteQueryText(
                "SELECT COUNT(*) AS Cnt FROM BookReservations WHERE BookID=@BookID AND Status='Active'",
                new SqlParameter("@BookID", bookID));
            int activeCount = active.Rows.Count > 0 ? Convert.ToInt32(active.Rows[0]["Cnt"]) : 0;

            if (activeCount > 0) return WriteOffResult.HasActiveBookings;
        }

        // Списываем не больше чем TotalCount
        int actualCount = Math.Min(count, book.TotalCount);
        writtenOff = actualCount;
        bool fullWriteOff = (book.TotalCount - actualCount) <= 0;

        try
        {
            string title = book.Title.Replace("\"", "\\\"");
            string snapshot = $"{{\"BookID\":{book.BookID},\"Title\":\"{title}\"," +
                              $"\"TotalCount\":{book.TotalCount},\"AvailableCount\":{book.AvailableCount}}}";

            if (fullWriteOff)
            {
                DBHelper.ExecuteQueryText(
                    "INSERT INTO WriteOffRecords (BookID, AdminID, Reason, SnapshotData, CanBeRestoredUntil) " +
                    "VALUES (@BookID, @AdminID, @Reason, @Snapshot, DATEADD(HOUR,24,GETDATE())); " +
                    "UPDATE Books SET IsArchived=1, AvailableCount=0, TotalCount=0 WHERE BookID=@BookID;",
                    new SqlParameter("@BookID", bookID),
                    new SqlParameter("@AdminID", adminID),
                    new SqlParameter("@Reason", reason),
                    new SqlParameter("@Snapshot", snapshot));

                return WriteOffResult.Success;
            }
            else
            {
                int reduceAvailable = Math.Min(actualCount, book.AvailableCount);

                DBHelper.ExecuteQueryText(
                    "INSERT INTO WriteOffRecords (BookID, AdminID, Reason, SnapshotData, CanBeRestoredUntil) " +
                    "VALUES (@BookID, @AdminID, @Reason, @Snapshot, DATEADD(HOUR,24,GETDATE())); " +
                    "UPDATE Books " +
                    "SET TotalCount     = TotalCount - @ActualCount, " +
                    "    AvailableCount = AvailableCount - @ReduceAvailable " +
                    "WHERE BookID=@BookID;",
                    new SqlParameter("@BookID", bookID),
                    new SqlParameter("@AdminID", adminID),
                    new SqlParameter("@Reason", reason),
                    new SqlParameter("@Snapshot", snapshot),
                    new SqlParameter("@ActualCount", actualCount),
                    new SqlParameter("@ReduceAvailable", reduceAvailable));

                return WriteOffResult.PartialSuccess;
            }
        }
        catch { writtenOff = 0; return WriteOffResult.NotFound; }
    }

    public static List<WriteOffRecord> GetWriteOffRecords()
    {
        DataTable dt = DBHelper.ExecuteQueryText(
            "SELECT w.RecordID, w.BookID, b.Title, b.Author, b.ISBN, w.Reason, w.WriteOffDate " +
            "FROM WriteOffRecords w JOIN Books b ON w.BookID=b.BookID " +
            "ORDER BY w.WriteOffDate DESC");

        var list = new List<WriteOffRecord>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(new WriteOffRecord
            {
                RecordID = (int)r["RecordID"],
                BookID = (int)r["BookID"],
                BookTitle = r["Title"].ToString(),
                BookAuthor = r["Author"].ToString(),
                BookISBN = r["ISBN"] != DBNull.Value ? r["ISBN"].ToString() : "",
                Reason = r["Reason"].ToString(),
                WriteOffDate = r["WriteOffDate"] != DBNull.Value ? (DateTime)r["WriteOffDate"] : DateTime.Now
            });
        }
        return list;
    }

    // WriteOff.aspx.cs использует RestoreFromRecord(recordId)
    public static bool RestoreFromRecord(int recordID)
    {
        try
        {
            DBHelper.ExecuteQueryText(
                "UPDATE WriteOffRecords SET IsRestored=1 " +
                "WHERE RecordID=@RecordID AND CanBeRestoredUntil>GETDATE() AND IsRestored=0; " +
                "IF @@ROWCOUNT > 0 BEGIN " +
                "  UPDATE Books SET IsArchived=0, " +
                "    TotalCount=CAST(JSON_VALUE((SELECT SnapshotData FROM WriteOffRecords WHERE RecordID=@RecordID),'$.TotalCount') AS INT), " +
                "    AvailableCount=CAST(JSON_VALUE((SELECT SnapshotData FROM WriteOffRecords WHERE RecordID=@RecordID),'$.AvailableCount') AS INT) " +
                "  WHERE BookID=(SELECT BookID FROM WriteOffRecords WHERE RecordID=@RecordID); " +
                "END",
                new SqlParameter("@RecordID", recordID));
            return true;
        }
        catch { return false; }
    }

    // Алиас — старое имя оставляем для совместимости
    public static bool RestoreBook(int recordID, int adminID = 0) => RestoreFromRecord(recordID);
}


