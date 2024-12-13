using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp4.ViewModel;

namespace WpfApp4.page.usepage
{
    /// <summary>
    /// Page5.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessMonitoringPage : Page
    {
        public ProcessMonitoringPage(int tubeNumber)
        {
            InitializeComponent();
            DataContext = new ProcessMonitoringVM(tubeNumber);
        }
    }
}
