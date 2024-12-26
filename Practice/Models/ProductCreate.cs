namespace Practice.Models
{
    public class ProductCreate
    {
        public int Id { get; set; }
        public string ?Name { get; set; }
        public string ?Description { get; set; }
        public string ?ModelType { get; set; }
        public IFormFile ?Image { get; set; }
    }
}
