using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace agent_framework_mcp_demo.Telemetry
{
    public static class AgentOTELExtensions
    {
        public static void ConfigureOpenTelemetry(this WebApplicationBuilder builder)
        {
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: AgentMetrics.SourceName);

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(rb => rb.AddService(serviceName: AgentMetrics.SourceName))
                .WithMetrics(metrics =>
                {
                    metrics.AddMeter(AgentMetrics.SourceName);
                    metrics.AddOtlpExporter();
                })
                .WithTracing(tracing =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        tracing.SetSampler<AlwaysOnSampler>();
                    }

                    tracing.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource(AgentMetrics.SourceName);
                    tracing.AddOtlpExporter();
                });
        }
    }
}
