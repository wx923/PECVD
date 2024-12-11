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
        }

        // 页面卸载时清理资源
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
