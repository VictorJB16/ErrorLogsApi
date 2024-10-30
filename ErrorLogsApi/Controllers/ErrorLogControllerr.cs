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
    }
}
