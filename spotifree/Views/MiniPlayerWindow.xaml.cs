using Spotifree.ViewModels;
using System.Windows;

namespace Spotifree.Views
{
    // Interaction logic for MiniPlayerWindow.xaml
    public partial class MiniPlayerWindow : Window
    {
        public MiniPlayerWindow(PlayerViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
        public MiniPlayerWindow() : this(null!)
        {
        }
    }
}