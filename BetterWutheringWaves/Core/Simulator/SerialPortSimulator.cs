using System;
using BetterWutheringWaves.Helpers;
using System.Threading;
using System.Windows;
using Vanara.PInvoke;

using BetterWutheringWaves.Core.Config.SerialCom;
using System.IO.Ports;

namespace BetterWutheringWaves.Core.Simulator;

public class SerialPortSimulator
{
    private SerialPortConfig _config;
    private SerialPort _serialPort = null;
    public SerialPortSimulator(SerialPortConfig config = null)
    {
        if (null == config)
        {
            _config = new();
        }
        else
        {
            _config = config;
        }
        
    }
    
    public SerialPort GetSerialPort(SerialPortConfig config=null)
    {
        if (config != null)
            _config = config;

        if (null == _serialPort)
        {
            _serialPort = new SerialPort
            {
                PortName = _config.PortName,
                BaudRate = _config.BaudRate,
                Parity = _config.Parity,
                DataBits = _config.DataBits,
                StopBits = _config.StopBits,
                Handshake = _config.Handshake,
                ReadTimeout = _config.ReadTimeout,
                WriteTimeout = _config.WriteTimeout
            };
        }

        return _serialPort;
    }
}
