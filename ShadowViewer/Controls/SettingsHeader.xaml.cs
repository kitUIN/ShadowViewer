using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using FluntIcon;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShadowViewer.Controls
{
    public sealed partial class SettingsHeader : UserControl
    {
        public static readonly DependencyProperty SymbolProperty =
           DependencyProperty.Register(nameof(Symbol),
               typeof(FluentIconSymbol),
               typeof(SettingsHeader),
               new PropertyMetadata(default, new PropertyChangedCallback(OnSymbolChanged)));
        public static readonly DependencyProperty GlyphProperty =
           DependencyProperty.Register(nameof(Glyph),
               typeof(string),
               typeof(SettingsHeader),
               new PropertyMetadata(default, new PropertyChangedCallback(OnGlyphChanged)));
        public static readonly DependencyProperty DescriptionProperty =
           DependencyProperty.Register(nameof(Description),
               typeof(string),
               typeof(SettingsHeader),
               new PropertyMetadata(default, new PropertyChangedCallback(OnDescriptionChanged)));
        public static readonly DependencyProperty HeaderProperty =
           DependencyProperty.Register(nameof(Header),
               typeof(string),
               typeof(SettingsHeader),
               new PropertyMetadata(default,new PropertyChangedCallback(OnHeaderChanged)));
        public static readonly DependencyProperty IsShowDescriptionProperty =
           DependencyProperty.Register(nameof(IsShowDescription),
               typeof(bool),
               typeof(SettingsHeader),
               new PropertyMetadata(default,new PropertyChangedCallback(OnIsShowDescriptionChanged)));
        public SettingsHeader()
        {
            this.InitializeComponent();
        }
        public FluentIconSymbol Symbol
        {
            get { return (FluentIconSymbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public string Glyph
        {
            get { return (string)GetValue(GlyphProperty); }
            set { SetValue(GlyphProperty, value); }
        }
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public bool IsShowDescription
        {
            get { return (bool)GetValue(IsShowDescriptionProperty); }
            set { SetValue(IsShowDescriptionProperty, value); }
        }
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is FluentIconSymbol symbol && d is SettingsHeader header)
            {
                header.Glyph = ((char)symbol).ToString();
            }
        }
        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsHeader control = (SettingsHeader)d;
            control.Header = (string)e.NewValue;
        }
        private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsHeader control = (SettingsHeader)d;
            control.Description = (string)e.NewValue;
            control.IsShowDescription = !string.IsNullOrEmpty(control.Description);
        }
        private static void OnGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsHeader control = (SettingsHeader)d;
            control.Glyph = (string)e.NewValue;
        }
        private static void OnIsShowDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsHeader control = (SettingsHeader)d; 
            control.IsShowDescription = (bool)e.NewValue;
        }
    }
}
