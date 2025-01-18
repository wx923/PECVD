using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using HslCommunication.ModBus;
using WpfApp.Services;
using WpfApp4.Models;

namespace WpfApp4.Services
{
    public class MotionBoatService
    {
        #region 单例模式
        private static readonly Lazy<MotionBoatService> _instance = new(() => new MotionBoatService());
        public static MotionBoatService Instance => _instance.Value;
        private MotionBoatService() 
        {
            Boats = new ObservableCollection<MotionBoatModel>();
            _modbusTcpClient = PlcCommunicationService.Instance.ModbusTcpClients[PlcCommunicationService.PlcType.Motion];
            StartDataUpdate();
        }
        #endregion

        #region 字段
        private readonly ModbusTcpNet _modbusTcpClient;
        private const int START_ADDRESS = 1000;  // 起始地址
        private const int BOAT_COUNT = 20;       // 最大舟数量
        private const int BOAT_DATA_LENGTH = 20;  // 每个舟的数据长度(预留足够空间用于扩展)
        #endregion

        #region 属性
        public ObservableCollection<MotionBoatModel> Boats { get; }
        #endregion

        #region 方法
        private void StartDataUpdate()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        // 读取所有舟的数据
                        var readResult = await _modbusTcpClient.ReadInt32Async(START_ADDRESS.ToString(), BOAT_COUNT * BOAT_DATA_LENGTH);
                        if (!readResult.IsSuccess)
                        {
                            continue;
                        }

                        var data = readResult.Content;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Boats.Clear();
                            for (int i = 0; i < BOAT_COUNT; i++)
                            {
                                var offset = i * BOAT_DATA_LENGTH;
                                var boat = new MotionBoatModel
                                {
                                    BoatNumber = data[offset],
                                    Location = data[offset + 1],
                                    Status = data[offset + 2],
                                    CurrentCoolingTime = data[offset + 3],
                                    TotalCoolingTime = data[offset + 4]
                                };

                                // 只添加有效的舟(编号不为0)
                                if (boat.BoatNumber != 0)
                                {
                                    Boats.Add(boat);
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        // 记录错误日志
                    }

                    await Task.Delay(1000); // 每秒更新一次
                }
            });
        }
        #endregion
    }
} 