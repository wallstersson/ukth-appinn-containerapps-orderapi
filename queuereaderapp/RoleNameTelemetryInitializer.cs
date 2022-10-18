using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace QueueReader
{
    public class RoleNameTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _name;

        public RoleNameTelemetryInitializer(string name)
        {
            _name = name;
        }
    
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = _name;
        }
    }
}