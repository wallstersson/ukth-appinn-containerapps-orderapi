using Azure.Storage.Queues;

namespace HttpApi;

public class MessageQueueClient
{
    private readonly ILogger<MessageQueueClient> _logger;
    private readonly QueueClient _queueClient;
    
    public MessageQueueClient(IConfiguration configuration, ILogger<MessageQueueClient> logger)
    {
        _logger = logger;
        var connectionString = configuration["QueueConnectionString"];
        var queueName =configuration["QueueName"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("QueueConnectionString", "'QueueConnectionString' config value is required. Please add an environment variable or app setting.");
        }

        if (string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentNullException("QueueName", "'QueueName' config value is required. Please add an environment variable or app setting.");
        }

        _queueClient = new QueueClient(connectionString, queueName);
    }

    public async Task<string> GetQueueInfo()
    {
        var response = await _queueClient.GetPropertiesAsync();
        var properties = response.Value;
        
        return $"Queue '{_queueClient.Name}' has {properties.ApproximateMessagesCount} message{(properties.ApproximateMessagesCount != 1 ? "s" : "")}";
    }

    public async Task<bool> SendMessage(string message) => await SendMessageToQueue(Guid.NewGuid().ToString());
    //public async Task<bool> SendMessage(string message) => await SendMessageToQueue($"{Guid.NewGuid()}--{message}");

    private async Task<bool> SendMessageToQueue(string message)
    {
        try
        {
            await _queueClient.SendMessageAsync(message);
            return true;
        }
        catch (Azure.RequestFailedException rfe)
        {
            _logger.LogError($"Something went wrong connecting to the queue: {rfe}");
            return false;
        }
    }
}