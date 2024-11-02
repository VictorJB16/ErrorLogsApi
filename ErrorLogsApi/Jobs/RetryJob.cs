using Application.Services;
using Azure.Messaging.ServiceBus;
using Domain.Entities;
using Newtonsoft.Json.Linq;
using Quartz;
using Microsoft.Extensions.Options;
using Infrastucture.Settings;

namespace ErrorLogsApi.Jobs
{
    [DisallowConcurrentExecution]
    public class RetryJob : IJob
    {
        private readonly ErrorLogService _errorLogService;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RetryJob(IServiceScopeFactory serviceScopeFactory, IOptions<AzureServiceBusSettings> serviceBusSettings)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var settings = serviceBusSettings.Value;

            _client = new ServiceBusClient(settings.SendConnectionString);
            _sender = _client.CreateSender(settings.FakerQueueName);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var errorLogService = scope.ServiceProvider.GetRequiredService<ErrorLogService>();

                var controlledErrors = await errorLogService.GetControlledErrorsAsync();

                foreach (var error in controlledErrors)
                {
                    bool success = await RetryProcessAsync(error);

                    if (success)
                    {
                        await errorLogService.DeleteErrorLogAsync(error.Id);
                    }
                    else
                    {
                        error.RetryCount++;

                        if (error.RetryCount >= 3)
                        {
                            error.IsControlled = false;
                        }

                        await errorLogService.UpdateErrorLogAsync(error);
                    }
                }
            }
        }


        private async Task<bool> RetryProcessAsync(ErrorLog error)
        {
            try
            {
                // Crea un mensaje para enviar al faker
                var message = new ServiceBusMessage(error.ErrorJson);

                // Envía el mensaje
                await _sender.SendMessageAsync(message);

                // Aquí podrías implementar lógica adicional para verificar si el envío fue exitoso
                // Por simplicidad, asumiremos que fue exitoso si no hay excepción

                return true;
            }
            catch (Exception ex)
            {
                // Maneja la excepción, puedes registrar el error si es necesario
                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
