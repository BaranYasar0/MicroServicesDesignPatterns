using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Interfaces;
using Stock.API.Models;

namespace Stock.API.Consurmers
{
    public class OrderCreatedEventConsumer:IConsumer<IOrderCreatedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderCreatedEventConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext context, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();

            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(await _context.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }

            if (stockResult.All(x => x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _context.Stocks.FirstOrDefaultAsync(x => x.Id == item.ProductId);

                    if (stock != null)
                        stock.Count -= item.Count;

                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Stock was reserved for Instance Id :{context.Message.CorrelationId}");


                //var sendEndpoint =
                //    await _sendEndpointProvider.GetSendEndpoint(
                //        new Uri($"queue:{RabbitMqSettings.StockReservedEventQueueName}"));

                StockReservedEvent stockReservedEvent = new StockReservedEvent(context.Message.CorrelationId)
                {
                    
                    OrderItems = context.Message.OrderItems
                };

                await _publishEndpoint.Publish(stockReservedEvent);
            }
            else
            {
                var stockNotReservedEvent = new StockNotReservedEvent(context.Message.CorrelationId)
                {
                    Reason = "Not enough stock"
                };

                await _publishEndpoint.Publish(stockNotReservedEvent);

                _logger.LogInformation(stockNotReservedEvent.Reason);

            }
        }
    }
}
