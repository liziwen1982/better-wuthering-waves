namespace BetterWutheringWaves.Core.Config.SerialCom;

using System.IO.Ports;

public class SerialPortConfig
{
    public string PortName { get; set; }
    public int BaudRate { get; set; }
    public Parity Parity { get; set; }
    public int DataBits { get; set; }
    public StopBits StopBits { get; set; }
    public Handshake Handshake { get; set; }
    public int ReadTimeout { get; set; }
    public int WriteTimeout { get; set; }

    public SerialPortConfig()
    {
        // Set default values
        PortName = "COM3";
        BaudRate = 57600;
        Parity = Parity.None;
        DataBits = 8;
        StopBits = StopBits.One;
        Handshake = Handshake.None;
        ReadTimeout = 500;
        WriteTimeout = 500;
    }
}