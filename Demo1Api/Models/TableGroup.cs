using System.Text.Json.Serialization;

namespace Demo1Api.Models
{
    public class TableGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // 🔑 NAVIGATION PROPERTY (BẮT BUỘC)
        [JsonIgnore]
        public List<Table> Tables { get; set; } = new();
    }
}
