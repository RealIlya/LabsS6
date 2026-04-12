-- ============================================================
-- BookCatalog — полный скрипт развёртывания базы данных
-- Выполнять целиком в SSMS или SQL Server Object Explorer
-- ============================================================

-- 1. Создание базы данных
CREATE DATABASE BookCatalogDB;
GO
USE BookCatalogDB;
GO

-- 2. Таблица пользователей
CREATE TABLE Users (
    UserID       INT PRIMARY KEY IDENTITY(1,1),
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role         NVARCHAR(20) DEFAULT 'User'
                 CHECK (Role IN ('User', 'Admin')),
    MaxBookings  INT DEFAULT 3,
    CreatedAt    DATETIME DEFAULT GETDATE(),
    IsActive     BIT DEFAULT 1
);
GO

-- 3. Таблица книг
CREATE TABLE Books (
    BookID         INT PRIMARY KEY IDENTITY(1,1),
    Title          NVARCHAR(200) NOT NULL,
    Author         NVARCHAR(150) NOT NULL,
    ISBN           NVARCHAR(20) UNIQUE,
    Publisher      NVARCHAR(100),
    Year           INT CHECK (Year BETWEEN 1000 AND 2026),
    Pages          INT,
    Genre          NVARCHAR(50),
    TotalCount     INT DEFAULT 1,
    AvailableCount INT DEFAULT 1,
    CoverImageURL  NVARCHAR(255),
    IsArchived     BIT DEFAULT 0,
    CreatedAt      DATETIME DEFAULT GETDATE()
);
GO

-- 4. Таблица бронирований
CREATE TABLE BookReservations (
    ReservationID INT PRIMARY KEY IDENTITY(1,1),
    UserID        INT FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    BookID        INT FOREIGN KEY REFERENCES Books(BookID),
    ReservedAt    DATETIME DEFAULT GETDATE(),
    ExpiresAt     DATETIME,
    Status        NVARCHAR(20) DEFAULT 'Active'
                  CHECK (Status IN ('Active','Completed','Cancelled','Expired','Queued')),
    CONSTRAINT CHK_Expires CHECK (ExpiresAt > ReservedAt)
);
GO

-- 5. Таблица записей о списании
CREATE TABLE WriteOffRecords (
    RecordID           INT PRIMARY KEY IDENTITY(1,1),
    BookID             INT FOREIGN KEY REFERENCES Books(BookID),
    AdminID            INT FOREIGN KEY REFERENCES Users(UserID),
    Reason             NVARCHAR(200) NOT NULL,
    WriteOffDate       DATETIME DEFAULT GETDATE(),
    SnapshotData       NVARCHAR(MAX),
    CanBeRestoredUntil DATETIME,
    IsRestored         BIT DEFAULT 0
);
GO

-- 6. Индексы
CREATE INDEX IX_Books_Search      ON Books(Title, Author, Genre, Year);
CREATE INDEX IX_Reservations_User ON BookReservations(UserID, Status);
CREATE INDEX IX_Reservations_Book ON BookReservations(BookID, Status);
GO

-- ============================================================
-- ХРАНИМЫЕ ПРОЦЕДУРЫ
-- ============================================================

-- sp_RegisterUser
CREATE PROCEDURE sp_RegisterUser
    @FullName     NVARCHAR(100),
    @Email        NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @UserID       INT OUTPUT,
    @ErrorMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        SET @ErrorMessage = N'Пользователь с таким email уже существует';
        SET @UserID = -1;
        RETURN;
    END
    INSERT INTO Users (FullName, Email, PasswordHash)
    VALUES (@FullName, @Email, @PasswordHash);
    SET @UserID = SCOPE_IDENTITY();
    SET @ErrorMessage = NULL;
END
GO

-- sp_AuthenticateUser
CREATE PROCEDURE sp_AuthenticateUser
    @Email        NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @UserID       INT OUTPUT,
    @FullName     NVARCHAR(100) OUTPUT,
    @Role         NVARCHAR(20) OUTPUT,
    @Success      BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @UserID = UserID, @FullName = FullName, @Role = Role
    FROM Users
    WHERE Email = @Email AND PasswordHash = @PasswordHash AND IsActive = 1;
    SET @Success = CASE WHEN @UserID IS NOT NULL THEN 1 ELSE 0 END;
