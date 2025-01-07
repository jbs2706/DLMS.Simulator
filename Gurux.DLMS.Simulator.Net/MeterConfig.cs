namespace Gurux.DLMS.Simulator.Net;

public class MeterConfig
{
    public int? Portnumber { get; set; }
    public int ServerCount { get; set; } = 1;
    public string? Interface { get; set; }
    public string? Password { get; set; }
}