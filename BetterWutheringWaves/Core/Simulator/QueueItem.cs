using System;
using System.IO.Ports;

namespace BetterWutheringWaves.Core.Simulator;

public class QueueItem
{
    public byte[] CommonData { get; }
    public int Delay { get; }
    public int RetryCount { get; }

    public SerialPort SerialPort { get; set; }

    public QueueItem(byte[] commonData, int delay = 0/*ms*/, int retryCount = 2)
    {
        CommonData = commonData;
        if (delay == 0)
        {
            Random random = new Random();
            
            // apm（每分钟操作数） 7 次
            // 1000 / 7 = 142
            // 142 / 2 = 71
            Delay = random.Next(50, 100);//2-100ms都可以，也不用太精准
        }
        RetryCount = retryCount;
    }
}