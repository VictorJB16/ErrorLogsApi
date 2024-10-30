using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ErrorLog
    {
        public string Id { get; set; } = null!;
        public string ErrorJson { get; set; } = null!;
        public DateTime OccurredAt { get; set; }
        public int RetryCount { get; set; } = 0;
        public bool IsControlled { get; set; } = true;
    }
}
