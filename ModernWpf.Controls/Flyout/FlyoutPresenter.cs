﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class FlyoutPresenter : ContentControl
    {
        static FlyoutPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlyoutPresenter), new FrameworkPropertyMetadata(typeof(FlyoutPresenter)));
        }

        public FlyoutPresenter()
        {
            IsVisibleChanged += OnIsVisibleChanged;
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(FlyoutPresenter));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region IsDefaultShadowEnabled

        public static readonly DependencyProperty IsDefaultShadowEnabledProperty =
            DependencyProperty.Register(
                nameof(IsDefaultShadowEnabled),
                typeof(bool),
                typeof(FlyoutPresenter),
                new PropertyMetadata(true));

        public bool IsDefaultShadowEnabled
        {
            get => (bool)GetValue(IsDefaultShadowEnabledProperty);
            set => SetValue(IsDefaultShadowEnabledProperty, value);
        }

        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                if (Parent is Popup popup && popup.IsOpen)
                {
                    popup.SetCurrentValue(Popup.IsOpenProperty, false);
                    e.Handled = true;
                }
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Focus();
            }
        }
    }
}
