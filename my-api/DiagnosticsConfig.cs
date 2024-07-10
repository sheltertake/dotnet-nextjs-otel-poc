using System.Diagnostics;
namespace MyApi;
public static class DiagnosticsConfig
{
    public const string ServiceName = "MyApi";
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);
}