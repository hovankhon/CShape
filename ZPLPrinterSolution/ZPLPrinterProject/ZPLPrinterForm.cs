using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace ZPLPrinterProject
{
    public partial class ZPLPrinterForm : Form
    {
        private string _FieName = string.Empty;
        private List<string> temporaryFiles = new List<string>();

        public ZPLPrinterForm(string[] args)
        {
            InitializeComponent();

            if (args.Length > 0)
            {
                OpenFile(args[0]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Load App Settings
            var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

            widthTextbox.Text = config.AppSettings.Settings["width"].Value;
            heightTextbox.Text = config.AppSettings.Settings["height"].Value;
            unitsCombobox.SelectedItem = config.AppSettings.Settings["units"].Value;
        }

        private void ZPLPrinterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //code lai
            #region Delete temporary files
            //var _Folder = _FieName.Replace(_FieName.Substring(_FieName.LastIndexOf("\\")), "");
            labelWebBrowser.Dispose();
            foreach (var filePath in temporaryFiles)
            {
                File.Delete(filePath);
            }
            #endregion
            #region Save app settings
            var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

            config.AppSettings.Settings["width"].Value = widthTextbox.Text;
            config.AppSettings.Settings["height"].Value = heightTextbox.Text;
            config.AppSettings.Settings["units"].Value = unitsCombobox.SelectedItem.ToString();

            config.Save(ConfigurationSaveMode.Modified);
            #endregion
        }

        public void OpenFile(string path)
        {
            sourceTextBox.Text = File.ReadAllText(path);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {

                openFileDialog.Filter = "zpl files (*.zpl)|*.zpl|(*.txt)|*.txt";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        OpenFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    }
                }
            }
        }

        private void previewButton_Click(object sender, EventArgs e)
        {
            previewButton.Enabled = false;

            // to inches
            double width = Double.Parse(widthTextbox.Text);
            double height = Double.Parse(heightTextbox.Text);

            if (unitsCombobox.SelectedItem.Equals("cm"))
            {
                width = width * 0.393701;
                height = height * 0.393701;
            }
            string tempFileName = Directory.GetCurrentDirectory()+"\\temp_" + Guid.NewGuid().ToString() + ".pdf";
            string tempFileNamea = zplanelibs.Zplanelibs.ViewZPL(sourceTextBox.Text, width, height, tempFileName);
            temporaryFiles.Add(tempFileNamea);

            try
            {
                previewButton.Enabled = true;
                var _Localhost = string.Format("file:///{0}/" + tempFileNamea, Directory.GetCurrentDirectory());
                labelWebBrowser.Url = new Uri(_Localhost);
            }
            catch (WebException exception)
            {
                Console.WriteLine("Error: {0}", exception.Status);
            }
        }
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void printButton_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            printDialog.PrinterSettings = new PrinterSettings();

            if (DialogResult.OK == printDialog.ShowDialog(this))
            {
                SendStringToPrinter(printDialog.PrinterSettings.PrinterName, sourceTextBox.Text);
            }
            #region clientSocket
            /*Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.NoDelay = true;

            IPAddress ip = IPAddress.Parse("192.168.1.22");
            IPEndPoint ipep = new IPEndPoint(ip, 9100);
            clientSocket.Connect(ipep);

            byte[] fileBytes = Encoding.ASCII.GetBytes(sourceTextBox.Text);

            clientSocket.Send(fileBytes);
            clientSocket.Close();*/
            #endregion
        }
        public bool SendStringToPrinter(string PrinterName, string txt)
        {
            return zplanelibs.Zplanelibs.ReadString(PrinterName, txt);
        }
    }
}
