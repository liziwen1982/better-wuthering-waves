using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BetterWutheringWaves.Core.Simulator;

public class SearialPortQueueManager
{
    private readonly ILogger<SearialPortQueueManager> _logger = App.GetLogger<SearialPortQueueManager>();
    private readonly ConcurrentQueue<QueueItem> _queue = new ConcurrentQueue<QueueItem>();

    private static Thread _thread = null;
    private SerialPortSimulator _serialPortSimulator = new();

    private static readonly Lazy<SearialPortQueueManager> _instance = 
        new Lazy<SearialPortQueueManager>(() => new SearialPortQueueManager());
    public static SearialPortQueueManager Instance => _instance.Value;

    
    // 启动work线程
    public SearialPortQueueManager()
    {
        if (null == _thread)
        {
            _thread = new Thread(new ParameterizedThreadStart(WorkFunc));
        
            // 启动线程
            _thread.Start(this);
        
            //_thread.Join();
        }
    }
    
    ~SearialPortQueueManager()
    {
        // _thread.Interrupt();
    }

    void WorkFunc(object? o)
    {
        _logger.LogInformation("WorkFunc ...\n");
        
        while (true)
        {
            try
            {
                if (!_serialPortSimulator.GetSerialPort().IsOpen)
                {
                    bool bMatch = false;
                    foreach (var portName in SerialPort.GetPortNames())
                    {
                        if (portName == _serialPortSimulator.GetSerialPort().PortName)
                        {
                            bMatch = true;
                            break;
                        }
                    }

                    if (!bMatch)
                    {
                        _logger.LogInformation("WorkFunc SerialPort not found file {} ...\n", _serialPortSimulator.GetSerialPort().PortName);
                        Thread.Sleep(3000);
                        continue;
                    }
                    
                    _serialPortSimulator.GetSerialPort().Open();
                    _logger.LogInformation("WorkFunc SerialPort Open ...\n");
                
                    if (!_serialPortSimulator.GetSerialPort().IsOpen)
                        continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

                
            Thread.Sleep(10);
        
            QueueItem? qItem = ((SearialPortQueueManager)o!)?.Dequeue();

            if (null == qItem)
            {
                Thread.Sleep(20);
                continue;
            }
            
            if(null != qItem)
                _logger.LogInformation("RetryCount:{}", qItem?.RetryCount);

            try
            {
                byte[] CommonData = qItem.CommonData;
                _logger.LogInformation("WorkFunc SerialPort Write {} ...\n", CommonData);
                _serialPortSimulator.GetSerialPort().Write(CommonData, 0, CommonData.Length);
                if(qItem.Delay > 0)
                    Thread.Sleep(qItem.Delay);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            /*
            if (nTryTest > 0)
            {
                nTryTest--;

                try
                {
                    // a 松开
                    byte[] keysup = { 0x57, 0xAB, 0x01, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    sp.Write(keysup, 0, keysup.Length);
            
                    Thread.Sleep(1000);
                    
                    //阻塞式接受
                    if (sp.BytesToRead > 0 )
                    {
                        byte[] _readb = new byte[sp.BytesToRead];
                        int nRet = sp.Read(_readb, 0, sp.BytesToRead);
                        _logger.LogInformation("_readb 按下:{}", _readb);
                    }
            
                    // a 松开
                    keysup = new byte[] { 0x57, 0xAB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    sp.Write(keysup, 0, keysup.Length);
            
                    Thread.Sleep(1000);
                    
                    //阻塞式接受
                    if (sp.BytesToRead > 0 )
                    {
                        byte[] _readb = new byte[sp.BytesToRead];
                        int nRet = sp.Read(_readb, 0, sp.BytesToRead);
                        _logger.LogInformation("_readb 松开:{}", _readb);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            */
        }
    }
    
    // 插入
    private void Enqueue(QueueItem item)
    {
        _logger.LogInformation("Enqueue 入队:{}, _queue.Count:{}", item.CommonData, _queue.Count);
        _queue.Enqueue(item);
    }

    // 取出
    private QueueItem Dequeue()
    {
        QueueItem item = null;

        if (_queue.Count > 0)
        {
            _queue.TryDequeue(out item);
            _logger.LogInformation("Dequeue 出队:{} _queue.Count:{}", item.CommonData, _queue.Count);
        }
        
        return item;
    }

    public int KeyboadPress(byte bKey, int nFunKey = 0)
    {
        _logger.LogInformation("KeyboadPress {} ...\n", bKey);
        
        // bKey 按下
        byte[] keysup = { 0x57, 0xAB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        keysup[5] = bKey;
        
        _logger.LogInformation("KeyboadPress down {} ...\n", keysup);
        
        int delay = 0;
        int retryCount = 0;
        QueueItem item = new QueueItem(keysup, delay, retryCount);
        
        Enqueue(item);
        
        // 设备要求每条指令间隔2ms以上
        Thread.Sleep(20);
        
        // bKey 松开
        keysup = new byte []{ 0x57, 0xAB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        //sp.Write(keysup, 0, keysup.Length);
        item = new QueueItem(keysup, delay, retryCount);
        
        _logger.LogInformation("KeyboadPress up {} ...\n", keysup);
        
        Enqueue(item);
        
        
        return 0;
    }

    public int KeyboadLoogPress(int nKey, int nTime, int nFunKey = 0)
    {
        return 0;
    }
    
    public int KeyboadPresses(int nKeyOne, int nKeyTwo = 0, int nKeyThree = 0, int nKeyFour = 0, int nKeyFive = 0, int nKeySix = 0, int nFunKey = 0)
    {
        return 0;
    }
    
    public bool IsEmpty => _queue.IsEmpty;
}