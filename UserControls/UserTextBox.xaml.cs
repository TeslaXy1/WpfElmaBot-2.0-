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


namespace WpfElmaBot_2._0_.UserControls
{
    /// <summary>
    /// Логика взаимодействия для UserTextBox.xaml
    /// </summary>
    public partial class UserTextBox : UserControl
    {
        public UserTextBox()
        {
            InitializeComponent();
        }

        public Binding appendText
        {
            get { return (Binding)GetValue(AppendTextProperty); }
            set { SetValue(AppendTextProperty, value); }
        }

        #region AppendText Attached Property
        public static readonly DependencyProperty AppendTextProperty =
            DependencyProperty.Register(
                "AppendText",
                typeof(Binding),
                typeof(UserTextBox),
                new UIPropertyMetadata(null, OnAppendTextChanged));

        public static string GetAppendText(TextBox textBox)
        {
            return (string)textBox.GetValue(AppendTextProperty);
        }

        public static void SetAppendText(
            TextBox textBox,
            string value)
        {
            textBox.SetValue(AppendTextProperty, value);
        }

        private static void OnAppendTextChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
            {
                return;
            }

            string toAppend = args.NewValue.ToString();

            if (toAppend == "")
            {
                return;
            }

            TextBox textBox = d as TextBox;
            textBox?.AppendText(toAppend);
            textBox?.ScrollToEnd();
        }
        #endregion
    }
}
