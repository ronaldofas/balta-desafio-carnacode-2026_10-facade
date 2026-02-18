using System;

namespace DesignPatternChallenge
{
    // Inventory Decorator
    public class InventoryDecorator : OrderProcessorDecorator
    {
        private readonly InventorySystem _inventory;

        public InventoryDecorator(IOrderProcessor next, InventorySystem inventory) : base(next)
        {
            _inventory = inventory;
        }

        public override void Process(OrderContext context)
        {
            var productId = context.Order.ProductId;
            var quantity = context.Order.Quantity;

            // Pré-processamento: Validação e Reserva
            if (!_inventory.CheckAvailability(productId))
            {
                Console.WriteLine("❌ Produto indisponível");
                throw new Exception("Produto indisponível"); 
            }

            _inventory.ReserveProduct(productId, quantity);

            try
            {
                base.Process(context); // Chama o próximo
            }
            catch
            {
                // Compensação em caso de erro nos próximos passos
                _inventory.ReleaseReservation(productId, quantity);
                throw; // Re-lança a exceção para subir a cadeia
            }
        }
    }

    // Coupon Decorator
    public class CouponDecorator : OrderProcessorDecorator
    {
        private readonly CouponSystem _couponSystem;

        public CouponDecorator(IOrderProcessor next, CouponSystem couponSystem) : base(next)
        {
            _couponSystem = couponSystem;
        }

        public override void Process(OrderContext context)
        {
            // Pré-processamento: Cálculos
            if (!string.IsNullOrEmpty(context.Order.CouponCode))
            {
                if (_couponSystem.ValidateCoupon(context.Order.CouponCode))
                {
                    var discountPercent = _couponSystem.GetDiscount(context.Order.CouponCode);
                    context.DiscountAmount = context.Subtotal * discountPercent;
                }
            }

            base.Process(context); // Chama o próximo

            // Pós-processamento: Marcar como usado
            // (Só executa se não houve exceção na cadeia)
            if (!string.IsNullOrEmpty(context.Order.CouponCode))
            {
                _couponSystem.MarkCouponAsUsed(context.Order.CouponCode, context.Order.CustomerEmail);
            }
        }
    }

    // Shipping Decorator
    public class ShippingDecorator : OrderProcessorDecorator
    {
        private readonly ShippingService _shippingService;

        public ShippingDecorator(IOrderProcessor next, ShippingService shippingService) : base(next)
        {
            _shippingService = shippingService;
        }

        public override void Process(OrderContext context)
        {
            // Pré-processamento: Calcular Frete
            // Nota: Lógica original usava Quantity * 0.5m como peso
            context.ShippingCost = _shippingService.CalculateShipping(context.Order.ZipCode, context.Order.Quantity * 0.5m);

            base.Process(context); // Chama o próximo

            // Pós-processamento: Criar etiqueta
            // (Executa após pagamento ser confirmado pelo próximo decorator)
            context.LabelId = _shippingService.CreateShippingLabel(context.OrderId, context.Order.ShippingAddress);
            _shippingService.SchedulePickup(context.LabelId, DateTime.Now.AddDays(1));
        }
    }

    // Payment Decorator
    public class PaymentDecorator : OrderProcessorDecorator
    {
        private readonly PaymentGateway _payment;

        public PaymentDecorator(IOrderProcessor next, PaymentGateway payment) : base(next)
        {
            _payment = payment;
        }

        public override void Process(OrderContext context)
        {
            // Pré-processamento: Pagamento
            string transactionId = _payment.InitializeTransaction(context.Total);
            context.TransactionId = transactionId;

            if (!_payment.ValidateCard(context.Order.CreditCard, context.Order.Cvv))
            {
                Console.WriteLine("❌ Cartão inválido");
                throw new Exception("Cartão inválido");
            }

            if (!_payment.ProcessPayment(transactionId, context.Order.CreditCard))
            {
                Console.WriteLine("❌ Pagamento recusado");
                throw new Exception("Pagamento recusado");
            }

            try 
            {
                base.Process(context); // Chama o próximo
            }
            catch 
            {
                // Compensação: Reembolso (se necessário)
                _payment.RollbackTransaction(transactionId);
                throw;
            }
        }
    }

    // Notification Decorator
    public class NotificationDecorator : OrderProcessorDecorator
    {
        private readonly NotificationService _notification;

        public NotificationDecorator(IOrderProcessor next, NotificationService notification) : base(next)
        {
            _notification = notification;
        }

        public override void Process(OrderContext context)
        {
            base.Process(context); // Executa tudo primeiro

            // Pós-processamento: Envia notificações DEPOIS que tudo deu certo
            _notification.SendOrderConfirmation(context.Order.CustomerEmail, context.OrderId);
            _notification.SendPaymentReceipt(context.Order.CustomerEmail, context.TransactionId);
            _notification.SendShippingNotification(context.Order.CustomerEmail, context.LabelId);
            
            Console.WriteLine($"\n✅ Pedido {context.OrderId} finalizado com sucesso!");
            Console.WriteLine($"   Total: R$ {context.Total:N2}");
        }
    }
}
