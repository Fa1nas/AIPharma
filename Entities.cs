using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AIPharma.Models
{
    public class Role
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new();
    }

    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public int? PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<Order> Orders { get; set; } = new();
        public List<Favorite> Favorites { get; set; } = new();
        public List<ChatSession> ChatSessions { get; set; } = new();
    }

    public class ProductCategory
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = new();
    }

    public class Product
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        public int ProductCategoryId { get; set; }
        public ProductCategory? ProductCategory { get; set; }
        [MaxLength(150)]
        public string Manufacturer { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Precision(10, 2)]
        public decimal Price { get; set; }
        [Precision(10, 2)]
        public decimal? DiscountPrice { get; set; }
        public bool IsPrescriptionRequired { get; set; }
        [MaxLength(300)]
        public string ImagePath { get; set; } = "/images/products/default.png";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<ProductStock> ProductStocks { get; set; } = new();
    }

    public class Pharmacy
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;
        [MaxLength(30)]
        public string Phone { get; set; } = string.Empty;
        [MaxLength(100)]
        public string WorkSchedule { get; set; } = "08:00 - 21:00";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<ProductStock> ProductStocks { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<Complaint> Complaints { get; set; } = new();
    }

    public class ProductStock
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }
        public int Quantity { get; set; }
    }

    public class Favorite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class ProductComparison
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }
        [MaxLength(50)]
        public string Status { get; set; } = "Новий";
        [MaxLength(50)]
        public string PaymentType { get; set; } = "Оплата при отриманні";
        [Precision(10, 2)]
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        [Precision(10, 2)]
        public decimal UnitPrice { get; set; }
    }

    public class Complaint
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public int PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Status { get; set; } = "Нова";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class ChatSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [MaxLength(150)]
        public string Title { get; set; } = "Нова консультація";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<ChatMessage> Messages { get; set; } = new();
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }
        public ChatSession? ChatSession { get; set; }
        [MaxLength(20)]
        public string Sender { get; set; } = string.Empty; // User або Assistant
        public string Text { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Source { get; set; } = string.Empty; // FAQ, CACHE, LLM
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class SystemPrompt
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public string PromptText { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class FaqAnswer
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(300)]
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Category { get; set; } = "Загальні питання";
        public int UseCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class AiCachedAnswer
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(300)]
        public string NormalizedQuestion { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int UseCount { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUsedAt { get; set; } = DateTime.Now;
    }

    public class ApiRequestLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public string RequestText { get; set; } = string.Empty;
        public string ResponseText { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Source { get; set; } = string.Empty; // FAQ, CACHE, LLM, MOCK
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Employee
    {
        public int Id { get; set; }
        public int PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Position { get; set; } = "Фармацевт";
        [Precision(10, 2)]
        public decimal HourlyRate { get; set; }
        public List<EmployeeWorkLog> WorkLogs { get; set; } = new();
    }

    public class EmployeeWorkLog
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public DateTime WorkDate { get; set; }
        [Precision(5, 2)]
        public decimal HoursWorked { get; set; }
    }

    public class EmployeeVacation
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [MaxLength(100)]
        public string Status { get; set; } = "Заплановано";
    }

    public class EmployeeSickLeave
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [MaxLength(200)]
        public string Comment { get; set; } = string.Empty;
    }
}