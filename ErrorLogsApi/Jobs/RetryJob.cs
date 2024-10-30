using Application.Services;
using Domain.Entities;
using Newtonsoft.Json.Linq;
using Quartz;

namespace ErrorLogsApi.Jobs
{
    [DisallowConcurrentExecution]
    public class RetryJob : IJob
    {
        private readonly ErrorLogService _errorLogService;

        public RetryJob(ErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var controlledErrors = await _errorLogService.GetControlledErrorsAsync();

            foreach (var error in controlledErrors)
            {
                bool success = await RetryProcessAsync(error);

                if (success)
                {
                    await _errorLogService.DeleteErrorLogAsync(error.Id);
                }
                else
                {
                    error.RetryCount++;

                    if (error.RetryCount >= 3)
                    {
                        error.IsControlled = false;
                    }

                    await _errorLogService.UpdateErrorLogAsync(error);
                }
            }
        }

        private async Task<bool> RetryProcessAsync(ErrorLog error)
        {
            // Deserializar el JSON para acceder a los datos si es necesario
            var errorData = JObject.Parse(error.ErrorJson);

            // Implementa aquí la lógica de reintento
            // Por ahora, simularemos un proceso que falla
            await Task.Delay(500); // Simular proceso

            // Puedes basar el éxito en algún valor dentro de errorData
            return false; // Cambia a true para simular éxito
        }
    }
}
