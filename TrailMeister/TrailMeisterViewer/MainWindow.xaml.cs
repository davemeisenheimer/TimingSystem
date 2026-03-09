using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TrailMeisterDb;
using TrailMeisterViewer.Windows.Settings;

namespace TrailMeisterViewer
{
    public partial class MainWindow : Window
    {
        private MainWindowVM _viewModel;
        private readonly Stack<(object Content, Action? OnBack)> _navigationStack = new();

        public MainWindow()
        {
            AppSettingsService.Load();
            this._viewModel = new MainWindowVM(this);
            this.DataContext = this._viewModel;
            InitializeComponent();
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsDialog { Owner = this };
            dlg.ShowDialog();
        }

        public void Navigate(UIElement view, Action? onBack = null)
        {
            _navigationStack.Push((mainContent.Content, onBack));
            mainContent.Content = view;
            btnBack.Visibility = Visibility.Visible;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (_navigationStack.Count == 0) return;
            var (content, onBack) = _navigationStack.Pop();
            mainContent.Content = content;
            onBack?.Invoke();
            btnBack.Visibility = _navigationStack.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
