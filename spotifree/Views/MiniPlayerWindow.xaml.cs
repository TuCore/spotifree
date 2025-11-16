using Spotifree.ViewModels;
using System.Windows;
using System.Windows.Input;

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
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}