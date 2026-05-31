using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringPlatform.Domain.Entities
{
    public class MonitorMetric
    {
        public Guid MonitorId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ResponseTime { get; set; }
    }
}
