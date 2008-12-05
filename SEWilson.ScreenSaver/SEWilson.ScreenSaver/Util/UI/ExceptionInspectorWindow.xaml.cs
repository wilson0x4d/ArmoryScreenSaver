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
using System.Diagnostics;

namespace SEWilson.ScreenSaver.Util.UI
{
    /// <summary>
    /// Interaction logic for ExceptionInspectorWindow.xaml
    /// </summary>
    public partial class ExceptionInspectorWindow : Window
    {
        public ExceptionInspectorWindow()
        {
            InitializeComponent();
        }

        public static void Inspect(Exception ex)
        {
            ExceptionInspectorWindow w = new ExceptionInspectorWindow();
            w.DataContext = ex;
            w.ShowDialog();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(CreateErrorDetail(DataContext as Exception));
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
            }
        }

        private void buttonSendErrorReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO modify to publish via web service instead of e-mail
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage("ssreporting@mrshaunwilson.com", "saver@mrshaunwilson.com");
                message.Subject = "ERR: " + DataContext.GetType().ToString();
                message.Body = CreateErrorDetail(DataContext as Exception);

                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                smtp.Host = "smtp.mrshaunwilson.com";
                smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("ssreporting@mrshaunwilson.com", "ssreporting2k");
                smtp.Send(message);

                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                MessageBox.Show(ex.Message, "Error Report Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string CreateErrorDetail(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendFormat("{0}: {1}: {2}",
                    ex.GetType().ToString(),
                    ex.Message,
                    ex.StackTrace);
                sb.AppendLine();
                ex = ex.InnerException;
            }
            return sb.ToString();
        }
    }
}
