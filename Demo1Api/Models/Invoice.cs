namespace Demo1Api.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }          // ← CỘT BẮT BUỘC
        public Customer Customer { get; set; } = null!;

        public DateTime InvoiceDate { get; set; }

        public string Status { get; set; } = "";

        public decimal TotalAmount { get; set; }

        public List<InvoiceDetail> InvoiceDetails { get; set; } = new();
    }

}
