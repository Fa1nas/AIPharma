namespace AIPharma.Models
{
    public class AdminDashboardViewModel
    {
        public int UsersCount { get; set; }
        public int ProductsCount { get; set; }
        public int PharmaciesCount { get; set; }
        public int OrdersCount { get; set; }
        public int ComplaintsCount { get; set; }
        public int FaqCount { get; set; }
        public int ChatMessagesCount { get; set; }

        public List<AdminPharmacyOrderStat> OrdersByPharmacy { get; set; } = new();
        public List<AdminPopularProductStat> PopularProducts { get; set; } = new();
        public List<AdminComplaintStat> ComplaintsByPharmacy { get; set; } = new();
        public List<AdminAiStat> AiStats { get; set; } = new();
        public List<AdminEmployeeSalaryStat> EmployeeSalaryStats { get; set; } = new();
    }

    public class AdminPharmacyOrderStat
    {
        public string PharmacyName { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class AdminPopularProductStat
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class AdminComplaintStat
    {
        public string PharmacyName { get; set; } = string.Empty;
        public int ComplaintsCount { get; set; }
    }

    public class AdminAiStat
    {
        public string Source { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class AdminEmployeeSalaryStat
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal Salary { get; set; }
    }
}