﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemsRepeaterTestApp.Samples.Selection
{
    class GroupedRepeaterItem : ContentControl
    {
        public GroupedRepeaterItem()
        {
            IsTabStop = true;
            SetResourceReference(FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            Margin = new Thickness(3);
        }

        public SelectionModel SelectionModel
        {
            get { return (SelectionModel)GetValue(SelectionModelProperty); }
            set { SetValue(SelectionModelProperty, value); }
        }

        public static readonly DependencyProperty SelectionModelProperty =
            DependencyProperty.Register("SelectionModel", typeof(SelectionModel), typeof(GroupedRepeaterItem), new PropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        public int RepeatedIndex
        {
            get { return (int)GetValue(RepeatedIndexProperty); }
            set { SetValue(RepeatedIndexProperty, value); }
        }

        public static readonly DependencyProperty RepeatedIndexProperty =
            DependencyProperty.Register("RepeatedIndex", typeof(int), typeof(GroupedRepeaterItem), new PropertyMetadata(0, new PropertyChangedCallback(OnPropertyChanged)));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(GroupedRepeaterItem), new PropertyMetadata(false));

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.Property == RepeatedIndexProperty)
            {
                var item = obj as GroupedRepeaterItem;
                if (item.SelectionModel != null)
                {
                    int groupIndex = item.GetGroupIndex();
                    item.IsSelected = groupIndex >= 0 ? item.SelectionModel.IsSelected(groupIndex, item.RepeatedIndex).Value : false;
                }
            }
            else if (args.Property == SelectionModelProperty)
            {
                if (args.OldValue != null)
                {
                    (args.OldValue as SelectionModel).PropertyChanged -= (obj as GroupedRepeaterItem).OnselectionModelChanged;
                }

                if (args.NewValue != null)
                {
                    (args.NewValue as SelectionModel).PropertyChanged += (obj as GroupedRepeaterItem).OnselectionModelChanged;
                }
            }
        }

        public void Select()
        {
            SelectionModel.Select(GetGroupIndex(), RepeatedIndex);
        }

        public void Deselect()
        {
            SelectionModel.Deselect(GetGroupIndex(), RepeatedIndex);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (SelectionModel != null)
            {
                if (e.Key == Key.Escape)
                {
                    SelectionModel.ClearSelection();
                }
                else if (e.Key == Key.Space)
                {
                    Select();
                }
                else if (!SelectionModel.SingleSelect)
                {
                    var isShiftPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
                    var isCtrlPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
                    if (e.Key == Key.A && isCtrlPressed)
                    {
                        SelectionModel.SelectAll();
                    }
                    else if (isShiftPressed)
                    {
                        SelectionModel.SelectRangeFromAnchor(GetGroupIndex(), RepeatedIndex);
                    }
                }
            }

            base.OnKeyUp(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (SelectionModel != null)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && !SelectionModel.SingleSelect)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        SelectionModel.DeselectRangeFromAnchor(GetGroupIndex(), RepeatedIndex);
                    }
                    else
                    {
                        SelectionModel.SelectRangeFromAnchor(GetGroupIndex(), RepeatedIndex);
                    }
                }
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    var groupIndex = GetGroupIndex();
                    if (SelectionModel.IsSelected(groupIndex, RepeatedIndex).Value)
                    {
                        Deselect();
                    }
                    else
                    {
                        Select();
                    }
                }
                else
                {
                    Select();
                    this.Focus();
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GroupedRepeaterItemAutomationPeer(this);
        }

        private int GetGroupIndex()
        {
            var child = this.Parent as FrameworkElement;
            var parent = child.Parent as FrameworkElement;

            while (!(parent is ItemsRepeater))
            {
                child = parent;
                parent = parent.Parent as FrameworkElement;
            }

            var groupIndex = (parent as ItemsRepeater).GetElementIndex(child);

            return groupIndex;
        }

        private void OnselectionModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedIndices")
            {
                bool oldValue = IsSelected;
                var groupIndex = GetGroupIndex();
                bool newValue = groupIndex >= 0 ? SelectionModel.IsSelected(groupIndex, RepeatedIndex).Value : false;

                if (oldValue != newValue)
                {
                    IsSelected = newValue;

                    // AutomationEvents.PropertyChanged is used as a value that means dont raise anything 
                    AutomationEvents eventToRaise =
                        oldValue ?
                            (SelectionModel.SingleSelect ? AutomationEvents.PropertyChanged : AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection) :
                            (SelectionModel.SingleSelect ? AutomationEvents.SelectionItemPatternOnElementSelected : AutomationEvents.SelectionItemPatternOnElementAddedToSelection);

                    if (eventToRaise != AutomationEvents.PropertyChanged && AutomationPeer.ListenerExists(eventToRaise))
                    {
                        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(this);
                        peer.RaiseAutomationEvent(eventToRaise);
                    }
                }
            }
        }
    }
}
