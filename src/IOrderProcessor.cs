using System;
using DesignPatternChallenge;

namespace DesignPatternChallenge
{
    // Interface base para o Componente de processamento de pedidos
    public interface IOrderProcessor
    {
        void Process(OrderContext context);
    }
}
