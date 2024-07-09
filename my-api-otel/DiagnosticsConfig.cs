using System.Diagnostics;

namespace MyApiOtel;
public static class DiagnosticsConfig
{
    public const string ServiceName = "MyApiOtel";
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);
}