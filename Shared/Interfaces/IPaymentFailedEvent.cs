using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Shared.Interfaces
{
    public interface IPaymentFailedEvent:CorrelatedBy<Guid>
    {
        public string Reason { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
