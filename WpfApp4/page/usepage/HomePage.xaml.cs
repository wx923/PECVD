using System.Windows.Controls;
using WpfApp4.ViewModel;

namespace WpfApp4.page.usepage
{
    /// <summary>
    /// Page1.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            DataContext = new HomePageViewModel();
        }
    }
}
