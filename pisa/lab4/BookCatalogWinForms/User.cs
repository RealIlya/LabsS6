using System;

namespace BookCatalogWinForms
{
    public enum UserRole
    {
        Guest,
        User,
        Admin
    }

    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }

        public bool CanSearch => true;
        public bool CanReserve => Role == UserRole.User || Role == UserRole.Admin;
        public bool CanAddBook => Role == UserRole.Admin;
        public bool CanDeleteBook => Role == UserRole.Admin;

        public override string ToString()
        {
            return $"{Name} ({Role})";
        }
    }
}
