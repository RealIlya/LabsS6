using System;
using System.Security.Cryptography;
using System.Text;

public class User
{
    public int UserID { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }

    // Хранится только хэш пароля, не сам пароль
    public string PasswordHash { get; set; }

    // "User" (Читатель) или "Admin" (Библиотекарь)
    public string Role { get; set; }

    public DateTime CreatedAt { get; set; }

    // Аккаунт не заблокирован администратором
    public bool IsActive { get; set; }

    // Сценарий 1, п.14: email подтверждён — открывает полный функционал
    public bool IsEmailConfirmed { get; set; }

    // Сценарий 4, шаг 6: максимально допустимое количество активных броней
    public int MaxBookings { get; set; } = 3;

    // Вычисляемые свойства
    public bool IsAdmin => Role == "Admin";

    // Пользователь может пользоваться системой только если аккаунт активен и email подтверждён
    public bool CanUseSystem => IsActive;

    // Хэширование пароля (SHA-256)
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    // Проверка пароля при авторизации (сценарий 2)
    public bool VerifyPassword(string password)
    {
        return PasswordHash == HashPassword(password);
    }
}


