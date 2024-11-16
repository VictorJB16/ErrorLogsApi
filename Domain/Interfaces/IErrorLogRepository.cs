using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IErrorLogRepository
    {
        Task AddErrorLogAsync(FailedPurchase failedPurchase);
        Task<IEnumerable<FailedPurchase>> GetControlledErrorsAsync(IEnumerable<string> controlledErrorMessages);
        Task<IEnumerable<FailedPurchase>> GetAllErrorLogsAsync();
        Task UpdateErrorLogAsync(FailedPurchase failedPurchase);
        Task DeleteErrorLogAsync(Guid id);
        Task<IEnumerable<FailedPurchase>> GetNewErrorLogsAsync();
    }
}
