using System.Windows;
using System.Windows.Controls;
using WpfApp4.ViewModel;

namespace WpfApp4.page.usepage
{
    public partial class MotionControlPage : Page
    {
        private MotionVM viewModel;

        public MotionControlPage()
        {
            InitializeComponent();
            viewModel = new MotionVM();
            DataContext = viewModel;

            // 注册页面加载事件
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时的初始化逻辑
            viewModel?.EventLogs.Add(new MotionVM.EventLog 
            { 
                Time = System.DateTime.Now, 
                Message = "页面已加载" 
            });
        }

        // 页面卸载时清理资源
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // 取消事件订阅
            Loaded -= Page_Loaded;
            
            // 清理 ViewModel 资源
            if (viewModel != null)
            {
                viewModel.EventLogs.Clear();
                viewModel = null;
            }

            // 清理数据上下文
            DataContext = null;
        }
    }
}
