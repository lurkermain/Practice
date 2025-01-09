using Microsoft.EntityFrameworkCore;

namespace Practice.Models
{
    public class Product
    {
        public int Id { get; set; } 
        public string ?Name { get; set; }
        public string ?Description { get; set; }

        public string ?ModelType { get; set; }

        public byte[] ?Image { get; set; }

    }
}
