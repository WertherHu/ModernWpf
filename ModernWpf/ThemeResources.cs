﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf
{
    public class ThemeResources : ResourceDictionaryEx, ISupportInitialize
    {
        private static ThemeResources _current;

        private ResourceDictionary _lightResources;
        private ResourceDictionary _darkResources;
        private ResourceDictionary _highContrastResources;

        public ThemeResources()
        {
            if (Current == null)
            {
                Current = this;
            }
        }

        internal static ThemeResources Current
        {
            get => _current;
            set
            {
                if (_current != null)
                {
                    throw new InvalidOperationException(nameof(Current) + " cannot be changed after it's set.");
                }

                _current = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines the light-dark preference for the overall
        /// theme of an app.
        /// </summary>
        /// <returns>
        /// A value of the enumeration. The initial value is the default theme set by the
        /// user in Windows settings.
        /// </returns>
        public ApplicationTheme? RequestedTheme
        {
            get => ThemeManager.Current.ApplicationTheme;
            set
            {
                if (ThemeManager.Current.ApplicationTheme != value)
                {
                    ThemeManager.Current.SetCurrentValue(ThemeManager.ApplicationThemeProperty, value);

                    if (!IsInitializePending && DesignMode.DesignModeEnabled)
                    {
                        UpdateDesignTimeThemeDictionary();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the accent color of the app.
        /// </summary>
        public Color? AccentColor
        {
            get => ThemeManager.Current.AccentColor;
            set
            {
                if (ThemeManager.Current.AccentColor != value)
                {
                    ThemeManager.Current.SetCurrentValue(ThemeManager.AccentColorProperty, value);

                    if (!IsInitializePending && DesignMode.DesignModeEnabled)
                    {
                        UpdateDesignTimeSystemColors();
                    }
                }
            }
        }

        #region Design Time

        private void DesignTimeInit()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);
            UpdateDesignTimeSystemColors();
            UpdateDesignTimeThemeDictionary();
            SystemParameters.StaticPropertyChanged += OnSystemParametersPropertyChanged;
        }

        private void UpdateDesignTimeSystemColors()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);

            var colors = GetDesignTimeSystemColors();
            if (MergedDictionaries.Count == 0)
            {
                MergedDictionaries.Add(colors);
            }
            else
            {
                MergedDictionaries[0] = colors;
            }

            ThemeManager.UpdateThemeBrushes(colors);
        }

        private void UpdateDesignTimeThemeDictionary()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);

            if (SystemParameters.HighContrast)
            {
                EnsureHighContrastResources();
                updateTo(_highContrastResources);
            }
            else
            {
                var appTheme = RequestedTheme ?? ApplicationTheme.Light;
                switch (appTheme)
                {
                    case ApplicationTheme.Light:
                        EnsureLightResources();
                        updateTo(_lightResources);
                        break;
                    case ApplicationTheme.Dark:
                        EnsureDarkResources();
                        updateTo(_darkResources);
                        break;
                }
            }

            void updateTo(ResourceDictionary themeDictionary)
            {
                MergedDictionaries.RemoveIfNotNull(_lightResources);
                MergedDictionaries.RemoveIfNotNull(_darkResources);
                MergedDictionaries.RemoveIfNotNull(_highContrastResources);
                Merge(themeDictionary);
            }
        }

        private ResourceDictionary GetDesignTimeSystemColors()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);
            if (AccentColor.HasValue)
            {
                return new ColorPaletteResources { Accent = AccentColor };
            }
            else
            {
                return new ResourceDictionary { Source = PackUriHelper.GetAbsoluteUri("DesignTime/SystemColors.xaml") };
            }
        }

        private void OnSystemParametersPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast))
            {
                UpdateDesignTimeThemeDictionary();
            }
        }

        #endregion

        #region ISupportInitialize

        private bool IsInitializePending { get; set; }

        void ISupportInitialize.BeginInit()
        {
            BeginInit();
            IsInitializePending = true;
        }

        void ISupportInitialize.EndInit()
        {
            IsInitializePending = false;

            //for (int i = MergedDictionaries.Count - 1; i >= 0; i--)
            //{
            //    if (MergedDictionaries[i] is ThemeDictionary td)
            //    {
            //        ThemeDictionaries[td.Key] = td;
            //        MergedDictionaries.RemoveAt(i);
            //    }
            //}

            if (DesignMode.DesignModeEnabled)
            {
                DesignTimeInit();
            }
            else
            {
                ThemeManager.Current.Initialize();
            }

            EndInit();
        }

        #endregion

        private int MergedThemeDictionaryCount
        {
            get
            {
                int count = 0;
                if (IsMerged(_lightResources)) { count++; };
                if (IsMerged(_darkResources)) { count++; };
                if (IsMerged(_highContrastResources)) { count++; };
                return count;
            }
        }

        internal void ApplyApplicationTheme(ApplicationTheme appTheme)
        {
            Update(appTheme);
        }

        internal void UpdateMergedThemeDictionaries(ResourceDictionary target, ElementTheme theme)
        {
            bool containsAppThemeDictionary;

            if (SystemParameters.HighContrast)
            {
                target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                target.MergedDictionaries.RemoveIfNotNull(_darkResources);
                containsAppThemeDictionary = false;
            }
            else
            {
                if (theme == ElementTheme.Light)
                {
                    EnsureLightResources();
                    target.MergedDictionaries.RemoveIfNotNull(_darkResources);
                    target.MergedDictionaries.InsertIfNotExists(0, _lightResources);
                    containsAppThemeDictionary = true;
                }
                else if (theme == ElementTheme.Dark)
                {
                    EnsureDarkResources();
                    target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                    target.MergedDictionaries.InsertIfNotExists(0, _darkResources);
                    containsAppThemeDictionary = true;
                }
                else // Default
                {
                    target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                    target.MergedDictionaries.RemoveIfNotNull(_darkResources);
                    containsAppThemeDictionary = false;
                }
            }

            if (target is ResourceDictionaryEx etr)
            {
                etr.ContainsApplicationThemeDictionary = containsAppThemeDictionary;
            }
        }

        private void Update(ApplicationTheme theme)
        {
            int startIndex = DesignMode.DesignModeEnabled ? 1 : 0;

            if (SystemParameters.HighContrast)
            {
                EnsureHighContrastResources();

                if (!MergedDictionaries.Contains(_highContrastResources))
                {
                    MergedDictionaries.InsertOrReplace(startIndex + MergedThemeDictionaryCount, _highContrastResources);
                }
                else
                {
                    Debug.Assert(MergedDictionaries[startIndex + MergedThemeDictionaryCount - 1] == _highContrastResources);
                }
            }
            else
            {
                MergedDictionaries.RemoveIfNotNull(_highContrastResources);

                ResourceDictionary targetDictionary;

                if (theme == ApplicationTheme.Light)
                {
                    EnsureLightResources();
                    targetDictionary = _lightResources;
                }
                else if (theme == ApplicationTheme.Dark)
                {
                    EnsureDarkResources();
                    targetDictionary = _darkResources;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(theme));
                }

                int targetIndex = startIndex + MergedThemeDictionaryCount - 1;
                if (targetIndex == 1 && MergedDictionaries[targetIndex] != targetDictionary)
                {
                    MergedDictionaries.Swap(startIndex, targetIndex);
                }
                Debug.Assert(MergedDictionaries[targetIndex] == targetDictionary);
            }
        }

        private void EnsureLightResources()
        {
            if (_lightResources == null)
            {
                _lightResources = GetThemeDictionary(ThemeManager.LightKey);
                Merge(_lightResources);
            }
        }

        private void EnsureDarkResources()
        {
            if (_darkResources == null)
            {
                _darkResources = GetThemeDictionary(ThemeManager.DarkKey);
                Merge(_darkResources);
            }
        }

        private void EnsureHighContrastResources()
        {
            if (_highContrastResources == null)
            {
                _highContrastResources = GetThemeDictionary(ThemeManager.HighContrastKey);
            }
        }

        private void Merge(ResourceDictionary dictionary)
        {
            int insertIndex = DesignMode.DesignModeEnabled ? 1 : 0;
            MergedDictionaries.Insert(insertIndex, dictionary);
            Debug.Assert(MergedThemeDictionaryCount >= 1 && MergedThemeDictionaryCount <= 2);
        }

        private bool IsMerged(ResourceDictionary dictionary)
        {
            return dictionary != null && MergedDictionaries.Contains(dictionary);
        }

        private ResourceDictionary GetThemeDictionary(string key)
        {
            ResourceDictionary defaultThemeDictionary = ThemeManager.GetDefaultThemeDictionary(key);

            if (ThemeDictionaries.TryGetValue(key, out ResourceDictionary themeDictionary))
            {
                if (!ContainsDefaultThemeResources(themeDictionary))
                {
                    themeDictionary.MergedDictionaries.Insert(0, defaultThemeDictionary);
                }
            }
            else
            {
                themeDictionary = defaultThemeDictionary;
            }

            return themeDictionary;
        }

        private static bool ContainsDefaultThemeResources(ResourceDictionary dictionary)
        {
            if (dictionary is DefaultThemeResources)
            {
                return true;
            }

            foreach (var mergedDictionary in dictionary.MergedDictionaries)
            {
                if (mergedDictionary is DefaultThemeResources ||
                    mergedDictionary != null && ContainsDefaultThemeResources(mergedDictionary))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
