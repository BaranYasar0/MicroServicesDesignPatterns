using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        
        public PaymentMessage PaymentMessage { get; set; }

        public List<OrderItemMessage> OrderItemMessages { get; set; } = new List<OrderItemMessage>();

    }
}
