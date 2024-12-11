using System;
using System.Threading.Tasks;
using HslCommunication.ModBus;


namespace WpfApp.Services;

/// <summary>
/// PLC通信服务类，提供与PLC的通信功能
/// </summary>
public class PlcCommunicationService
{
    public readonly ModbusTcpNet _modbusTcp;
    private bool _isConnected;
    private readonly object _lock = new();


    /// <summary>
    /// 初始化PLC通信服务
    /// </summary>
    /// <param name="ipAddress">PLC的IP地址</param>
    /// <param name="port">PLC的端口号</param>
    public PlcCommunicationService(string ipAddress = "127.0.0.1", int port = 502)
    {
        _modbusTcp = new ModbusTcpNet(ipAddress, port)
        {
            ReceiveTimeOut = 1000,  // 接收超时时间（毫秒）
            ConnectTimeOut = 2000   // 连接超时时间（毫秒）
        };

    }

    /// <summary>
    /// 异步连接到PLC
    /// </summary>
    /// <returns>连接是否成功</returns>
    public async Task<bool> ConnectAsync()
    {
        try
        {
            lock (_lock)
            {
                if (_isConnected) return true;
            }

            var result = await _modbusTcp.ConnectServerAsync();
            _isConnected = result.IsSuccess;
            return _isConnected;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// 断开与PLC的连接
    /// </summary>
    public void Disconnect()
    {
        try
        {
            lock (_lock)
            {
                if (!_isConnected) return;

                _modbusTcp.ConnectClose();
                _isConnected = false;
            }
        }
        catch (Exception ex)
        {
        }
    }

    /// <summary>
    /// 异步读取16位整数
    /// </summary>
    /// <param name="address">PLC地址（如"D0"）</param>
    /// <returns>读取的值，失败返回null</returns>
    public async Task<short?> ReadInt16Async(string address)
    {
        try
        {
            var result = await _modbusTcp.ReadInt16Async(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            return null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}