using MassTransit;
using Shared;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer:IConsumer<StockReservedEvent>
    {
        private readonly ILogger<StockReservedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(ILogger<StockReservedEventConsumer> logger, IPublishEndpoint endpoint)
        {
            _logger = logger;
            _publishEndpoint = endpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var balance = 100m;

            if (balance > context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was withdrawn from credit card from user id={context.Message.BuyerId}");

                await _publishEndpoint.Publish(new PaymentCompletedEvent
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId
                });
            }

            else
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was not  withdrawn from credit card from user id={context.Message.BuyerId}");

                await _publishEndpoint.Publish(new PaymentFailedEvent
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems,
                    Message =
                        $"{context.Message.Payment.TotalPrice} TL was not  withdrawn from credit card from user id={context.Message.BuyerId}"
                });
            }
        }
    }
}
