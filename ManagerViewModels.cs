namespace AIPharma.Models
{
    public class ManagerDashboardViewModel
    {
        public string PharmacyName { get; set; } = string.Empty;
        public string PharmacyAddress { get; set; } = string.Empty;

        public int OrdersCount { get; set; }
        public int ComplaintsCount { get; set; }
        public int ProductsInStockCount { get; set; }
        public int EmployeesCount { get; set; }

        public decimal TotalOrdersAmount { get; set; }

        public List<AdminPopularProductStat> PopularProducts { get; set; } = new();
        public List<AdminEmployeeSalaryStat> EmployeeSalaryStats { get; set; } = new();
    }
}