namespace Demo1Api.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        // ===== TABLE (BẮT BUỘC) =====
        public int TableId { get; set; }
        public Table Table { get; set; } = null!;

        // ===== CUSTOMER (CHƯA CẦN KHI MỞ BÀN) =====
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime InvoiceDate { get; set; }

        public string Status { get; set; } = "";

        public decimal TotalAmount { get; set; }

        public List<InvoiceDetail> InvoiceDetails { get; set; } = new();
    }
}
