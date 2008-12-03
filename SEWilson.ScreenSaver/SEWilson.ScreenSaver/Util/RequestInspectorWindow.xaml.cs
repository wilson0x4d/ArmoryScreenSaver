using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace SEWilson.ScreenSaver.Util
{
    /// <summary>
    /// Interaction logic for RequestInspectorWindow.xaml
    /// </summary>
    public partial class RequestInspectorWindow : Window
    {
        public RequestInspectorWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            HttpWebRequest httpWebRequest = HttpWebRequest.Create(new Uri(textBox1.Text)) as HttpWebRequest;
            httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618)";
            httpWebRequest.Referer = textBox1.Text;
            httpWebRequest.Accept = "text/xml";

            try
            {
                using (StreamReader reader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()))
                {
                    string rte = reader.ReadToEnd();
                    textBlock1.Text = rte;
                }
            }
            catch
            {
            }
        }
    }
}
