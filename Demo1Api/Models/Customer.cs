using System.Text.Json.Serialization;

namespace Demo1Api.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";

        public string? TaxCode { get; set; }
        public string? IdCard { get; set; }
        public string? Email { get; set; }

        public string Address { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public List<Invoice> Invoices { get; set; } = new();
    }
}