END
GO

-- sp_SearchBooks
CREATE PROCEDURE sp_SearchBooks
    @Title    NVARCHAR(200) = NULL,
    @Author   NVARCHAR(150) = NULL,
    @Genre    NVARCHAR(50)  = NULL,
    @YearFrom INT = NULL,
    @YearTo   INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT BookID, Title, Author, ISBN, Publisher, Year, Genre,
           AvailableCount, TotalCount, CoverImageURL
    FROM Books
    WHERE IsArchived = 0
      AND (@Title    IS NULL OR Title  LIKE '%' + @Title  + '%')
      AND (@Author   IS NULL OR Author LIKE '%' + @Author + '%')
      AND (@Genre    IS NULL OR Genre  = @Genre)
      AND (@YearFrom IS NULL OR Year  >= @YearFrom)
      AND (@YearTo   IS NULL OR Year  <= @YearTo)
    ORDER BY Title;
END
GO

-- sp_CreateBooking
CREATE PROCEDURE sp_CreateBooking
    @UserID        INT,
    @BookID        INT,
    @BookingDays   INT = 3,
    @ReservationID INT OUTPUT,
    @ErrorMessage  NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Books
                   WHERE BookID = @BookID AND AvailableCount > 0 AND IsArchived = 0)
    BEGIN
        SET @ErrorMessage = N'Книга недоступна для бронирования';
        SET @ReservationID = -1; RETURN;
    END
    DECLARE @ActiveBookings INT;
    SELECT @ActiveBookings = COUNT(*) FROM BookReservations
    WHERE UserID = @UserID AND Status = 'Active';
    DECLARE @MaxBookings INT;
    SELECT @MaxBookings = MaxBookings FROM Users WHERE UserID = @UserID;
    IF @ActiveBookings >= @MaxBookings
    BEGIN
        SET @ErrorMessage = N'Превышен лимит бронирований';
        SET @ReservationID = -1; RETURN;
    END
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO BookReservations (UserID, BookID, ExpiresAt)
        VALUES (@UserID, @BookID, DATEADD(DAY, @BookingDays, GETDATE()));
        SET @ReservationID = SCOPE_IDENTITY();
        UPDATE Books SET AvailableCount = AvailableCount - 1 WHERE BookID = @BookID;
        COMMIT;
        SET @ErrorMessage = NULL;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @ErrorMessage = ERROR_MESSAGE();
        SET @ReservationID = -1;
    END CATCH
END
GO

-- sp_AddBook
CREATE PROCEDURE sp_AddBook
    @Title         NVARCHAR(200),
    @Author        NVARCHAR(150),
    @ISBN          NVARCHAR(20),
    @Publisher     NVARCHAR(100) = NULL,
    @Year          INT,
    @Pages         INT = NULL,
    @Genre         NVARCHAR(50)  = NULL,
    @Count         INT = 1,
    @CoverImageURL NVARCHAR(255) = NULL,
    @BookID        INT OUTPUT,
    @IsDuplicate   BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @IsDuplicate = 0;
    IF @ISBN IS NOT NULL AND EXISTS (
        SELECT 1 FROM Books WHERE ISBN = @ISBN AND IsArchived = 0)
    BEGIN
        SET @IsDuplicate = 1;
        UPDATE Books
        SET TotalCount     = TotalCount     + @Count,
            AvailableCount = AvailableCount + @Count
        WHERE ISBN = @ISBN;
        SELECT @BookID = BookID FROM Books WHERE ISBN = @ISBN;
        RETURN;
    END
    INSERT INTO Books
        (Title, Author, ISBN, Publisher, Year, Pages, Genre,
         TotalCount, AvailableCount, CoverImageURL)
    VALUES
        (@Title, @Author, @ISBN, @Publisher, @Year, @Pages, @Genre,
         @Count, @Count, @CoverImageURL);
    SET @BookID = SCOPE_IDENTITY();
END
GO

