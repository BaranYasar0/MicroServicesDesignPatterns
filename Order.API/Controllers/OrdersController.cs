using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }


        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {
            var newOrder = new Models.Order()
            {
                BuyerId = orderCreate.BuyerId,
                OrderStatus = OrderStatus.Suspend,
                Address = new Address()
                {
                    Line = orderCreate.AddressDto.Line,
                    Province = orderCreate.AddressDto.Province,
                    District = orderCreate.AddressDto.District
                },
                CreatedDate = DateTime.Now
                
            };

            orderCreate.OrderItems.ForEach(x =>
            {
                newOrder.OrderItems.Add(new OrderItem
                {
                    Price = x.Price,
                    ProductId = x.ProductId,
                    Count = x.Count
                });
            });

            await _context.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            var orderCreatedEvent = new OrderCreatedEvent
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                PaymentMessage = new PaymentMessage()
                {
                    CardName = orderCreate.PaymentDto.CardName,
                    CardNumber = orderCreate.PaymentDto.CardNumber,
                    Expiration = orderCreate.PaymentDto.Expiration,
                    CVV = orderCreate.PaymentDto.CVV,
                    TotalPrice = orderCreate.OrderItems.Sum(x => x.Price * x.Count)
                }

            };
            
            orderCreate.OrderItems.ForEach(x =>
            {
                orderCreatedEvent.OrderItemMessages.Add(new OrderItemMessage()
                {
                    Count = x.Count,
                    ProductId = x.ProductId
                });
            });

            await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok();
        }
    }
}
