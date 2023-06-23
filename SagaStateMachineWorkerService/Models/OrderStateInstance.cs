using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateInstance:SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public string BuyerId { get; set; }

        public int OrderId { get; set; }

        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedDate { get; set; }

        public override string ToString()
        {
            var properties = GetType().GetProperties();

            var sb = new StringBuilder();

            properties.ToList().ForEach(x =>
            {
                var value=x.GetValue(this);
                sb.Append($"{x.Name}:{value}\n");
            });

            sb.Append("-------------------------");
            return sb.ToString();
        }
    }
}
