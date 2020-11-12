using Microsoft.EntityFrameworkCore;

namespace Hoganvest.Data.Repository.Models
{
    public partial class HoganvestContext : DbContext
    {
        public HoganvestContext()
        {
        }

        public HoganvestContext(DbContextOptions<HoganvestContext> options)
            : base(options)
        {
        }

        public virtual DbSet<UrjanetStatements> UrjanetStatements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=192.168.19.145;Database=Hoganvest;integrated security=false;user id=sa;password=CT!@#QWE123");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UrjanetStatements>(entity =>
            {
                entity.HasKey(e => e.LogicalAccountId)
                    .HasName("PK__UrjanetS__2F4FA634F7E25425");

                entity.Property(e => e.LogicalAccountId)
                    .HasColumnName("Logical Account Id")
                    .HasMaxLength(50);

                entity.Property(e => e.AccountType)
                    .HasColumnName("Account Type")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingCity)
                    .HasColumnName("Billing City")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingRecipientName)
                    .HasColumnName("Billing Recipient Name")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingState)
                    .HasColumnName("Billing State")
                    .HasMaxLength(10);

                entity.Property(e => e.BillingStreet)
                    .HasColumnName("Billing Street")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingStreet2)
                    .HasColumnName("Billing Street 2")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingZipCode)
                    .HasColumnName("Billing Zip Code")
                    .HasMaxLength(50);

                entity.Property(e => e.CorrelationId)
                    .HasColumnName("Correlation Id")
                    .HasMaxLength(50);

                entity.Property(e => e.CurrentCharges).HasColumnName("Current Charges");

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasColumnType("datetime");

                entity.Property(e => e.EndDate)
                    .HasColumnName("End Date")
                    .HasColumnType("datetime");

                entity.Property(e => e.InvoiceNumber)
                    .HasColumnName("Invoice Number")
                    .HasMaxLength(50);

                entity.Property(e => e.NormalizedAccountNumber).HasColumnName("Normalized Account Number");

                entity.Property(e => e.OutstandingBalance).HasColumnName("Outstanding Balance");

                entity.Property(e => e.PaymentCity)
                    .HasColumnName("Payment City")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentCurrency)
                    .HasColumnName("Payment Currency")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentRecipientName)
                    .HasColumnName("Payment Recipient Name")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentState)
                    .HasColumnName("Payment State")
                    .HasMaxLength(10);

                entity.Property(e => e.PaymentStreet)
                    .HasColumnName("Payment Street")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentStreet2)
                    .HasColumnName("Payment Street 2")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentZipCode)
                    .HasColumnName("Payment Zip Code")
                    .HasMaxLength(50);

                entity.Property(e => e.PreviousCharges).HasColumnName("Previous Charges");

                entity.Property(e => e.PropertyId).HasColumnName("PropertyID");

                entity.Property(e => e.ProviderName)
                    .HasColumnName("Provider Name")
                    .HasMaxLength(50);

                entity.Property(e => e.RawAccountNumber)
                    .HasColumnName("Raw Account Number")
                    .HasMaxLength(50);

                entity.Property(e => e.RawBillingAddress)
                    .HasColumnName("Raw Billing Address")
                    .HasMaxLength(50);

                entity.Property(e => e.RawPaymentAddress)
                    .HasColumnName("Raw Payment Address")
                    .HasMaxLength(50);

                entity.Property(e => e.RawServiceAddress)
                    .HasColumnName("Raw Service Address")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceCity)
                    .HasColumnName("Service City")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceRecipientName)
                    .HasColumnName("Service Recipient Name")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceState)
                    .HasColumnName("Service State")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceStreet)
                    .HasColumnName("Service Street")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceStreet2)
                    .HasColumnName("Service Street 2")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceZipCode).HasColumnName("Service Zip Code");

                entity.Property(e => e.StartDate)
                    .HasColumnName("Start Date")
                    .HasColumnType("datetime");

                entity.Property(e => e.StatementDate)
                    .HasColumnName("Statement Date")
                    .HasColumnType("datetime");

                entity.Property(e => e.StatementId)
                    .HasColumnName("Statement Id")
                    .HasMaxLength(50);

                entity.Property(e => e.StatementType)
                    .HasColumnName("Statement Type")
                    .HasMaxLength(50);

                entity.Property(e => e.SummaryAccountNumber)
                    .HasColumnName("Summary Account Number")
                    .HasMaxLength(50);

                entity.Property(e => e.TotalDue).HasColumnName("Total Due");

                entity.Property(e => e.UtilityProviderId)
                    .HasColumnName("Utility Provider Id")
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
