using System.Windows.Controls;
using WpfApp4.ViewModel;

namespace WpfApp4.page.usepage
{
    public partial class ProcessManagementPage : Page
    {
        private ProcessManagementVM _viewModel;

        public ProcessManagementPage()
        {
            InitializeComponent();
            _viewModel = new ProcessManagementVM();
            DataContext = _viewModel;
        }
    }
} 