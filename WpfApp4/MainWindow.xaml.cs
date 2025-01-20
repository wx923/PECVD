using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp4.page.usepage;
using WpfApp4.page.usepage;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //定时器对象
        private DispatcherTimer _timer;
        //用于按钮状态切换
        private Button LastButtonUI;
        private int CurrentTubeNumber = 1; // 当前选中的炉管号

        public MainWindow()
        {
            InitializeComponent();
            
            // 默认导航到 HomePage
            MainFrame.Navigate(new HomePage());
            LastButtonUI = BtnHome;
            LastButtonUI.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
            
            // 初始化时间显示
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // 默认选中炉管1
            BtnFurnaceTube1.IsChecked = true;
            
            // 添加炉管选择事件处理
            BtnFurnaceTube1.Checked += FurnaceTube_Checked;
            BtnFurnaceTube2.Checked += FurnaceTube_Checked;
            BtnFurnaceTube3.Checked += FurnaceTube_Checked;
            BtnFurnaceTube4.Checked += FurnaceTube_Checked;
            BtnFurnaceTube5.Checked += FurnaceTube_Checked;
            BtnFurnaceTube6.Checked += FurnaceTube_Checked;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void FurnaceTube_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                string content = radioButton.Content.ToString();
                CurrentTubeNumber = int.Parse(content.Replace("炉管", ""));
                
                // 获取当前页面并刷新
                RefreshCurrentPage();
            }
        }

        private void RefreshCurrentPage()
        {
            // 获取当前页面
            if (MainFrame.Content is Page currentPage)
            {
                // 根据当前页面类型重新导航
                if (currentPage is ParameterSettingPage)
                {
                    MainFrame.Navigate(new ParameterSettingPage(CurrentTubeNumber));
                    LastButtonUI = BtnParameterSetting;
                }
                else if (currentPage is ControlInterfacePage)
                {
                    MainFrame.Navigate(new ControlInterfacePage(CurrentTubeNumber));
                    LastButtonUI = BtnControlInterface;
                }
                else if (currentPage is GlobalMonitoringPage)
                {
                    MainFrame.Navigate(new GlobalMonitoringPage(CurrentTubeNumber));
                    LastButtonUI = BtnGlobalMonitoring;
                }
                else if (currentPage is ProcessMonitoringPage)
                {
                    MainFrame.Navigate(new ProcessMonitoringPage(CurrentTubeNumber));
                    LastButtonUI = BtnProcessMonitoring;
                }
                else if (currentPage is MonitoringAlarmPage)
                {
                    MainFrame.Navigate(new MonitoringAlarmPage(CurrentTubeNumber));
                    LastButtonUI = BtnMonitoringAlarm;
                }

                // 更新按钮样式
                if (LastButtonUI != null)
                {
                    LastButtonUI.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
                }
            }
        }

        // 检查是否选择了炉管
        private bool CheckTubeSelected()
        {
            if (!BtnFurnaceTube1.IsChecked == true && 
                !BtnFurnaceTube2.IsChecked == true && 
                !BtnFurnaceTube3.IsChecked == true && 
                !BtnFurnaceTube4.IsChecked == true && 
                !BtnFurnaceTube5.IsChecked == true && 
                !BtnFurnaceTube6.IsChecked == true)
            {
                MessageBox.Show("请先选择炉管！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                BtnFurnaceTube1.IsChecked = true; // 默认选中炉管1
                return false;
            }
            return true;
        }

        //使用页面路由跳转
        private void NavigateToParameterSettingPage(object sender, RoutedEventArgs e)
        {
            if (CheckTubeSelected())
            {
                MainFrame.Navigate(new ParameterSettingPage(CurrentTubeNumber));
                LastButtonUI.Style = (Style)FindResource("TopNavigationButtonStyle");
                LastButtonUI = BtnParameterSetting;
                BtnParameterSetting.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
            }
        }

        private void NavigateToMotionControlPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MotionControlPage());
            LastButtonUI.Style = (Style)FindResource("TopNavigationButtonStyle");
            LastButtonUI = BtnMotionControl;
            BtnMotionControl.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
        }

        private void NavigateToControlInterfacePage(object sender, RoutedEventArgs e)
        {
            if (CheckTubeSelected())
            {
                MainFrame.Navigate(new ControlInterfacePage(CurrentTubeNumber));
                LastButtonUI.Style = (Style)FindResource("TopNavigationButtonStyle");
                LastButtonUI = BtnControlInterface;
                BtnControlInterface.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
            }
        }

        private void NavigateToGlobalMonitoringPage(object sender, RoutedEventArgs e)
        {
            if (CheckTubeSelected())
            {
                MainFrame.Navigate(new GlobalMonitoringPage(CurrentTubeNumber));
                LastButtonUI.Style = (Style)FindResource("TopNavigationButtonStyle");
                LastButtonUI = BtnGlobalMonitoring;
                BtnGlobalMonitoring.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
            }
        }

        private void NavigateToProcessMonitoringPage(object sender, RoutedEventArgs e)
        {
            if (CheckTubeSelected())
            {
                MainFrame.Navigate(new ProcessMonitoringPage(CurrentTubeNumber));
                LastButtonUI.Style = (Style)FindResource("TopNavigationButtonStyle");
                LastButtonUI = BtnProcessMonitoring;
                BtnProcessMonitoring.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
            }
        }

        private void NavigateToMonitoringAlarmPage(object sender, RoutedEventArgs e)
        {
            if (CheckTubeSelected())
            {
                MainFrame.Navigate(new MonitoringAlarmPage(CurrentTubeNumber));
                LastButtonUI.Style = (Style)FindResource("TopNavigationButtonStyle");
                LastButtonUI = BtnMonitoringAlarm;
                BtnMonitoringAlarm.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
            }
        }

        private void NavigateToHomePage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HomePage());
            LastButtonUI.Style = (Style)FindResource("TopNavigationButtonStyle");
            LastButtonUI = BtnHome;
            BtnHome.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
        }


        private void NavigateToBoatManagementPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BoatManagementPage());
            LastButtonUI.Style = (Style)FindResource("TopNavigationButtonStyle");
            LastButtonUI = BtnBoatManagement;
            BtnBoatManagement.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
        }

        private void NavigateToProcessManagementPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProcessManagementPage());
            LastButtonUI.Style = (Style)FindResource("TopNavigationButtonStyle");
            LastButtonUI = BtnProcessManagement;
            BtnProcessManagement.Style = (Style)FindResource("TopNavigationSelectedButtonStyle");
        }
    }
}