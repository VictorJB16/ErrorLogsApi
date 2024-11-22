using Application.Services;
using Azure.Messaging.ServiceBus;
using Domain.Entities;
using Quartz;
using Microsoft.Extensions.Options;
using Infrastucture.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

namespace ErrorLogsApi.Jobs
{
    [DisallowConcurrentExecution]
    public class RetryJob : IJob
    {
        private readonly ErrorLogService _errorLogService;
        private readonly HttpClient _httpClient;

        public RetryJob(ErrorLogService errorLogService, IHttpClientFactory httpClientFactory)
        {
            _errorLogService = errorLogService;

            // Crear un cliente HTTP para llamar al endpoint
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // Obtener los errores controlados (solo aquellos que pueden ser reintentados)
            var controlledErrors = await _errorLogService.GetControlledErrorsAsync();

            foreach (var error in controlledErrors)
            {
                // Filtrar errores que son reintentables (IsRetriable = true)
                if (error.IsRetriable)
                {
                    bool success = await RetryProcessAsync(error);

                    if (success)
                    {
                        // Eliminar el error de la base de datos si el reintento fue exitoso
                        await _errorLogService.DeleteErrorLogAsync(error.Id);
                    }
                    else
                    {
                        // Incrementar el contador de reintentos
                        error.RetryCount++;

                        if (error.RetryCount >= 3)
                        {
                            // Después de 3 reintentos, marcar como no reintentable
                            error.IsRetriable = false;

                            // Lanzar una excepción para este error (el Job ha fallado)
                            await _errorLogService.UpdateErrorLogAsync(error);
                            throw new InvalidOperationException($"Error ID {error.Id}: No se pudo procesar después de 3 intentos");
                        }

                        // Actualizar el estado del error (aún reintentable)
                        await _errorLogService.UpdateErrorLogAsync(error);
                    }
                }
            }
        }

        private async Task<bool> RetryProcessAsync(FailedPurchase failedPurchase)
        {
            try
            {
                // Enviar solo el Id de la compra fallida al endpoint del Faker
                var errorPayload = new
                {
                    Id = failedPurchase.Id // Solo enviamos el ID
                };

                var json = JsonConvert.SerializeObject(errorPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Endpoint dinámico utilizando el ID de la compra
                var url = $"https://x5fq8jzp-7257.use2.devtunnels.ms/api/Purchases/retry/{failedPurchase.Id}";

                // Enviar el mensaje al endpoint
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return true; // El reintento fue exitoso
                }
                else
                {
                    return false; // El reintento falló
                }
            }
            catch (Exception ex)
            {
                // Maneja la excepción, puedes loguearlo si es necesario
                Console.WriteLine($"Error al procesar el mensaje: {ex.Message}");
                return false;
            }
        }


        //codigo para simular que el retry se hizo correctamente,regresa true entonces se elimina en la db

        //private async Task<bool> RetryProcessAsync(FailedPurchase failedPurchase)
        //{
        //    try
        //    {
        //        // Simulamos que el reintento fue exitoso
        //        Console.WriteLine($"Simulando envío al Faker para el error con ID {failedPurchase.Id}");

        //        // Simulación de un envío exitoso
        //        // Puedes poner una condición o contador aquí para simular fallos intermitentes si lo deseas
        //        await Task.Delay(1000);  // Simular un pequeño delay como si se estuviera procesando el mensaje

        //        // Regresar 'true' para indicar que el reintento fue exitoso
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // En caso de error, se loguea (simulamos un fallo)
        //        Console.WriteLine($"Error al simular el envío: {ex.Message}");
        //        return false;
        //    }
        //}

    }



}
