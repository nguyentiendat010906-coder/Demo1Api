using System.Text.Json.Serialization;

namespace Demo1Api.Models
{
    public class Table
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Status { get; set; } = "empty";

        // 🔑 FK nhóm bàn
        public int TableGroupId { get; set; }

        // 🔗 Invoice đang phục vụ (QUAN TRỌNG)
        public int? CurrentInvoiceId { get; set; }   // 👈 THÊM DÒNG NÀY

        // tránh vòng lặp JSON
        [JsonIgnore]
        public TableGroup? TableGroup { get; set; }
    }
}
