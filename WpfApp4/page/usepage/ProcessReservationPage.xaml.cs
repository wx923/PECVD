using System.Windows.Controls;
using WpfApp4.ViewModel;

namespace WpfApp4.page.usepage
{
    /// <summary>
    /// Page7.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessReservationPage : Page
    {
        public ProcessReservationPage()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}
