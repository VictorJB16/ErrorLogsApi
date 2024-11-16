using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ErrorLogsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorLogController : ControllerBase
    {
        private readonly ErrorLogService _errorLogService;

        public ErrorLogController(ErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }

        // Endpoint para recibir el error
        [HttpPost]
        public async Task<IActionResult> ReceiveError([FromBody] FailedPurchase errorLog)
        {
            if (errorLog == null)
                return BadRequest("No data received");

            // Establecer la fecha de creación si no se envía
            errorLog.CreatedAt = DateTime.UtcNow;

            // Aquí ya no validamos si el error es reintentable, simplemente lo almacenamos
            await _errorLogService.AddErrorLogAsync(errorLog);

            return Ok("Error log stored successfully");
        }

        // Endpoint para obtener todos los errores almacenados
        [HttpGet]
        public async Task<IActionResult> GetAllErrorLogs()
        {
            var errorLogs = await _errorLogService.GetAllErrorLogsAsync();
            return Ok(errorLogs);
        }

        // Endpoint para obtener solo los errores controlados (errores reintentables)
        [HttpGet("controlled")]
        public async Task<IActionResult> GetControlledErrors()
        {
            var controlledErrors = await _errorLogService.GetControlledErrorsAsync();
            return Ok(controlledErrors);
        }

        // Endpoint para enviar errores en tiempo real usando SSE
        /*[HttpGet("stream")]
        public async Task StreamErrors(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            try
            {
                // Mantener la conexión abierta para enviar eventos en tiempo real
                while (!cancellationToken.IsCancellationRequested)
                {
                    var newErrorLogs = await _errorLogService.GetNewErrorLogsAsync();
                    foreach (var errorLog in newErrorLogs)
                    {
                        await Response.WriteAsync($"data: {JsonConvert.SerializeObject(errorLog)}\n\n");
                        await Response.Body.FlushAsync();
                    }

                    await Task.Delay(10000);  // Espera 10 segundos antes de enviar los siguientes errores
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la transmisión de errores: {ex.Message}");
            }
        }*/
    }

}