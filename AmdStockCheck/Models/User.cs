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
    public sealed class User
    {
        [Key]
        public ulong Id { get; set; }
        [ForeignKey("Id")]
        public ulong ProductId { get; set; }
        public ulong UserId { get; set; }
    }
}
