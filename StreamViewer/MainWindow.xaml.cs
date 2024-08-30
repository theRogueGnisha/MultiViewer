using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamViewer
{
    public partial class MainWindow : Window
    {
        private List<ContentControl> _streamViews;
        private Dictionary<ContentControl, (string Url, string Title)> _streamInfo = new Dictionary<ContentControl, (string, string)>();
        private Dictionary<ContentControl, TextBlock> _titleBlocks;

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
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var streams = File.ReadAllLines("streams.txt")
                .Select(line => line.Split(','))
                .Where(parts => parts.Length == 2)
                .Select(parts => (Url: parts[0].Trim(), Title: parts[1].Trim()))
                .ToList();

            for (int i = 0; i < Math.Min(streams.Count, _streamViews.Count); i++)
            {
                try
                {
                    var webView = new Microsoft.Web.WebView2.Wpf.WebView2();
                    _streamViews[i].Content = webView;
                    _streamInfo[_streamViews[i]] = streams[i];

                    await webView.EnsureCoreWebView2Async();

                    webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                    webView.CoreWebView2.Settings.IsScriptEnabled = true;
                    webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                    webView.CoreWebView2.Settings.IsWebMessageEnabled = true;

                    webView.CoreWebView2.NavigationCompleted += (s, args) =>
                    {
                        if (!args.IsSuccess)
                        {
                            Dispatcher.Invoke(() => MessageBox.Show($"Navigation failed for {streams[i].Url}: {args.WebErrorStatus}"));
                        }
                    };

                    UpdateStreamTitle(_streamViews[i], streams[i].Title);
                    webView.CoreWebView2.Navigate(streams[i].Url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading stream {streams[i].Url}: {ex.Message}");
                }
            }
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

                    // Swap stream info
                    var tempInfo = _streamInfo[monitorView];
                    _streamInfo[monitorView] = _streamInfo[focusView];
                    _streamInfo[focusView] = tempInfo;

                    // Update titles
                    UpdateStreamTitle(focusView, _streamInfo[focusView].Title);
                    UpdateStreamTitle(monitorView, _streamInfo[monitorView].Title);
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
