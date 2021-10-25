using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmdStockCheck.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public ulong Id { get; set; }
        public string ProductId { get; set; }
        public string Alias { get; set; }
        public string CheckUrl  { get; set; }
        public List<User> Users  { get; set; }
    }
}
