﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfElmaBot_2._0_.ViewModels.Base;

namespace WpfElmaBot_2._0_.ViewModels
{
    internal  class TextBoxApppendBehaviors 

    {
        #region AppendText Attached Property
        public static readonly DependencyProperty AppendTextProperty =
            DependencyProperty.RegisterAttached(
                "AppendText",
                typeof(string),
                typeof(TextBoxApppendBehaviors),
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
