using AIPharma.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AIPharma.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
        public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<ProductComparison> ProductComparisons => Set<ProductComparison>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Complaint> Complaints => Set<Complaint>();
        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<SystemPrompt> SystemPrompts => Set<SystemPrompt>();
        public DbSet<FaqAnswer> FaqAnswers => Set<FaqAnswer>();
        public DbSet<AiCachedAnswer> AiCachedAnswers => Set<AiCachedAnswer>();
        public DbSet<ApiRequestLog> ApiRequestLogs => Set<ApiRequestLog>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<EmployeeWorkLog> EmployeeWorkLogs => Set<EmployeeWorkLog>();
        public DbSet<EmployeeVacation> EmployeeVacations => Set<EmployeeVacation>();
        public DbSet<EmployeeSickLeave> EmployeeSickLeaves => Set<EmployeeSickLeave>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Favorite>()
                .HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();

            modelBuilder.Entity<ProductComparison>()
                .HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();

            modelBuilder.Entity<ProductStock>()
                .HasIndex(x => new { x.ProductId, x.PharmacyId })
                .IsUnique();
        }
    }
}