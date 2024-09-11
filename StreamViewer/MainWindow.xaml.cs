using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;

namespace StreamViewer
{
    public partial class MainWindow : Window
    {
        private List<ContentControl> _streamViews;
        private Dictionary<ContentControl, TextBlock> _titleBlocks;
        private string START_PAGE;

        public MainWindow()
        {
            InitializeComponent();
            _streamViews = new List<ContentControl> { FocusView, MonitorView1, MonitorView2, MonitorView3, MonitorView4, MonitorView5 };
            _titleBlocks = new Dictionary<ContentControl, TextBlock>
            {
                { FocusView, FocusTitle },
                { MonitorView1, Stream1Title },
                { MonitorView2, Stream2Title },
                { MonitorView3, Stream3Title },
                { MonitorView4, Stream4Title },
                { MonitorView5, Stream5Title }
            };

            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string startDirectory = Path.Combine(currentDirectory, "start");
            START_PAGE = new Uri(Path.Combine(startDirectory, "index.html")).AbsoluteUri;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var streamView in _streamViews)
            {
                try
                {
                    var webView = new Microsoft.Web.WebView2.Wpf.WebView2();
                    streamView.Content = webView;

                    await webView.EnsureCoreWebView2Async();

                    webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                    webView.CoreWebView2.Settings.IsScriptEnabled = true;
                    webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                    webView.CoreWebView2.Settings.IsWebMessageEnabled = true;

                    webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
                    webView.CoreWebView2.DocumentTitleChanged += WebView_DocumentTitleChanged;

                    webView.CoreWebView2.Navigate(START_PAGE);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading webpage: {ex.Message}");
                }
            }
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Navigation failed: {e.WebErrorStatus}"));
            }
        }

        private void WebView_DocumentTitleChanged(object sender, object e)
        {
            var webView = sender as Microsoft.Web.WebView2.Wpf.WebView2;
            if (webView == null) return;

            var streamView = _streamViews.Find(view => view.Content == webView);
            if (streamView == null) return;

            UpdateStreamTitle(streamView, webView.CoreWebView2.DocumentTitle);
        }

        private void UpdateStreamTitle(ContentControl streamView, string title)
        {
            if (_titleBlocks.TryGetValue(streamView, out TextBlock titleBlock))
            {
                Dispatcher.Invoke(() =>
                {
                    titleBlock.Text = streamView == FocusView ? $"Focus: {title}" : title;
                });
            }
        }

        private void FocusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int index))
            {
                if (index < _streamViews.Count - 1)  // -1 because FocusView is included
                {
                    var monitorView = _streamViews[index + 1];  // +1 to skip FocusView
                    var focusView = _streamViews[0];  // FocusView

                    // Swap content
                    var tempContent = monitorView.Content;
                    monitorView.Content = focusView.Content;
                    focusView.Content = tempContent;

                    if (focusView.Content is Microsoft.Web.WebView2.Wpf.WebView2 focusWebView)
                    {
                        UpdateStreamTitle(focusView, focusWebView.CoreWebView2.DocumentTitle);
                    }
                    if (monitorView.Content is Microsoft.Web.WebView2.Wpf.WebView2 monitorWebView)
                    {
                        UpdateStreamTitle(monitorView, monitorWebView.CoreWebView2.DocumentTitle);
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}