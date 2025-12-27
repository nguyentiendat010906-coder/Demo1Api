namespace Demo1Api.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        // ⏱ Thời gian bắt đầu
        public DateTime InvoiceDate { get; set; }

        // ⏱ Thời gian kết thúc (có thể null)
        public DateTime? EndTime { get; set; }

        public string Status { get; set; } = "open"; // open | paid

        public decimal Subtotal { get; set; }        // Tổng trước VAT
        public decimal VatAmount { get; set; }       // Tiền VAT
        public decimal TotalAmount { get; set; }     // Tổng sau VAT

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? TableId { get; set; }
        public Table? Table { get; set; }

        public string? CashierName { get; set; }

        public List<InvoiceDetail> InvoiceDetails { get; set; } = new();
    }
}