-- sp_WriteOffBook
CREATE PROCEDURE sp_WriteOffBook
    @BookID            INT,
    @AdminID           INT,
    @Reason            NVARCHAR(200),
    @Count             INT = 1,
    @RecordID          INT OUTPUT,
    @HasActiveBookings BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM BookReservations
               WHERE BookID = @BookID AND Status = 'Active')
    BEGIN
        SET @HasActiveBookings = 1;
        SET @RecordID = -1; RETURN;
    END
    SET @HasActiveBookings = 0;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @Snapshot NVARCHAR(MAX);
        SELECT @Snapshot = (
            SELECT BookID, Title, Author, ISBN, Year, Genre, TotalCount, AvailableCount
            FROM Books WHERE BookID = @BookID
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );
        INSERT INTO WriteOffRecords
            (BookID, AdminID, Reason, SnapshotData, CanBeRestoredUntil)
        VALUES
            (@BookID, @AdminID, @Reason, @Snapshot, DATEADD(HOUR, 24, GETDATE()));
        SET @RecordID = SCOPE_IDENTITY();
        IF @Count >= (SELECT AvailableCount FROM Books WHERE BookID = @BookID)
            UPDATE Books
            SET IsArchived = 1, AvailableCount = 0, TotalCount = 0
            WHERE BookID = @BookID;
        ELSE
            UPDATE Books
            SET AvailableCount = AvailableCount - @Count,
                TotalCount     = TotalCount     - @Count
            WHERE BookID = @BookID;
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @RecordID = -1;
        THROW;
    END CATCH
END
GO

-- ============================================================
-- ТЕСТОВЫЕ ДАННЫЕ
-- ============================================================

-- Администратор (пароль: 123123)
INSERT INTO Users (FullName, Email, PasswordHash, Role, MaxBookings, IsActive)
VALUES (N'Администратор', N'ad@test.com',
        N'123123',
        N'Admin', 10, 1);
GO

-- Книги
INSERT INTO Books
    (Title, Author, ISBN, Publisher, Year, Pages, Genre, TotalCount, AvailableCount)
VALUES
(N'Мастер и Маргарита',
    N'Михаил Булгаков',         N'978-5-17-090000-1', N'АСТ',              1967, 480,  N'fiction',   3, 3),
(N'Преступление и наказание',
    N'Фёдор Достоевский',       N'978-5-17-090000-2', N'АСТ',              1866, 592,  N'fiction',   2, 2),
(N'Война и мир',
    N'Лев Толстой',             N'978-5-17-090000-3', N'Эксмо',            1869, 1274, N'fiction',   2, 2),
(N'1984',
    N'Джордж Оруэлл',           N'978-5-17-090000-4', N'АСТ',              1949, 320,  N'fiction',   3, 3),
(N'Краткая история времени',
    N'Стивен Хокинг',           N'978-5-17-090000-5', N'АСТ',              1988, 212,  N'science',   2, 2),
(N'Sapiens',
    N'Юваль Ной Харари',        N'978-5-17-090000-6', N'Синдбад',          2011, 512,  N'science',   2, 2),
(N'Чистый код',
    N'Роберт Мартин',           N'978-5-17-090000-7', N'Питер',            2008, 464,  N'education', 2, 2),
(N'Совершенный код',
    N'Стив Макконнелл',         N'978-5-17-090000-8', N'Русская Редакция', 2004, 896,  N'education', 1, 1),
(N'Гарри Поттер и философский камень',
    N'Джоан Роулинг',           N'978-5-17-090000-9', N'Росмэн',           1997, 432,  N'children',  3, 3),
(N'Маленький принц',
    N'Антуан де Сент-Экзюпери', N'978-5-17-090001-0', N'Эксмо',            1943, 96,   N'children',  2, 2);
GO

-- Проверка
SELECT 'Users'  AS [Table], COUNT(*) AS [Rows] FROM Users
UNION ALL
SELECT 'Books',  COUNT(*) FROM Books
UNION ALL
SELECT 'Stored procedures',
       COUNT(*) FROM sys.procedures
       WHERE name LIKE 'sp_%';
GO