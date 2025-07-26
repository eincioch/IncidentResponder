using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrdersService;

public class Order
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// Clean, well-implemented OrdersProcessor for contrast with the buggy PaymentsProcessor
/// This demonstrates good practices that GitHub Copilot Agent can recommend
/// </summary>
public class OrdersProcessor
{
    private readonly List<Order> _orders = new();

    /// <summary>
    /// Properly implemented method with null checks and validation
    /// Shows best practices that contrast with PaymentsProcessor bugs
    /// </summary>
    public async Task<bool> ProcessOrder(Order order)
    {
        try
        {
            // Proper null validation
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            ValidateOrder(order);
            
            // Calculate total amount safely
            order.TotalAmount = CalculateOrderTotal(order);
            order.Status = OrderStatus.Processing;
            order.CreatedAt = DateTime.UtcNow;

            // Simulate processing
            await Task.Delay(100);

            _orders.Add(order);
            
            Console.WriteLine($"Successfully processed order {order.OrderId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing order: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Comprehensive validation with proper null checks
    /// </summary>
    private static void ValidateOrder(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.OrderId))
            throw new ArgumentException("OrderId cannot be null or empty", nameof(order));

        if (string.IsNullOrWhiteSpace(order.CustomerId))
            throw new ArgumentException("CustomerId cannot be null or empty", nameof(order));

        if (order.Items == null || order.Items.Count == 0)
            throw new ArgumentException("Order must have at least one item", nameof(order));

        foreach (var item in order.Items)
        {
            if (string.IsNullOrWhiteSpace(item.ProductId))
                throw new ArgumentException("Product ID cannot be null or empty");

            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            if (item.UnitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative");
        }
    }

    /// <summary>
    /// Safe calculation with null protection
    /// </summary>
    private static decimal CalculateOrderTotal(Order order)
    {
        decimal total = 0;
        
        foreach (var item in order.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }

        return total;
    }

    public Task<List<Order>> GetOrdersByCustomer(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId cannot be null or empty", nameof(customerId));

        var customerOrders = _orders.FindAll(o => o.CustomerId == customerId);
        return Task.FromResult(customerOrders);
    }

    public Task<Order?> GetOrderById(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("OrderId cannot be null or empty", nameof(orderId));

        var order = _orders.Find(o => o.OrderId == orderId);
        return Task.FromResult(order);
    }

    public Task<int> GetOrderCount() => Task.FromResult(_orders.Count);
}