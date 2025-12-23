namespace Demo1Api.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal Price { get; set; }

        public int? Stock { get; set; }      // cho phép NULL

        public string Category { get; set; } = null!;

        public string UnitType { get; set; } = null!;
    }
}
