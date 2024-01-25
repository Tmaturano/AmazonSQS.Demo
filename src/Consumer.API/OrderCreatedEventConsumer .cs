
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Consumer.API;

public class OrderCreatedEventConsumer : BackgroundService
{
    private readonly ILogger<OrderCreatedEventConsumer> _logger;
    private readonly IAmazonSQS _sqsClient;
    private const string OrderCreatedEventQueueName = "order-created";

    public OrderCreatedEventConsumer(ILogger<OrderCreatedEventConsumer> logger, IAmazonSQS sqsClient)
    {
        _logger = logger;
        _sqsClient = sqsClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Polling Queue {queueName}", OrderCreatedEventQueueName);
        var queueUrl = await GetQueueUrl();

        var receivedRequest = new ReceiveMessageRequest(queueUrl);

        while(!stoppingToken.IsCancellationRequested)
        {
            var response = await _sqsClient.ReceiveMessageAsync(receivedRequest);
            if (response.Messages.Any())
            {
                foreach (var message in response.Messages)
                {
                    _logger.LogInformation("Received Message from Queue {queueName} with body as : \n {body}", OrderCreatedEventQueueName, message.Body);

                    Task.Delay(3000).Wait(); //simulate some background processing

                    await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
                }
            }
        }
    }

    private async Task<string> GetQueueUrl()
    {
        try
        {
            var response = await _sqsClient.GetQueueUrlAsync(OrderCreatedEventQueueName);
            return response.QueueUrl;
        }
        catch (QueueDoesNotExistException)
        {
            _logger.LogInformation("Queue {queueName} doesn't exist. Creating...", OrderCreatedEventQueueName);
            var response = await _sqsClient.CreateQueueAsync(OrderCreatedEventQueueName);
            return response.QueueUrl;
        }
    }
}
