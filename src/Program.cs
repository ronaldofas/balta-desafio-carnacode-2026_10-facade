using System;

namespace DesignPatternChallenge
{
    public class Runner
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("       BALTA.IO - DESIGN PATTERNS CHALLENGE   ");
            Console.WriteLine("==============================================\n");
            
            Console.WriteLine("Escolha o modo de execução:");
            Console.WriteLine("1. Legado (Monolítico)");
            Console.WriteLine("2. Refatorado (Decorator)");
            Console.Write("\nOpção: ");

            var option = Console.ReadLine();

            if (option == "1")
            {
                Console.Clear();
                // Chamada ao código antigo
                DesignPatternChallenge.Program.Main(args);
                return;
            }

            if (option == "2")
            {
                RunDecoratorPipeline();
                return;
            }

            Console.WriteLine("Opção inválida.");
        }

        private static void RunDecoratorPipeline()
        {
            Console.Clear();
            Console.WriteLine("=== Processando Pedido (Via Decorator) ===\n");

            // 1. Setup dos Subsistemas
            var inventory = new InventorySystem();
            var payment = new PaymentGateway();
            var shipping = new ShippingService();
            var coupon = new CouponSystem();
            var notification = new NotificationService();

            // 2. Dados do Pedido (Mesmos do exemplo original)
            var order = new OrderDTO
            {
                ProductId = "PROD001",
                Quantity = 2,
                CustomerEmail = "cliente@email.com",
                CreditCard = "1234567890123456",
                Cvv = "123",
                ShippingAddress = "Rua Exemplo, 123",
                ZipCode = "12345-678",
                CouponCode = "PROMO10",
                ProductPrice = 100.00m
            };

            // 3. Criação do Pipeline (Decorators)
            // Ordem de construção (de dentro pra fora) define a execução
            
            IOrderProcessor pipeline = new BaseOrderProcessor();
            
            // Ordem de execução desejada (Pre): Notification -> Inventory -> Coupon -> Shipping -> Payment -> Base
            // Ordem de construção (Wrap):
            
            pipeline = new PaymentDecorator(pipeline, payment);       // Executa Pay antes do base
            pipeline = new ShippingDecorator(pipeline, shipping);     // Executa Calc Shipping antes do Payment
            pipeline = new CouponDecorator(pipeline, coupon);         // Executa Calc Coupon antes do Shipping
            pipeline = new InventoryDecorator(pipeline, inventory);   // Executa Check/Reserve antes do Coupon
            pipeline = new NotificationDecorator(pipeline, notification); // Envia notificação APÓS tudo voltar com sucesso

            // 4. Execução
            try
            {
                var context = new OrderContext(order);
                pipeline.Process(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erro no processamento: {ex.Message}");
            }
        }
    }
}
