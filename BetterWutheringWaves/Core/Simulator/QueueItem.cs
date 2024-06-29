using System.IO.Ports;

namespace BetterWutheringWaves.Core.Simulator;

public class QueueItem
{
    public byte[] CommonData { get; }
    public int Delay { get; }
    public int RetryCount { get; }

    public SerialPort SerialPort { get; set; }

    public QueueItem(byte[] commonData, int retryCount = 2, int delay = 20/*ms*/)
    {
        CommonData = commonData;
        Delay = delay;
        RetryCount = retryCount;
    }
}