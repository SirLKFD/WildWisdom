using System;

namespace ASI.Basecode.Data.Models
{
    public partial class PasswordResetToken
    {
        public int TokenId { get; set; }
        public string Email { get; set; }
        public int Role { get; set; } // 0 = User, 1 = Admin
        public string Token { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}

