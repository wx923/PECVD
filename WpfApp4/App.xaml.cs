using System.Windows;
using WpfApp4.Services;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 初始化状态监控服务
            _ = ProcessStateMonitorService.Instance;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 清理资源
            ProcessStateMonitorService.Instance.Cleanup();
            base.OnExit(e);
        }
    }
}
