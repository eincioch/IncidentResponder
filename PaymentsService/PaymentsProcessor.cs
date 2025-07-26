using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentsService;

public class PaymentDetails
{
    public string? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentsProcessor
{
    private readonly IPaymentGateway? _paymentGateway;
    private readonly Dictionary<string, decimal> _transactionFees = new();

    public PaymentsProcessor(IPaymentGateway? paymentGateway = null)
    {
        _paymentGateway = paymentGateway;
        InitializeTransactionFees();
    }

    private void InitializeTransactionFees()
    {
        _transactionFees.Add("USD", 0.029m);
        _transactionFees.Add("EUR", 0.035m);
        _transactionFees.Add("GBP", 0.032m);
    }

    /// <summary>
    /// BUG: This method has a deliberate NullReferenceException that GitHub Copilot Agent can help identify
    /// The bug occurs when paymentDetails.Currency is null and we try to access the dictionary
    /// MCP servers can provide log context to help debug this issue
    /// </summary>
    public async Task<bool> ProcessPayment(PaymentDetails paymentDetails)
    {
        try
        {
            // DELIBERATE BUG: No null check for paymentDetails
            ValidatePaymentData(paymentDetails);
            
            var transactionFee = CalculateTransactionFee(paymentDetails);
            var totalAmount = paymentDetails.Amount + transactionFee;

            // DELIBERATE BUG: _paymentGateway could be null
            var result = await _paymentGateway!.ProcessAsync(paymentDetails.PaymentId!, totalAmount);
            
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            // Log the error - this is where MCP log query server can help
            Console.WriteLine($"ERROR in PaymentsProcessor.cs: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// POTENTIAL BUG: This method doesn't handle null paymentDetails properly
    /// GitHub Copilot Agent can suggest defensive programming patterns here
    /// </summary>
    private void ValidatePaymentData(PaymentDetails paymentDetails)
    {
        // DELIBERATE BUG: Direct property access without null check
        if (string.IsNullOrEmpty(paymentDetails.PaymentId))
            throw new ArgumentException("PaymentId cannot be null or empty");

        if (paymentDetails.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        // DELIBERATE BUG: Currency could be null, causing NullReferenceException in CalculateTransactionFee
        if (string.IsNullOrEmpty(paymentDetails.Currency))
            throw new ArgumentException("Currency cannot be null or empty");
    }

    /// <summary>
    /// BUG: This method assumes paymentDetails and Currency are never null
    /// MCP metrics server can show error rates to highlight this problematic method
    /// </summary>
    private decimal CalculateTransactionFee(PaymentDetails paymentDetails)
    {
        // DELIBERATE BUG: No null check for paymentDetails parameter
        // DELIBERATE BUG: Dictionary access without ContainsKey check
        var feeRate = _transactionFees[paymentDetails.Currency!]; // Could throw KeyNotFoundException
        
        return paymentDetails.Amount * feeRate;
    }

    public async Task<List<PaymentDetails>> GetFailedPayments()
    {
        // Simulate some failed payments for demo purposes
        return new List<PaymentDetails>
        {
            new() { PaymentId = "PMT-12345", Amount = 299.99m, Currency = null, CustomerId = "CUST-001", CreatedAt = DateTime.Now.AddMinutes(-30) },
            new() { PaymentId = "PMT-12347", Amount = 89.99m, Currency = "USD", CustomerId = null, CreatedAt = DateTime.Now.AddMinutes(-20) }
        };
    }
}

// Mock interface for demonstration
public interface IPaymentGateway
{
    Task<PaymentResult> ProcessAsync(string paymentId, decimal amount);
}

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}