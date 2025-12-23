using System.Collections.Generic;

namespace Demo1Api.Models
{
    public class TableGroup
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        // ✅ BỔ SUNG
        public string? Description { get; set; }

        public ICollection<Table> Tables { get; set; } = new List<Table>();
    }
}
