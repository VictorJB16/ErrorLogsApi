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

        // Constructor que inicializa _errorLogRepository
        public ErrorLogService(IErrorLogRepository errorLogRepository)
        {
            _errorLogRepository = errorLogRepository;
        }

        public async Task AddErrorLogAsync(FailedPurchase errorLog)
        {
            if (_errorLogRepository == null)
            {
                throw new InvalidOperationException("ErrorLogRepository is not initialized.");
            }

            // Validar solo errores de tipo transacción
            if (ValidateError(errorLog))
            {
                errorLog.Id = Guid.NewGuid();  // Asignar un ID único
                errorLog.CreatedAt = DateTime.UtcNow;  // Fecha de creación
                await _errorLogRepository.AddErrorLogAsync(errorLog);  // Guardar el error
            }
            else
            {
                throw new InvalidOperationException("Error type is invalid for transaction");
            }
        }

        // Validar solo errores de tipo transacción
        public bool ValidateError(FailedPurchase errorLog)
        {
            return errorLog.Status != "500" && errorLog.Status != "404"; // Ignorar 500 y 404
        }

        // Obtener todos los registros de error
        public async Task<IEnumerable<FailedPurchase>> GetAllErrorLogsAsync()
        {
            return await _errorLogRepository.GetAllErrorLogsAsync();
        }

        // Obtener solo errores controlados (transacciones fallidas)
        public async Task<IEnumerable<FailedPurchase>> GetControlledErrorsAsync()
        {
            return await _errorLogRepository.GetControlledErrorsAsync();
        }
        public async Task UpdateErrorLogAsync(FailedPurchase failedPurchase)
        {
            await _errorLogRepository.UpdateErrorLogAsync(failedPurchase);
        }

        public async Task DeleteErrorLogAsync(Guid id)
        {
            await _errorLogRepository.DeleteErrorLogAsync(id);
        }

        public async Task<IEnumerable<FailedPurchase>> GetNewErrorLogsAsync()
        {
            return await _errorLogRepository.GetNewErrorLogsAsync();   // Obtener los errores nuevos
        }
    }
}
