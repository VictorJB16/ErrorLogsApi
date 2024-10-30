using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ErrorLogService
    {
        private readonly IErrorLogRepository _errorLogRepository;

        public ErrorLogService(IErrorLogRepository errorLogRepository)
        {
            _errorLogRepository = errorLogRepository;
        }

        public async Task AddErrorLogAsync(ErrorLog errorLog)
        {
            await _errorLogRepository.AddErrorLogAsync(errorLog);
        }

        public async Task<IEnumerable<ErrorLog>> GetControlledErrorsAsync()
        {
            return await _errorLogRepository.GetControlledErrorsAsync();
        }

        public async Task UpdateErrorLogAsync(ErrorLog errorLog)
        {
            await _errorLogRepository.UpdateErrorLogAsync(errorLog);
        }

        public async Task DeleteErrorLogAsync(string id)
        {
            await _errorLogRepository.DeleteErrorLogAsync(id);
        }
    }
}
