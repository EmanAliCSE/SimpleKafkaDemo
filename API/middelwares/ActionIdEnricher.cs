using Serilog.Core;
using Serilog.Events;

namespace API.middelwares
{
    public class ActionIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var firstProp = logEvent.Properties.FirstOrDefault();
            if (firstProp.Key != null)
            {
                logEvent.AddOrUpdateProperty(
                    propertyFactory.CreateProperty("ActionId", firstProp.Value)
                );
            }
        }
    }



}
