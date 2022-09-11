using System;
using System.Windows;
using System.Windows.Input;
using WpfElmaBot.Service;
using WpfElmaBot_2._0_.Service;
using WpfElmaBot_2._0_.View.Windows;
using WpfElmaBot_2._0_.ViewModels;
using WpfElmaBot_2._0_.ViewModels.Base;

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
            ElmaMessages.Start();
            new MainWindowViewModel().Status = "Бот запущен";
            //new MainWindowViewModel().Status =  $"{DateTime.UtcNow.ToString("g")}-Бот запущен";



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
            new SettingPage().ShowDialog();
           
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
