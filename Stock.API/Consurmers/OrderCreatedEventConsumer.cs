using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Models;

namespace Stock.API.Consurmers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
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

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();

            foreach (var item in context.Message.OrderItemMessages)
            {
                stockResult.Add(await _context.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }

            if (stockResult.All(x => x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItemMessages)
                {
                    var stock = await _context.Stocks.FirstOrDefaultAsync(x => x.Id == item.ProductId);

                    if (stock != null)
                        stock.Count -= item.Count;

                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Stock was reserved for Buyer Id :{context.Message.BuyerId}");


                var sendEndpoint =
                    await _sendEndpointProvider.GetSendEndpoint(
                        new Uri($"queue:{RabbitMqSettings.StockReservedEventQueueName}"));

                StockReservedEvent stockReservedEvent = new StockReservedEvent()
                {
                    Payment = context.Message.PaymentMessage,
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItemMessages
                };

                await sendEndpoint.Send(stockReservedEvent);
            }
            else
            {
                var stockNotReservedEvent = new StockNotReservedEvent()
                {
                    OrderId = context.Message.OrderId,
                    Message = "Not enough stock"
                };

                await _publishEndpoint.Publish(stockNotReservedEvent);

                _logger.LogInformation(stockNotReservedEvent.Message);

            }
        }
    }
}
