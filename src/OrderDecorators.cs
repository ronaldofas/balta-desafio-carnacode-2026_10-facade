using System;

namespace DesignPatternChallenge
{
    // Componente Concreto Base
    public class BaseOrderProcessor : IOrderProcessor
    {
        public void Process(OrderContext context)
        {
            Console.WriteLine($"\n[Base] Iniciando processamento do pedido para {context.Order.CustomerEmail}...");
            context.OrderId = $"ORD{DateTime.Now.Ticks}";
        }
    }

    // Decorator Base
    public abstract class OrderProcessorDecorator : IOrderProcessor
    {
        protected readonly IOrderProcessor _next;

        public OrderProcessorDecorator(IOrderProcessor next)
        {
            _next = next;
        }

        public virtual void Process(OrderContext context)
        {
            if (_next != null)
                _next.Process(context);
        }
    }
}
