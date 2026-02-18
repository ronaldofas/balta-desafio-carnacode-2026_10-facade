using System;

namespace DesignPatternChallenge
{
    public class OrderContext
    {
        public OrderDTO Order { get; }
        public decimal Subtotal => Order.ProductPrice * Order.Quantity;
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total => Subtotal - DiscountAmount + ShippingCost;

        // Dados gerados durante o processo
        public string TransactionId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string LabelId { get; set; } = string.Empty;

        public OrderContext(OrderDTO order)
        {
            Order = order;
        }
    }
}
