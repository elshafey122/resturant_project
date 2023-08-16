namespace firstproject.api.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int categoryId { get; set; }
        public Category category { get; set; }
    }
}
