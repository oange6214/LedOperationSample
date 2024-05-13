﻿using System.Windows.Controls;
using System.Windows.Input;

namespace LedOperationSample.Mvvm.Views
{
    public partial class CreateModeView : UserControl
    {
        public CreateModeView()
        {
            InitializeComponent();

            Focusable = true;
            Loaded += (s, e) => Keyboard.Focus(this);
        }
    }
}
