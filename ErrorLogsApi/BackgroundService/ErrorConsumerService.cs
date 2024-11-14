using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using Azure.Messaging.ServiceBus;
using Domain.Entities;
using Infrastucture.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ErrorLogsApi.BackgroundServices
{
    public class ErrorConsumerService : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ErrorConsumerService(IServiceScopeFactory serviceScopeFactory, IOptions<AzureServiceBusSettings> serviceBusSettings)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var settings = serviceBusSettings.Value;

            _client = new ServiceBusClient(settings.ListenConnectionString);

            _processor = _client.CreateProcessor(settings.ErrorQueueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync(stoppingToken);
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var errorLogService = scope.ServiceProvider.GetRequiredService<ErrorLogService>();

                var messageBody = args.Message.Body.ToString();

                // Procesa el mensaje recibido y convierte el mensaje en FailedPurchase
                await ProcessErrorMessageAsync(messageBody, errorLogService);

                // Marca el mensaje como completado para que se elimine de la cola
                await args.CompleteMessageAsync(args.Message);
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            // Maneja cualquier error que ocurra durante el procesamiento de mensajes
            return Task.CompletedTask;
        }

        private async Task ProcessErrorMessageAsync(string message, ErrorLogService errorLogService)
        {
            // Convierte el mensaje en un objeto FailedPurchase
            var failedPurchase = new FailedPurchase
            {
                ErrorMessage = message,  // Asumiendo que el mensaje contiene el ErrorMessage
                CreatedAt = DateTime.UtcNow
            };

            // Almacena el error en la base de datos
            await errorLogService.AddErrorLogAsync(failedPurchase);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _processor.CloseAsync();
            await base.StopAsync(stoppingToken);
        }
    }
}
