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
        Task AddErrorLogAsync(ErrorLog errorLog);
        Task<IEnumerable<ErrorLog>> GetControlledErrorsAsync();
        Task UpdateErrorLogAsync(ErrorLog errorLog);
        Task DeleteErrorLogAsync(string id);
    }
}
