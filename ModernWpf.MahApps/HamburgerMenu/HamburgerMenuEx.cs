﻿using MahApps.Metro.Controls;
using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ModernWpf.MahApps.Controls
{
    /// <summary>
    /// Represents a container that enables navigation of app content. It has a header,
    /// a view for the main content, and a menu pane for navigation commands.
    /// </summary>
    [TemplatePart(Name = c_navViewBackButton, Type = typeof(Button))]
    public class HamburgerMenuEx : HamburgerMenu
    {
        private const string c_searchButtonName = "PaneAutoSuggestButton";
        private const string c_navViewBackButton = "NavigationViewBackButton";

        private static readonly Point c_frame1point1 = new Point(0.9, 0.1);
        private static readonly Point c_frame1point2 = new Point(1.0, 0.2);
        private static readonly Point c_frame2point1 = new Point(0.1, 0.9);
        private static readonly Point c_frame2point2 = new Point(0.2, 1.0);

        private UIElement _paneGrid;
        private Button _paneSearchButton;
        private Button _backButton;
        private ListBox _buttonsListView;
        private ListBox _optionsListView;

        private UIElement _prevIndicator;
        private UIElement _nextIndicator;

        static HamburgerMenuEx()
        {
            DisplayModeProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnDisplayModePropertyChanged));
            IsPaneOpenProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnIsPaneOpenPropertyChanged));
            CompactPaneLengthProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnPaneLengthPropertyChanged));
            OpenPaneLengthProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnPaneLengthPropertyChanged));
            SelectedItemProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnSelectedItemPropertyChanged));
            SelectedOptionsItemProperty.OverrideMetadata(typeof(HamburgerMenuEx), new FrameworkPropertyMetadata(OnSelectedItemPropertyChanged));
        }

        /// <summary>
        /// Initializes a new instance of the HamburgerMenuEx class.
        /// </summary>
        public HamburgerMenuEx()
        {
            DefaultStyleKey = typeof(HamburgerMenuEx);
            SetResourceReference(DefaultItemFocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            SetResourceReference(ClientAreaAnimationProperty, SystemParameters.ClientAreaAnimationKey);
        }

        #region IsBackButtonVisible

        /// <summary>
        /// Identifies the IsBackButtonVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsBackButtonVisible),
                typeof(bool),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the back button is visible or collapsed.
        /// </summary>
        /// <returns>true if the back button is visible; otherwise, false. The default is false.</returns>
        public bool IsBackButtonVisible
        {
            get => (bool)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        #endregion

        #region IsBackEnabled

        /// <summary>
        /// Identifies the IsBackEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.Register(
                nameof(IsBackEnabled),
                typeof(bool),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the back button is enabled or disabled.
        /// </summary>
        /// <returns>true if the back button is enabled; otherwise, false. The default is true.</returns>
        public bool IsBackEnabled
        {
            get => (bool)GetValue(IsBackEnabledProperty);
            set => SetValue(IsBackEnabledProperty, value);
        }

        #endregion

        #region BackButtonCommand

        public static readonly DependencyProperty BackButtonCommandProperty =
            DependencyProperty.Register(
                nameof(BackButtonCommand),
                typeof(ICommand),
                typeof(HamburgerMenuEx),
                null);

        public ICommand BackButtonCommand
        {
            get => (ICommand)GetValue(BackButtonCommandProperty);
            set => SetValue(BackButtonCommandProperty, value);
        }

        #endregion

        #region BackButtonCommandParameter

        public static readonly DependencyProperty BackButtonCommandParameterProperty =
            DependencyProperty.Register(
                nameof(BackButtonCommandParameter),
                typeof(object),
                typeof(HamburgerMenuEx),
                null);

        public object BackButtonCommandParameter
        {
            get => GetValue(BackButtonCommandParameterProperty);
            set => SetValue(BackButtonCommandParameterProperty, value);
        }

        #endregion

        #region PaneLength

        private static readonly DependencyPropertyKey PaneLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(PaneLength),
                typeof(double),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty PaneLengthProperty =
            PaneLengthPropertyKey.DependencyProperty;

        public double PaneLength
        {
            get => (double)GetValue(PaneLengthProperty);
            private set => SetValue(PaneLengthPropertyKey, value);
        }

        private void UpdatePaneLength()
        {
            PaneLength = IsPaneOpen ? OpenPaneLength : CompactPaneLength;
        }

        #endregion

        #region Header

        /// <summary>
        /// Identifies the Header dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(HamburgerMenuEx));

        /// <summary>
        /// Gets or sets the header content.
        /// </summary>
        /// <returns>The header content.</returns>
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region HeaderTemplate

        /// <summary>
        /// Identifies the HeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                nameof(HeaderTemplate),
                typeof(DataTemplate),
                typeof(HamburgerMenuEx));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the control's header.
        /// </summary>
        /// <returns>The DataTemplate used to display the control's header.</returns>
        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        #endregion

        #region DefaultItemFocusVisualStyle

        private static readonly DependencyProperty DefaultItemFocusVisualStyleProperty =
            DependencyProperty.Register(
                nameof(DefaultItemFocusVisualStyle),
                typeof(Style),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(OnDefaultItemFocusVisualStyleChanged));

        private Style DefaultItemFocusVisualStyle
        {
            get => (Style)GetValue(DefaultItemFocusVisualStyleProperty);
            set => SetValue(DefaultItemFocusVisualStyleProperty, value);
        }

        private static void OnDefaultItemFocusVisualStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).ChangeItemFocusVisualStyle();
        }

        #endregion

        #region PaneTitle

        /// <summary>
        /// Identifies the PaneTitle dependency property.
        /// </summary>
        public static readonly DependencyProperty PaneTitleProperty =
            DependencyProperty.Register(
                nameof(PaneTitle),
                typeof(string),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(OnPaneTitleChanged));

        /// <summary>
        /// Gets or sets the label adjacent to the menu icon when the pane is open.
        /// </summary>
        /// <returns>
        /// The label adjacent to the menu icon when the pane is open. The default is an
        /// empty string.
        /// </returns>
        public string PaneTitle
        {
            get => (string)GetValue(PaneTitleProperty);
            set => SetValue(PaneTitleProperty, value);
        }

        private static void OnPaneTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).OnPaneTitleChanged(e);
        }

        private void OnPaneTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            HasPaneTitle = !string.IsNullOrEmpty((string)e.NewValue);
        }

        #endregion

        #region HasPaneTitle

        private static readonly DependencyPropertyKey HasPaneTitlePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(HasPaneTitle),
                typeof(bool),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasPaneTitleProperty =
            HasPaneTitlePropertyKey.DependencyProperty;

        public bool HasPaneTitle
        {
            get => (bool)GetValue(HasPaneTitleProperty);
            private set => SetValue(HasPaneTitlePropertyKey, value);
        }

        #endregion

        #region AutoSuggestBox

        /// <summary>
        /// Identifies the AutoSuggestBox dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoSuggestBoxProperty =
            DependencyProperty.Register(
                nameof(AutoSuggestBox),
                typeof(AutoSuggestBox),
                typeof(HamburgerMenuEx));

        /// <summary>
        /// Gets or sets an AutoSuggestBox to be displayed in the HamburgerMenuEx.
        /// </summary>
        /// <returns>An AutoSuggestBox box to be displayed in the HamburgerMenuEx.</returns>
        public AutoSuggestBox AutoSuggestBox
        {
            get => (AutoSuggestBox)GetValue(AutoSuggestBoxProperty);
            set => SetValue(AutoSuggestBoxProperty, value);
        }

        #endregion

        #region SelectedMenuItem

        private static readonly DependencyProperty SelectedMenuItemProperty =
            DependencyProperty.Register(
                nameof(SelectedMenuItem),
                typeof(object),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(OnSelectedMenuItemChanged));

        private object SelectedMenuItem
        {
            get => GetValue(SelectedMenuItemProperty);
            set => SetValue(SelectedMenuItemProperty, value);
        }

        private static void OnSelectedMenuItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).OnSelectedMenuItemChanged(e.OldValue, e.NewValue);
        }

        private void OnSelectedMenuItemChanged(object oldValue, object newValue)
        {
            SelectionChanged?.Invoke(this, new HamburgerMenuSelectionChangedEventArgs { SelectedItem = newValue });

            AnimateSelectionChanged(oldValue, newValue);
        }

        private void UpdateSelectedMenuItem()
        {
            SelectedMenuItem = SelectedItem ?? SelectedOptionsItem;
        }

        #endregion

        #region ClientAreaAnimation

        private static readonly DependencyProperty ClientAreaAnimationProperty =
            DependencyProperty.Register(
                nameof(ClientAreaAnimation),
                typeof(bool),
                typeof(HamburgerMenuEx),
                new PropertyMetadata(SystemParameters.ClientAreaAnimation));

        private bool ClientAreaAnimation
        {
            get => (bool)GetValue(ClientAreaAnimationProperty);
            set => SetValue(ClientAreaAnimationProperty, value);
        }

        #endregion

        /// <summary>
        /// Occurs when the back button receives an interaction such as a click or tap.
        /// </summary>
        public event EventHandler<HamburgerMenuBackRequestedEventArgs> BackRequested;

        /// <summary>
        /// Occurs when the DisplayMode property changes.
        /// </summary>
        public event EventHandler<HamburgerMenuDisplayModeChangedEventArgs> DisplayModeChanged;

        /// <summary>
        /// Occurs when the pane is opened.
        /// </summary>
        public event EventHandler PaneOpened;

        /// <summary>
        /// Occurs when the pane is closed.
        /// </summary>
        public event EventHandler PaneClosed;

        /// <summary>
        /// Occurs when the currently selected item changes.
        /// </summary>
        public event EventHandler<HamburgerMenuSelectionChangedEventArgs> SelectionChanged;

        /// <summary>
        /// Called when the Template's tree has been generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_paneSearchButton != null)
            {
                _paneSearchButton.Click -= OnPaneSearchButtonClick;
            }

            if (_backButton != null)
            {
                _backButton.Click -= OnBackButtonClicked;
            }

            if (_buttonsListView != null && _optionsListView != null)
            {
                _buttonsListView.ItemContainerGenerator.StatusChanged -= OnListViewItemContainerGeneratorStatusChanged;
                _optionsListView.ItemContainerGenerator.StatusChanged -= OnListViewItemContainerGeneratorStatusChanged;
            }

            base.OnApplyTemplate();

            _paneGrid = GetTemplateChild("PaneGrid") as UIElement;
            _paneSearchButton = GetTemplateChild(c_searchButtonName) as Button;
            _backButton = GetTemplateChild(c_navViewBackButton) as Button;
            _buttonsListView = GetTemplateChild("ButtonsListView") as ListBox;
            _optionsListView = GetTemplateChild("OptionsListView") as ListBox;

            if (_paneSearchButton != null)
            {
                _paneSearchButton.Click += OnPaneSearchButtonClick;
            }

            if (_backButton != null)
            {
                _backButton.Click += OnBackButtonClicked;
            }

            if (_buttonsListView != null && _optionsListView != null)
            {
                _buttonsListView.ItemContainerGenerator.StatusChanged += OnListViewItemContainerGeneratorStatusChanged;
                _optionsListView.ItemContainerGenerator.StatusChanged += OnListViewItemContainerGeneratorStatusChanged;
            }

            ChangeItemFocusVisualStyle();
        }

        private static void OnDisplayModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).OnDisplayModeChanged(e);
        }

        private static void OnIsPaneOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).OnIsPaneOpenChanged(e);
        }

        private static void OnPaneLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).OnPaneLengthPropertyChanged(e);
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenuEx)d).UpdateSelectedMenuItem();
        }

        private void OnPaneSearchButtonClick(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(IsPaneOpenProperty, true);

            AutoSuggestBox?.Focus();
        }

        private void OnBackButtonClicked(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, new HamburgerMenuBackRequestedEventArgs());
        }

        private void OnListViewItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if ((_buttonsListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated || !_buttonsListView.HasItems) &&
                (_optionsListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated || !_optionsListView.HasItems))
            {
                _buttonsListView.ItemContainerGenerator.StatusChanged -= OnListViewItemContainerGeneratorStatusChanged;
                _optionsListView.ItemContainerGenerator.StatusChanged -= OnListViewItemContainerGeneratorStatusChanged;

                var item = SelectedItem;
                if (item != null)
                {
                    AnimateSelectionChanged(null /* prevItem */, item);
                }
            }
        }

        private void OnDisplayModeChanged(DependencyPropertyChangedEventArgs e)
        {
            DisplayModeChanged?.Invoke(this, new HamburgerMenuDisplayModeChangedEventArgs((SplitViewDisplayMode)e.NewValue));
        }

        private void OnIsPaneOpenChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdatePaneLength();
            ChangeItemFocusVisualStyle();

            if ((bool)e.NewValue)
            {
                PaneOpened?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PaneClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnPaneLengthPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdatePaneLength();
            ChangeItemFocusVisualStyle();
        }

        private void ChangeItemFocusVisualStyle()
        {
            if (DefaultItemFocusVisualStyle != null)
            {
                var focusVisualStyle = new Style(typeof(Control), DefaultItemFocusVisualStyle);
                focusVisualStyle.Setters.Add(new Setter(Control.WidthProperty, IsPaneOpen ? OpenPaneLength : CompactPaneLength));
                focusVisualStyle.Setters.Add(new Setter(Control.HorizontalAlignmentProperty, HorizontalAlignment.Left));
                focusVisualStyle.Seal();

                SetValue(ItemFocusVisualStylePropertyKey, focusVisualStyle);
            }
        }

        private void AnimateSelectionChanged(object prevItem, object nextItem)
        {
            UIElement prevIndicator = FindSelectionIndicator(prevItem);
            UIElement nextIndicator = FindSelectionIndicator(nextItem);

            bool haveValidAnimation = false;
            // It's possible that AnimateSelectionChanged is called multiple times before the first animation is complete.
            // To have better user experience, if the selected target is the same, keep the first animation
            // If the selected target is not the same, abort the first animation and launch another animation.
            if (_prevIndicator != null || _nextIndicator != null) // There is ongoing animation
            {
                if (nextIndicator != null && _nextIndicator == nextIndicator) // animate to the same target, just wait for animation complete
                {
                    if (prevIndicator != null && prevIndicator != _prevIndicator)
                    {
                        ResetElementAnimationProperties(prevIndicator, 0.0);
                    }
                    haveValidAnimation = true;
                }
                else
                {
                    // If the last animation is still playing, force it to complete.
                    OnAnimationComplete();
                }
            }

            if (!haveValidAnimation)
            {
                UIElement paneContentGrid = _paneGrid;

                if ((prevItem != nextItem) && paneContentGrid != null && prevIndicator != null && nextIndicator != null && ClientAreaAnimation && RenderCapability.Tier > 0)
                {
                    // Make sure both indicators are visible and in their original locations
                    ResetElementAnimationProperties(prevIndicator, 1.0);
                    ResetElementAnimationProperties(nextIndicator, 1.0);

                    // get the item positions in the pane
                    Point point = new Point(0, 0);
                    double prevPos;
                    double nextPos;

                    Point prevPosPoint = prevIndicator.TransformToVisual(paneContentGrid).Transform(point);
                    Point nextPosPoint = nextIndicator.TransformToVisual(paneContentGrid).Transform(point);

                    prevPos = prevPosPoint.Y;
                    nextPos = nextPosPoint.Y;

                    // Play the animation on both the previous and next indicators
                    PlayIndicatorAnimations(prevIndicator, 0, nextPos - prevPos, true);
                    PlayIndicatorAnimations(nextIndicator, prevPos - nextPos, 0, false);

                    _prevIndicator = prevIndicator;
                    _nextIndicator = nextIndicator;
                }
                else
                {
                    // if all else fails, or if animations are turned off, attempt to correctly set the positions and opacities of the indicators.
                    ResetElementAnimationProperties(prevIndicator, 0.0);
                    ResetElementAnimationProperties(nextIndicator, 1.0);
                }
            }
        }

        private void PlayIndicatorAnimations(UIElement indicator, double from, double to, bool isOutgoing)
        {
            Size size = indicator.RenderSize;
            double dimension = size.Height;

            double beginScale = 1.0;
            double endScale = 1.0;

            if (isOutgoing)
            {
                // fade the outgoing indicator so it looks nice when animating over the scroll area
                var opacityAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1.0, KeyTime.FromPercent(0.0)),
                        new DiscreteDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0 / 3)),
                        new SplineDoubleKeyFrame(0.0, KeyTime.FromPercent(1.0), new KeySpline(c_frame2point1, c_frame2point2)),
                    },
                    Duration = TimeSpan.FromMilliseconds(600)
                };

                indicator.BeginAnimation(OpacityProperty, opacityAnim);
            }

            var posAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(from < to ? from : (from + (dimension * (beginScale - 1))), KeyTime.FromPercent(0.0)),
                    new DiscreteDoubleKeyFrame(from < to ? (to + (dimension * (endScale - 1))) : to, KeyTime.FromPercent(0.333)),
                },
                Duration = TimeSpan.FromMilliseconds(600)
            };

            var scaleAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(beginScale, KeyTime.FromPercent(0.0)),
                    new SplineDoubleKeyFrame(
                        Math.Abs(to - from) / dimension + (from < to ? endScale : beginScale),
                        KeyTime.FromPercent(1.0 / 3),
                        new KeySpline(c_frame1point1, c_frame1point2)),
                    new SplineDoubleKeyFrame(endScale, KeyTime.FromPercent(1.0), new KeySpline(c_frame2point1, c_frame2point2)),
                },
                Duration = TimeSpan.FromMilliseconds(600)
            };

            var centerAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(from < to ? 0.0 : dimension, KeyTime.FromPercent(0.0)),
                    new DiscreteDoubleKeyFrame(from < to ? dimension : 0.0, KeyTime.FromPercent(1.0)),
                },
                Duration = TimeSpan.FromMilliseconds(200)
            };

            var translateTransform = new TranslateTransform();
            var scaleTransform = new ScaleTransform();
            indicator.RenderTransform = new TransformGroup { Children = { scaleTransform, translateTransform } };

            scaleAnim.Completed += delegate
            {
                OnAnimationComplete();
            };

            translateTransform.BeginAnimation(TranslateTransform.YProperty, posAnim);
            scaleTransform.BeginAnimation(ScaleTransform.CenterYProperty, centerAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void OnAnimationComplete()
        {
            var indicator = _prevIndicator;
            ResetElementAnimationProperties(indicator, 0.0);
            _prevIndicator = null;

            indicator = _nextIndicator;
            ResetElementAnimationProperties(indicator, 1.0);
            _nextIndicator = null;
        }

        private void ResetElementAnimationProperties(UIElement element, double desiredOpacity)
        {
            if (element != null)
            {
                element.Opacity = desiredOpacity;
                element.RenderTransform = null;
                element.BeginAnimation(OpacityProperty, null);
            }
        }

        private UIElement FindSelectionIndicator(object item)
        {
            if (item != null)
            {
                var container = _buttonsListView?.ItemContainerGenerator.ContainerFromItem(item) ??
                                _optionsListView?.ItemContainerGenerator.ContainerFromItem(item);
                if (container is Control control)
                {
                    return control.Template?.FindName("SelectionIndicator", control) as UIElement;
                }
            }

            return null;
        }
    }
}
