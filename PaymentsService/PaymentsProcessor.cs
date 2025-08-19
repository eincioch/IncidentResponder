using System;
using System.Collections.Generic;
using System.Threading;
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
    private readonly IPaymentGateway _paymentGateway;
    private readonly Dictionary<string, decimal> _transactionFees;

    // Retry/timeout policy settings
    private const int MaxGatewayRetries = 3;
    private static readonly TimeSpan GatewayTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromMilliseconds(250);

    public PaymentsProcessor(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _transactionFees = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            { "USD", 0.029m },
            { "EUR", 0.035m },
            { "GBP", 0.032m }
        };
    }

    /// <summary>
    /// Process a payment with defensive validation, safe fee calculation, and resilient gateway calls.
    /// </summary>
    public async Task<bool> ProcessPayment(PaymentDetails paymentDetails)
    {
        try
        {
            ValidatePaymentData(paymentDetails);

            var transactionFee = CalculateTransactionFee(paymentDetails);
            var totalAmount = paymentDetails.Amount + transactionFee;

            // Retry with exponential backoff and timeout to reduce transient failures/timeouts
            for (int attempt = 1; attempt <= MaxGatewayRetries; attempt++)
            {
                try
                {
                    var result = await _paymentGateway
                        .ProcessAsync(paymentDetails.PaymentId!, totalAmount)
                        .WaitAsync(GatewayTimeout)
                        .ConfigureAwait(false);

                    return result.IsSuccess;
                }
                catch (Exception ex) when (IsTransient(ex))
                {
                    if (attempt == MaxGatewayRetries)
                    {
                        Console.WriteLine($"[ERROR] Payment gateway call failed after {attempt} attempts: {ex.Message}");
                        break;
                    }

                    var delay = TimeSpan.FromMilliseconds(BaseRetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    Console.WriteLine($"[WARN] Transient gateway error (attempt {attempt}/{MaxGatewayRetries}): {ex.Message}. Retrying in {delay.TotalMilliseconds}ms...");
                    await Task.Delay(delay).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Non-transient error
                    Console.WriteLine($"[ERROR] Non-transient error calling payment gateway: {ex}");
                    return false;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            // Log and fail gracefully to reduce unhandled exceptions and error rates
            Console.WriteLine($"[ERROR] PaymentsProcessor.ProcessPayment failed: {ex}");
            return false;
        }
    }

    private static bool IsTransient(Exception ex)
    {
        // Consider timeouts and common transient gateway exceptions as retryable
        if (ex is TimeoutException)
            return true;

        if (ex is InvalidOperationException ioe && ioe.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            return true;

        // TaskCanceledException may bubble from WaitAsync
        if (ex is TaskCanceledException)
            return true;

        return false;
    }

    /// <summary>
    /// Validate incoming payment data with full null checks and normalization.
    /// </summary>
    private static void ValidatePaymentData(PaymentDetails? paymentDetails)
    {
        if (paymentDetails is null)
            throw new ArgumentNullException(nameof(paymentDetails));

        if (string.IsNullOrWhiteSpace(paymentDetails.PaymentId))
            throw new ArgumentException("PaymentId cannot be null or empty", nameof(paymentDetails));

        if (paymentDetails.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(paymentDetails));

        if (string.IsNullOrWhiteSpace(paymentDetails.Currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(paymentDetails));

        // Normalize currency for consistent lookups
        paymentDetails.Currency = paymentDetails.Currency.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(paymentDetails.CustomerId))
            throw new ArgumentException("CustomerId cannot be null or empty", nameof(paymentDetails));
    }

    /// <summary>
    /// Safe fee calculation that tolerates unknown currencies.
    /// </summary>
    private decimal CalculateTransactionFee(PaymentDetails paymentDetails)
    {
        if (paymentDetails is null)
            throw new ArgumentNullException(nameof(paymentDetails));

        var currency = paymentDetails.Currency?.Trim().ToUpperInvariant();

        // Default to a conservative fee if currency is unknown to avoid KeyNotFoundException
        const decimal defaultFeeRate = 0.03m;
        var feeRate = defaultFeeRate;

        if (!string.IsNullOrEmpty(currency) && _transactionFees.TryGetValue(currency, out var knownRate))
        {
            feeRate = knownRate;
        }

        return Math.Round(paymentDetails.Amount * feeRate, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<List<PaymentDetails>> GetFailedPayments()
    {
        // Simulate some failed payments for demo purposes
        return new List<PaymentDetails>
        {
            new() { PaymentId = "PMT-12345", Amount = 299.99m, Currency = "USD", CustomerId = "CUST-001", CreatedAt = DateTime.Now.AddMinutes(-30) },
            new() { PaymentId = "PMT-12347", Amount = 89.99m, Currency = "USD", CustomerId = "CUST-002", CreatedAt = DateTime.Now.AddMinutes(-20) }
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