using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HslCommunication.ModBus;

namespace WpfApp.Services;

/// <summary>
/// PLC通信服务类，提供与PLC的通信功能
/// </summary>
public class PlcCommunicationService
{
    // 添加单例实例
    private static readonly Lazy<PlcCommunicationService> _instance = 
        new Lazy<PlcCommunicationService>(() => new PlcCommunicationService());
    
    // 公共访问点
    public static PlcCommunicationService Instance => _instance.Value;

    // 添加连接状态改变事件
    public event EventHandler<(PlcType PlcType, bool IsConnected)> ConnectionStateChanged;
    public event EventHandler<bool> AllPlcsConnectionStateChanged;

    // 定义PLC类型枚举
    public enum PlcType
    {
        Furnace1 = 0,
        Furnace2 = 1,
        Furnace3 = 2,
        Furnace4 = 3,
        Furnace5 = 4,
        Furnace6 = 5,
        Motion = 6
    }

    // 修改为公共属性
    public Dictionary<PlcType, ModbusTcpNet> ModbusTcpClients { get; private set; }
    public Dictionary<PlcType, bool> ConnectionStates { get; private set; }
    private readonly object _lock = new();

    /// <summary>
    /// 私有构造函数，确保单例模式
    /// </summary>
    private PlcCommunicationService()
    {
        ModbusTcpClients = new Dictionary<PlcType, ModbusTcpNet>();
        ConnectionStates = new Dictionary<PlcType, bool>();

        // 初始化运动控制PLC客户端
        InitializePlcClient(PlcType.Motion, "192.168.1.88");

        // 初始化6个炉管PLC客户端
        var plcIpAddresses = new[]
        {
            "192.168.1.89",  // PLC1
            "192.168.1.90",  // PLC2
            "192.168.1.91",  // PLC3
            "192.168.1.92",  // PLC4
            "192.168.1.93",  // PLC5
            "192.168.1.94"   // PLC6
        };

        for (int i = 0; i < 6; i++)
        {
            var plcType = (PlcType)i;
            InitializePlcClient(plcType, plcIpAddresses[i]);
        }

        // 自动连接所有PLC
        _ = ConnectAllPlcsAsync();
    }

    private async Task ConnectAllPlcsAsync()
    {
        try
        {
            bool allConnected = await ConnectAllAsync();
            AllPlcsConnectionStateChanged?.Invoke(this, allConnected);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"自动连接PLC失败: {ex.Message}");
            AllPlcsConnectionStateChanged?.Invoke(this, false);
        }
    }

    private void InitializePlcClient(PlcType plcType, string ipAddress)
    {
        ModbusTcpClients[plcType] = new ModbusTcpNet(ipAddress, 502)
        {
            ReceiveTimeOut = 1000,
            ConnectTimeOut = 2000
        };
        ConnectionStates[plcType] = false;
    }

    /// <summary>
    /// 获取指定PLC的ModbusTcp客户端
    /// </summary>
    public ModbusTcpNet GetModbusTcpClient(PlcType plcType)
    {
        return ModbusTcpClients[plcType];
    }

    /// <summary>
    /// 异步连接到指定的PLC
    /// </summary>
    public async Task<bool> ConnectAsync(PlcType plcType)
    {
        try
        {
            lock (_lock)
            {
                if (ConnectionStates[plcType]) return true;
            }

            var client = ModbusTcpClients[plcType];
            var result = await client.ConnectServerAsync();
            
            bool isConnected = result.IsSuccess;
            lock (_lock)
            {
                ConnectionStates[plcType] = isConnected;
            }
            
            // 触发连接状态改变事件
            ConnectionStateChanged?.Invoke(this, (plcType, isConnected));
            
            return isConnected;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PLC {plcType} 连接失败: {ex.Message}");
            ConnectionStateChanged?.Invoke(this, (plcType, false));
            return false;
        }
    }

    /// <summary>
    /// 连接所有PLC
    /// </summary>
    public async Task<bool> ConnectAllAsync()
    {
        var tasks = new List<Task<bool>>();
        foreach (PlcType plcType in Enum.GetValues(typeof(PlcType)))
        {
            tasks.Add(ConnectAsync(plcType));
        }

        var results = await Task.WhenAll(tasks);
        return results.All(x => x);
    }

    /// <summary>
    /// 断开指定PLC的连接
    /// </summary>
    public void Disconnect(PlcType plcType)
    {
        try
        {
            lock (_lock)
            {
                if (!ConnectionStates[plcType]) return;

                ModbusTcpClients[plcType].ConnectClose();
                ConnectionStates[plcType] = false;
                
                // 触发连接状态改变事件
                ConnectionStateChanged?.Invoke(this, (plcType, false));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"断开PLC {plcType} 连接失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 断开所有PLC连接
    /// </summary>
    public void DisconnectAll()
    {
        foreach (PlcType plcType in Enum.GetValues(typeof(PlcType)))
        {
            Disconnect(plcType);
        }
    }

    /// <summary>
    /// 检查指定PLC是否已连接
    /// </summary>
    public bool IsConnected(PlcType plcType)
    {
        lock (_lock)
        {
            return ConnectionStates[plcType];
        }
    }
}