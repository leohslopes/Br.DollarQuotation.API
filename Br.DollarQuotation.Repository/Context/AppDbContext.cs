using Br.DollarQuotation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Br.DollarQuotation.Repository.Context
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users => Set<User>();

        public DbSet<CurrencyQuotation> CurrencyQuotations => Set<CurrencyQuotation>();

        public DbSet<QuotationAlert> QuotationAlerts => Set<QuotationAlert>();

        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly( typeof(AppDbContext).Assembly);
        }
    }
}