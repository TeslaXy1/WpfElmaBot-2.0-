using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfElmaBot.Service;

namespace WpfElmaBot_2._0_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            new TelegramCore().Start();
        }

        private void MainWind_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MainBtn_Click(object sender, RoutedEventArgs e)
        {
            MainBtn.IsDefault = true;
            SettingBtn.IsDefault = false;
            ErrorBtn.IsDefault = false;
        }

        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            MainBtn.IsDefault = false;
            SettingBtn.IsDefault = true;
            ErrorBtn.IsDefault = false;
        }

        private void ErrorBtn_Click(object sender, RoutedEventArgs e)
        {
            MainBtn.IsDefault = false;
            SettingBtn.IsDefault = false;
            ErrorBtn.IsDefault = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
