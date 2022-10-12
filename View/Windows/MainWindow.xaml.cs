using NLog;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Data;
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

          
        }



        private void MainWind_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
