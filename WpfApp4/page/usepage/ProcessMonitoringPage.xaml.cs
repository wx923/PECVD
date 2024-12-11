using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp4.page.usepage
{
    /// <summary>
    /// Page5.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessMonitoringPage : Page
    {
        public ObservableCollection<SensorData> DataCollection { get; set; }
        public ProcessMonitoringPage(int tubeNumber)
        {
            InitializeComponent();
            InitializeComponent();
            DataCollection = new ObservableCollection<SensorData>();
            myDataGrid.ItemsSource = DataCollection;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            DataCollection.Add(new SensorData
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Temperature = $"{new Random().Next(20, 30)}°C",
                Humidity = $"{new Random().Next(40, 70)}%"
            });
        }
        public class SensorData
        {
            public string Time { get; set; }
            public string Temperature { get; set; }
            public string Humidity { get; set; }
        }
    }
}
