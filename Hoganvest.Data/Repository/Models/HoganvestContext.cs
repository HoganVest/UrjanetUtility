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

        public virtual DbSet<CredentialDetails> CredentialDetails { get; set; }
        public virtual DbSet<Credential> Credential { get; set; }
        public virtual DbSet<UrjanetStatements> UrjanetStatements { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=SQLDB-01; Database =Urjanet; User Id=sa; Password=Phx0ff!c@structureproperties;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CredentialDetails>(entity =>
            {
                entity.HasKey(e => e.CredentialDetailsId)
                    .HasName("PK__Credentia__1B4D67B5CA456128");

                entity.Property(e => e.AccountNumber)
                    .HasColumnName("Account Number")
                    .HasMaxLength(100);

                entity.Property(e => e.AccountStatus)
                    .HasColumnName("Account Status")
                    .HasMaxLength(100);

                entity.Property(e => e.PropertyId).HasMaxLength(100);

                entity.HasOne(d => d.Credential)
                    .WithMany(p => p.CredentialDetails)
                    .HasForeignKey(d => d.CredentialId)
                    .HasConstraintName("FK__Credential__Crden__4BAC3F29");
            });

            modelBuilder.Entity<Credential>(entity =>
            {
                entity.HasKey(e => e.CredentialId)
                    .HasName("PK_UrjanetCredentials");

                entity.Property(e => e.CorrelationId).HasMaxLength(100);

                entity.Property(e => e.Created).HasColumnType("date");

                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                entity.Property(e => e.LastModified).HasColumnType("date");

                entity.Property(e => e.LastModifiedBy).HasMaxLength(100);

                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ProviderName).HasMaxLength(100);

                entity.Property(e => e.Status).HasMaxLength(100);

                entity.Property(e => e.UserName).HasMaxLength(100);

                entity.Property(e => e.Website)
                    .HasColumnName("website")
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });
            modelBuilder.Entity<UrjanetStatements>(entity =>
            {
                entity.HasKey(e => e.LogicalAccountId)
                    .HasName("PK__UrjanetS__2F4FA634F7E25425");

                entity.Property(e => e.LogicalAccountId)
                    .HasColumnName("Logical_Account_Id")
                    .HasMaxLength(50);

                entity.Property(e => e.AccountType)
                    .HasColumnName("Account_Type")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingCity)
                    .HasColumnName("Billing_City")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingRecipientName)
                    .HasColumnName("Billing_Recipient_Name")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingState)
                    .HasColumnName("Billing_State")
                    .HasMaxLength(10);

                entity.Property(e => e.BillingStreet)
                    .HasColumnName("Billing_Street")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingStreet2)
                    .HasColumnName("Billing_Street_2")
                    .HasMaxLength(50);

                entity.Property(e => e.BillingZipCode)
                    .HasColumnName("Billing_Zip_Code")
                    .HasMaxLength(50);

                entity.Property(e => e.CorrelationId)
                    .HasColumnName("Correlation_Id")
                    .HasMaxLength(50);

                entity.Property(e => e.CurrentCharges).HasColumnName("Current_Charges");

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due_Date")
                    .HasColumnType("datetime");

                entity.Property(e => e.EndDate)
                    .HasColumnName("End_Date")
                    .HasColumnType("datetime");

                entity.Property(e => e.InvoiceNumber)
                    .HasColumnName("Invoice_Number")
                    .HasMaxLength(50);

                entity.Property(e => e.NormalizedAccountNumber).HasColumnName("Normalized_Account_Number");

                entity.Property(e => e.OutstandingBalance).HasColumnName("Outstanding_Balance");

                entity.Property(e => e.PaymentCity)
                    .HasColumnName("Payment_City")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentCurrency)
                    .HasColumnName("Payment_Currency")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentRecipientName)
                    .HasColumnName("Payment_Recipient_Name")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentState)
                    .HasColumnName("Payment_State")
                    .HasMaxLength(10);

                entity.Property(e => e.PaymentStreet)
                    .HasColumnName("Payment_Street")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentStreet2)
                    .HasColumnName("Payment_Street_2")
                    .HasMaxLength(50);

                entity.Property(e => e.PaymentZipCode)
                    .HasColumnName("Payment_Zip_Code")
                    .HasMaxLength(50);

                entity.Property(e => e.PreviousCharges).HasColumnName("Previous_Charges");

                entity.Property(e => e.PropertyId).HasColumnName("PropertyID");

                entity.Property(e => e.ProviderName)
                    .HasColumnName("Provider_Name")
                    .HasMaxLength(50);

                entity.Property(e => e.RawAccountNumber)
                    .HasColumnName("Raw_Account_Number")
                    .HasMaxLength(50);

                entity.Property(e => e.RawBillingAddress)
                    .HasColumnName("Raw_Billing_Address")
                    .HasMaxLength(50);

                entity.Property(e => e.RawPaymentAddress)
                    .HasColumnName("Raw_Payment_Address")
                    .HasMaxLength(50);

                entity.Property(e => e.RawServiceAddress)
                    .HasColumnName("Raw_Service_Address")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceCity)
                    .HasColumnName("Service_City")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceRecipientName)
                    .HasColumnName("Service_Recipient_Name")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceState)
                    .HasColumnName("Service_State")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceStreet)
                    .HasColumnName("Service_Street")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceStreet2)
                    .HasColumnName("Service_Street_2")
                    .HasMaxLength(50);

                entity.Property(e => e.ServiceZipCode).HasColumnName("Service_Zip_Code");

                entity.Property(e => e.StartDate)
                    .HasColumnName("Start_Date")
                    .HasColumnType("datetime");

                entity.Property(e => e.StatementDate)
                    .HasColumnName("Statement_Date")
                    .HasColumnType("datetime");

                entity.Property(e => e.StatementId)
                    .HasColumnName("Statement_Id")
                    .HasMaxLength(50);

                entity.Property(e => e.StatementType)
                    .HasColumnName("Statement_Type")
                    .HasMaxLength(50);

                entity.Property(e => e.SummaryAccountNumber)
                    .HasColumnName("Summary_Account_Number")
                    .HasMaxLength(50);

                entity.Property(e => e.TotalDue).HasColumnName("Total_Due");

                entity.Property(e => e.UtilityProviderId)
                    .HasColumnName("Utility_Provider_Id")
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
