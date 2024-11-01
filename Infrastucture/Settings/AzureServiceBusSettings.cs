using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastucture.Settings
{
    public class AzureServiceBusSettings
    {
        public string SendConnectionString { get; set; } = null!;
        public string ListenConnectionString { get; set; } = null!;
        public string ErrorQueueName { get; set; } = null!;
        public string FakerQueueName { get; set; } = null!;
    }

}
