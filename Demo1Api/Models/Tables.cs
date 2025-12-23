namespace Demo1Api.Models
{
    public class Table
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int Capacity { get; set; }

        public string Status { get; set; } = "empty";

        // 🔑 FK
        public int TableGroupId { get; set; }

        // ✅ nullable để khỏi warning
        public TableGroup? TableGroup { get; set; }
    }
}
