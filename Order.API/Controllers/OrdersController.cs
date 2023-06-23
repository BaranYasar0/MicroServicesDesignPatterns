using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISendEndpointProvider _sendEndpointProvider;
 

        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _sendEndpointProvider = sendEndpointProvider;
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

            var orderCreatedEvent = new OrderCreatedRequestEvent()
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage()
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
                orderCreatedEvent.OrderItems.Add(new OrderItemMessage()
                {
                    Count = x.Count,
                    ProductId = x.ProductId
                });
            });

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.OrderSaga}"));

            await sendEndpoint.Send<IOrderCreatedRequestEvent>(orderCreatedEvent);

            //await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok();
        }
    }
}
