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
        
        //Random random = new Random();
        
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
        
            QueueItem? qItem = ((SearialPortQueueManager)o!)?.Dequeue();

            if (null == qItem)
            {
                Thread.Sleep(2); // 避免空转时cpu过高
                continue;
            }
            
            //if(null != qItem)
            //    _logger.LogInformation("RetryCount:{}", qItem?.RetryCount);

            try
            {
                byte[] CommonData = qItem.CommonData;
                _logger.LogInformation("{}-{} - WorkFunc SerialPort Write {} ...\n", DateTime.Now, DateTime.Now.Millisecond, CommonData);
                _serialPortSimulator.GetSerialPort().Write(CommonData, 0, CommonData.Length);

                
                //阻塞式接受
                if (_serialPortSimulator.GetSerialPort().BytesToRead > 0 )
                {
                    byte[] _readb = new byte[_serialPortSimulator.GetSerialPort().BytesToRead];
                    int nRet = _serialPortSimulator.GetSerialPort().Read(_readb, 0, _serialPortSimulator.GetSerialPort().BytesToRead);
                    _logger.LogInformation("_readb {}", _readb);
                }
                
                //Thread.Sleep(random.Next(50, 101)); 
                //Thread.Sleep(500);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            if(qItem.Delay > 0)
                Thread.Sleep(qItem.Delay);
        }
    }
    
    // 插入
    private void Enqueue(QueueItem item)
    {
        //_logger.LogInformation("Enqueue 入队:{}, _queue.Count:{}", item.CommonData, _queue.Count);

        // 队列为空时，发送一个松开指令
        if (_queue.IsEmpty)
        {
            byte[] keysup = { 0x57, 0xAB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            QueueItem _item = new QueueItem(keysup);
            _queue.Enqueue(_item);
            _queue.Enqueue(_item);
        }
        _queue.Enqueue(item);
    }

    // 取出
    private QueueItem Dequeue()
    {
        QueueItem item = null;

        if (_queue.Count > 0)
        {
            _queue.TryDequeue(out item);
            //_logger.LogInformation("{}, Dequeue 出队:{} _queue.Count:{}", DateTime.Now,  item.CommonData, _queue.Count);
        }
        
        return item;
    }

    public int KeyboadPress(byte bKey, int nFunKey = 0)
    {
        //_logger.LogInformation("KeyboadPress {} ...\n", bKey);
        
        // bKey 按下
        byte[] keysup = { 0x57, 0xAB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        keysup[5] = bKey;
        
        //_logger.LogInformation("KeyboadPress down {} ...\n", keysup);
        
        int delay = 0;
        int retryCount = 0;
        //delay = 10;
        QueueItem item = new QueueItem(keysup, delay, retryCount);
        
        Enqueue(item);
        
        // 设备要求每条指令间隔2ms以上
        //Thread.Sleep(10);
        
        // bKey 松开
        keysup = new byte []{ 0x57, 0xAB, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        //sp.Write(keysup, 0, keysup.Length);
        //delay = 0;
        item = new QueueItem(keysup, delay, retryCount);
        
        //_logger.LogInformation("KeyboadPress up {} ...\n", keysup);
        
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