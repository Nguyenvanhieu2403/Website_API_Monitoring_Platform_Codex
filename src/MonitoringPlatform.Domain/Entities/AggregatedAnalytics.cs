using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringPlatform.Domain.Entities
{
    public class AggregatedAnalytics
    {
        public Guid OrganizationId { get; set; }
        public int TotalMonitors { get; set; }
        public int TotalChecks { get; set; }
        public decimal AverageUptimePercentage { get; set; }
        public int AverageResponseTime { get; set; }
        public decimal AverageFailureRate { get; set; }
    }
}
