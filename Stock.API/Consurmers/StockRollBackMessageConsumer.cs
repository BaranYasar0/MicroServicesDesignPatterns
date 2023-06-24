using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;
using Stock.API.Models;

namespace Stock.API.Consurmers
{
    public class StockRollBackMessageConsumer:IConsumer<IStockRollBackMessage>
    {
        private readonly ILogger<StockRollBackMessageConsumer> _logger;
        private readonly AppDbContext _context;

        public StockRollBackMessageConsumer(ILogger<StockRollBackMessageConsumer> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task Consume(ConsumeContext<IStockRollBackMessage> context)
        {
            foreach (var messageOrderItem in context.Message.OrderItems)
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(x => x.ProductId == messageOrderItem.ProductId);

                if (stock is not null)
                {
                    stock.Count += messageOrderItem.Count;
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation($"Stock was roll back");
        }
    }
}
