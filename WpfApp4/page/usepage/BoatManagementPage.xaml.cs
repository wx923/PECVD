using System.Windows.Controls;
using WpfApp4.ViewModel;
using System.Windows;
using WpfApp4.Models;
using System;
using System.Linq;

namespace WpfApp4.page.usepage
{
    public partial class BoatManagementPage : Page
    {
        private readonly BoatManagementVM viewModel;

        public BoatManagementPage()
        {
            InitializeComponent();
            viewModel = new BoatManagementVM();
            DataContext = viewModel;
        }

 
    }
} 