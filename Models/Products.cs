using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace hushazvillany_backend.Models
{
    public class Products
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Price { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public string? ImagePath { get; set; }
        public int Discount { get; set; } = 0;
        public string? Category { get; set; }

    }
}
