using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebView
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Console.WriteLine("Hello from button 1.");
            //Point point = new Point(0, 1000);
            webBrowser1.Document.Window.ScrollTo(0, 1000);
            //webBrowser1.AutoScrollOffset = point;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Console.WriteLine("Hello from button 2.");
            webBrowser1.Document.Window.ScrollTo(0, -1000);

            //Point point = new Point(0, -1000);
            //webBrowser1.AutoScrollOffset = point;
        }
    }
}
