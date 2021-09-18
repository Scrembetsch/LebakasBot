using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmdStockCheck.Models
{
    [Table("Users")]
    public sealed class RegisteredUser : IEquatable<RegisteredUser>, IEquatable<ulong>
    {
        [Key]
        public ulong UserId { get; set; }

        public bool Equals(RegisteredUser user)
        {
            return UserId == user.UserId;
        }

        public bool Equals(ulong userId)
        {
            return UserId == userId;
        }
    }
}
