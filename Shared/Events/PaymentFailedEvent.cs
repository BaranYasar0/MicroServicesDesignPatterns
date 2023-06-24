using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Interfaces;

namespace Shared.Events
{
    public class PaymentFailedEvent:IPaymentFailedEvent
    {
        public PaymentFailedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
        public string Reason { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
