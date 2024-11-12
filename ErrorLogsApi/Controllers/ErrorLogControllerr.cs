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

        [HttpPost]
        public async Task<IActionResult> ReceiveError([FromBody] object errorJson)
        {
            if (errorJson == null)
                return BadRequest();

            var errorLog = new ErrorLog
            {
                ErrorJson = JsonConvert.SerializeObject(errorJson),
                OccurredAt = DateTime.UtcNow
            };

            await _errorLogService.AddErrorLogAsync(errorLog);

            return Ok();
        }

        // Endpoint para enviar errores en tiempo real usando SSE
        [HttpGet("stream")]
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

                    
                    await Task.Delay(10000);  
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error en la transmisión de errores: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ErrorLog>>> GetAllErrorLogs()
        {
            var errorLogs = await _errorLogService.GetAllErrorLogsAsync();
            return Ok(errorLogs);
        }

        [HttpGet("controlled")]
        public async Task<ActionResult<IEnumerable<ErrorLog>>> GetControlledErrors()
        {
            var controlledErrors = await _errorLogService.GetControlledErrorsAsync();
            return Ok(controlledErrors);
        }
    }
}
