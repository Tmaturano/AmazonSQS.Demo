using Amazon.SQS;
using Amazon.SQS.Model;
using Messages;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Producer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IAmazonSQS _sqsClient;
    private readonly ILogger<OrdersController> _logger;
    private const string OrderCreatedEventQueueName = "order-created";

    public OrdersController(IAmazonSQS amazonSQS, ILogger<OrdersController> logger)
    {
        _sqsClient = amazonSQS;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync()
    {
        //Just to simulate an endpoint that supposes to receive those ids, save to database and then send to SQS
        var createdOrderId = Guid.NewGuid(); 
        var customerId = Guid.NewGuid();

        var orderCreatedEvent = new OrderCreatedEvent(createdOrderId, customerId);

        var queueUrl = await GetQueueUrlAsync();

        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(orderCreatedEvent)
        };

        _logger.LogInformation("Publishing message to Queue {queueName} with body : \n {request}", OrderCreatedEventQueueName, sendMessageRequest.MessageBody);

        var result = await _sqsClient.SendMessageAsync(sendMessageRequest);
        return Ok(result);
    }

    private async Task<string> GetQueueUrlAsync()
    {
        try
        {
            var response = await _sqsClient.GetQueueUrlAsync(OrderCreatedEventQueueName);

            return response.QueueUrl;
        }
        catch (QueueDoesNotExistException)
        {
            _logger.LogInformation("Queue {queueName} does not exist. Creating...", OrderCreatedEventQueueName);

            var response = await _sqsClient.CreateQueueAsync(OrderCreatedEventQueueName);
            return response.QueueUrl;
        }
    }
}
