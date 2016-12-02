// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Windows.Forms;
using CefSharp.MinimalExample.WinForms.Controls;
using CefSharp.WinForms;
using EyeXFramework;
using EyeXFramework.Forms;
using Tobii.EyeX.Framework;

using System.IO.Ports;
using System.Drawing;

namespace CefSharp.MinimalExample.WinForms
{
    public partial class BrowserForm : Form
    {
        private readonly ChromiumWebBrowser browser;
        // serial port source modified from:
        // http://forum.arduino.cc/index.php?topic=40336.0
        SerialPort port;
        int counter = 0;
        int accumulator = 0;
        static int N = 10; // number of samples
        GazePointDataStream lightlyFilteredGazeDataStream;

        public BrowserForm(Program p)
        {
            InitializeComponent();
            port = new SerialPort();
            p.EyeXHost.Connect(behaviorMap1);
            
            port.PortName = "COM3";
            port.BaudRate = 9600;
            port.DtrEnable = true;
            port.Open();
            port.DataReceived += port_DataReceived;

            Text = "FloRead";
            WindowState = FormWindowState.Maximized;
            // http://www.nytimes.com/2011/03/27/business/27novel.html
            browser = new ChromiumWebBrowser("file:///C:/Users/Abhii/Source/Repos/floread/floreadWeb/index.html")
            {
                Dock = DockStyle.Fill,
            };
            toolStripContainer.ContentPanel.Controls.Add(browser);

            browser.LoadingStateChanged += OnLoadingStateChanged;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
            browser.StatusMessage += OnBrowserStatusMessage;
            browser.TitleChanged += OnBrowserTitleChanged;
            browser.AddressChanged += OnBrowserAddressChanged;

            var bitness = Environment.Is64BitProcess ? "x64" : "x86";
            var version = String.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}, Environment: {3}", Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion, bitness);
            DisplayOutput(version);
            behaviorMap1.Add(browser, new GazeAwareBehavior(OnGaze));
            this.lightlyFilteredGazeDataStream = p.EyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered);
        }

        private void OnGaze(object sender, GazeAwareEventArgs ea)
        {
            var browsr = sender as ChromiumWebBrowser;
            if (browsr != null)
            {
                //panel.BorderStyle = (e.HasGaze) ? BorderStyle.FixedSingle : BorderStyle.None;
                //Console.WriteLine(e.X);
                //Console.WriteLine(e.Y);
                if (ea.HasGaze)
                {
                    this.lightlyFilteredGazeDataStream.Next += (s, e) => doSomething(e.X, e.Y, e.Timestamp);
                }
            }
        }

        private void MoveCursor(double x,double y)
        {
            // Set the Current cursor, move the cursor's Position,
            // and set its clipping rectangle to the form. 

            this.Cursor = new Cursor(Cursor.Current.Handle);
            Cursor.Position = new Point((int)x, (int)y);
            Cursor.Clip = new Rectangle(this.Location, this.Size);
        }

        private void doSomething(double X, double Y, double timestamp)
        {
            Console.WriteLine("Gaze point at ({0:0.0}, {1:0.0}) @{2:0}", X, Y, timestamp);
            MoveCursor(X, Y);
        }

        private void port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string line = port.ReadLine();
            BeginInvoke(new LineReceivedEvent(LineReceived), line);
        }

        private delegate void LineReceivedEvent(string line);
        private void LineReceived(string line)
        {
            // TODO: modify the CSS of the web-app
            int photoValue = int.Parse(line);

            if (counter < N - 1)
            {
                accumulator += photoValue;
                counter++;
                return;
            }
            else {
                photoValue = (accumulator + photoValue) / N;
                counter = 0;
                accumulator = 0;
            }

            Console.WriteLine(photoValue);

            string color = "black", background = "white";

            // room is brightest
            if (photoValue < 150)
            {
                color = "black";
                background = "white";
            }
            else if (photoValue < 325)
            {
                color = "#073642";
                background = "#eee8d5";

            }
            else if (photoValue < 440)
            {
                color = "#fdf6e3";
                background = "#586e75";
            }
            else if (photoValue < 570)
            {
                color = "#eee8d5";
                background = "#073642";
            }
            else  // room is darkest
            {
                color = "white";
                background = "black";
            }
            // https://github.com/cefsharp/CefSharp/wiki/Frequently-asked-questions#CallJS
            // var script = string.Format("setColors({0});'", photoValue);
            browser.GetMainFrame().ExecuteJavaScriptAsync(string.Format("document.body.style.background = '{0}'; document.body.style.color = '{1}';", background, color));
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            DisplayOutput(string.Format("Line: {0}, Source: {1}, Message: {2}", args.Line, args.Source, args.Message));
        }

        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => statusLabel.Text = args.Value);
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            SetCanGoBack(args.CanGoBack);
            SetCanGoForward(args.CanGoForward);

            this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));
        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);
        }

        private void SetCanGoBack(bool canGoBack)
        {
            this.InvokeOnUiThreadIfRequired(() => backButton.Enabled = canGoBack);
        }

        private void SetCanGoForward(bool canGoForward)
        {
            this.InvokeOnUiThreadIfRequired(() => forwardButton.Enabled = canGoForward);
        }

        private void SetIsLoading(bool isLoading)
        {
            goButton.Text = isLoading ?
                "Stop" :
                "Go";
            goButton.Image = isLoading ?
                Properties.Resources.nav_plain_red :
                Properties.Resources.nav_plain_green;

            HandleToolStripLayout();
        }

        public void DisplayOutput(string output)
        {
            this.InvokeOnUiThreadIfRequired(() => outputLabel.Text = output);
        }

        private void HandleToolStripLayout(object sender, LayoutEventArgs e)
        {
            HandleToolStripLayout();
        }

        private void HandleToolStripLayout()
        {
            var width = toolStrip1.Width;
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item != urlTextBox)
                {
                    width -= item.Width - item.Margin.Horizontal;
                }
            }
            urlTextBox.Width = Math.Max(0, width - urlTextBox.Margin.Horizontal - 18);
        }

        private void ExitMenuItemClick(object sender, EventArgs e)
        {
            browser.Dispose();
            Cef.Shutdown();
            Close();
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            browser.Back();
        }

        private void ForwardButtonClick(object sender, EventArgs e)
        {
            browser.Forward();
        }

        private void UrlTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            LoadUrl(urlTextBox.Text);
        }

        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                browser.Load(url);
            }
        }

        private void BrowserForm_Load(object sender, EventArgs e)
        {

        }
    }
}
