using System;

namespace Hoganvest.Core.Dtos
{
    public class UrjanetStatementsDto
    {
        public string LogicalAccountId { get; set; }
        public string AccountType { get; set; }
        public long? NormalizedAccountNumber { get; set; }
        public string RawAccountNumber { get; set; }
        public string UtilityProviderId { get; set; }
        public string ProviderName { get; set; }
        public string SummaryAccountNumber { get; set; }
        public string StatementId { get; set; }
        public string InvoiceNumber { get; set; }
        public string StatementType { get; set; }
        public DateTime? StatementDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ServiceRecipientName { get; set; }
        public string ServiceStreet { get; set; }
        public string ServiceStreet2 { get; set; }
        public string ServiceCity { get; set; }
        public string ServiceState { get; set; }
        public int? ServiceZipCode { get; set; }
        public string RawServiceAddress { get; set; }
        public string BillingRecipientName { get; set; }
        public string BillingStreet { get; set; }
        public string BillingStreet2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingZipCode { get; set; }
        public string RawBillingAddress { get; set; }
        public string PaymentRecipientName { get; set; }
        public string PaymentStreet { get; set; }
        public string PaymentStreet2 { get; set; }
        public string PaymentCity { get; set; }
        public string PaymentState { get; set; }
        public string PaymentZipCode { get; set; }
        public string RawPaymentAddress { get; set; }
        public double? TotalDue { get; set; }
        public double? OutstandingBalance { get; set; }
        public double? CurrentCharges { get; set; }
        public double? PreviousCharges { get; set; }
        public string PaymentCurrency { get; set; }
        public string CorrelationId { get; set; }
        public int? PropertyId { get; set; }
    }
}
