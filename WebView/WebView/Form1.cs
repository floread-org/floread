using System;
using System.Windows.Forms;
using System.IO.Ports;

namespace WebView
{
    public partial class Form1 : Form
    {
        // serial port source modified from:
        // http://forum.arduino.cc/index.php?topic=40336.0
        SerialPort port = new SerialPort();

        public Form1()
        {
            InitializeComponent();
            port.PortName = "COM3";
            port.BaudRate = 9600;
            port.DtrEnable = true;
            port.Open();
            port.DataReceived += port_DataReceived;
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
            Console.WriteLine(photoValue);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Console.WriteLine("Hello from button 1.");
            webBrowser1.Document.Window.ScrollTo(0, 1000);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Console.WriteLine("Hello from button 2.");
            webBrowser1.Document.Window.ScrollTo(0, -1000);
        }
    }
}