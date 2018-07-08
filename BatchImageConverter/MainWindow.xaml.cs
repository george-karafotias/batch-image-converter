using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BatchImageConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker worker = new BackgroundWorker();
        private string inputFolderPath;
        private string outputFormat;
        private string outputFolderPath;

        public MainWindow()
        {
            InitializeComponent();

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        private void SelectInputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                string[] files = GetImagesFrom(fbd.SelectedPath, false);
                if (files != null && files.Length > 0)
                    StatusBarTextBlock.Text = "Found " + files.Length.ToString() + " images";
                else
                    StatusBarTextBlock.Text = "Found no images";

                InputFolderTextBox.Text = fbd.SelectedPath;
            }
        }

        private void SelectOutputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                OutputFolderTextBox.Text = fbd.SelectedPath;
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            inputFolderPath = InputFolderTextBox.Text;
            outputFormat = (OutputFormatComboBox.Text as string).ToLower();
            outputFolderPath = OutputFolderTextBox.Text;

            if (worker.IsBusy != true)
            {
                ConvertButton.IsEnabled = false;
                CancelButton.IsEnabled = true;

                worker.RunWorkerAsync();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (worker.WorkerSupportsCancellation == true)
            {
                worker.CancelAsync();
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string[] inputImagesPath = GetImagesFrom(inputFolderPath, false);

                for (int i = 0; i < inputImagesPath.Length; i++)
                {
                    if ((worker.CancellationPending == true))
                    {
                        e.Cancel = true;
                        break;
                    }

                    string inputImagePath = inputImagesPath[i];
                    System.Drawing.Image inputImage = System.Drawing.Image.FromFile(inputImagePath);
                    string inputImageFileName = System.IO.Path.GetFileName(inputImagePath);
                    string inputImageFormat = System.IO.Path.GetExtension(inputImagePath);
                    string outputImagePath = System.IO.Path.Combine(new string[] { outputFolderPath, inputImageFileName.Replace(inputImageFormat, "." + outputFormat) });
                    inputImage.Save(outputImagePath);

                    worker.ReportProgress(Convert.ToInt32(((double)i / (double)inputImagesPath.Length) * 100));
                }
            }
            catch (Exception exc)
            {
                e.Result = "Error";
                System.Windows.MessageBox.Show(exc.Message, "An error occured!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusBarTextBlock.Text = "Completed " + e.ProgressPercentage + "%";
        }  

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                StatusBarTextBlock.Text = "Cancelled!";
            }
            else if (!(e.Error == null))
            {
                StatusBarTextBlock.Text = ("Error: " + e.Error.Message);
            }
            else
            {
                StatusBarTextBlock.Text = (Convert.ToString(e.Result) == "Error") ? "Error!" : "Done!";
            }

            ConvertButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }

        public static String[] GetImagesFrom(String searchFolder, bool isRecursive)
        {
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            string[] files = GetFilesFrom(searchFolder, filters, false);
            return files;
        }

        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }
    }
}
