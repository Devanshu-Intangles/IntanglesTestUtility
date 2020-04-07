using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using SerialPortLib;
using System.Collections.ObjectModel;
using IntanglesTestUtility.Model;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Data;

namespace IntanglesTestUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private TaskCompletionSource<bool> eventHandled;
        private Process FlashTestProcess;
        string IMEINo;
        string SimNo;
        private int ProcessId;
        public static string processName;
        public string consoleMonitorData ;
        public string serialMonitorData;

        private StringBuilder consoleOutput;
        private StringBuilder serialOutput;

        private ObservableCollection<TestResultsModel> _resultsCollection;
        private string testResult;
        public ObservableCollection<TestResultsModel> ResultsCollection
        {
            get
            {
                return _resultsCollection ?? (_resultsCollection = new ObservableCollection<TestResultsModel>());
            }
            set
            {
                _resultsCollection = value;
                RaisePropertyChange("ResultsCollection");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public StringBuilder ConsoleOutput
        {
            get
            {
                if (consoleOutput == null)
                {
                    consoleOutput = new StringBuilder();
                }
                return consoleOutput;
            }
            set
            {
                consoleOutput = value;
                RaisePropertyChange("ConsoleOutput");
            }
        }
        public string TestResult
        {
            get
            {
                return testResult;
            }
            set
            {
                testResult = value;
                RaisePropertyChange("TestResult");
            }
        }

        public StringBuilder SerialOutput
        {
            get
            {
                if (serialOutput == null)
                {
                    serialOutput = new StringBuilder();
                }
                return serialOutput;
            }
            set
            {
                serialOutput = value;
                RaisePropertyChange("SerialOutput");
            }
        }
        public string ConsoleMonitorData
        {
            get
            {
                return consoleMonitorData;
            }
            set
            {
                consoleMonitorData = value;
                RaisePropertyChange("ConsoleMonitorData");
            }
        }
        public string SerialMonitorData
        {
            get
            {
                return serialMonitorData;
            }
            set
            {
                serialMonitorData = value;
                RaisePropertyChange("SerialMonitorData");
            }
        }

        enum Params
        {
            STM_IMU,
            STM_RTC,
            STM_Internal_Battery,
            STM_External_Battery,
            STM_CAN1,
            STM_CAN2,
            STM_Ignition,
            WP_IMEI,
            WP_SIM,
            WP_GPS,
            WP_Digital_Inputs
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            // Listen to Serial Port events
            ResultsCollection.Add(new TestResultsModel { IsSelected=false,Parameter="STM-IMU",Result= string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "STM-RTC", Result = string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "STM-Internal Battery", Result = string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "STM-External Battery", Result = string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "STM-CAN1", Result = string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "STM-CAN2", Result = string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "STM-Ignition", Result = string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "WP-IMEI", Result = string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "WP-SIM", Result = string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "WP-GPS", Result = string.Empty });
            ResultsCollection.Add(new TestResultsModel { IsSelected = false, Parameter = "WP-Digital Inputs", Result = string.Empty });

        }

        private async void Button_Click_StartAsync(object sender, RoutedEventArgs e)
        {
            ClearAllData();
            var appSettings = ConfigurationManager.AppSettings;
            if (File.Exists(appSettings["TempFileName"]))
            {
                 // File.Delete(appSettings["TempFileName"]);
            }
            eventHandled = new TaskCompletionSource<bool>();
            try
            {
                ConsoleOutput.Clear();
                using (FlashTestProcess = new Process())
                {
                    FlashTestProcess.StartInfo.UseShellExecute = false;
                    FlashTestProcess.EnableRaisingEvents = true;
                    FlashTestProcess.StartInfo.FileName = appSettings["Exe"];
                    FlashTestProcess.StartInfo.CreateNoWindow = true;
                    FlashTestProcess.StartInfo.Arguments = appSettings["ScriptPath"];
                    FlashTestProcess.OutputDataReceived += ConsoleOutputHandler;
                    FlashTestProcess.Exited += new EventHandler(FlashTestProcess_Exited);
                    FlashTestProcess.StartInfo.RedirectStandardOutput = true;
                    FlashTestProcess.Start();
                    FlashTestProcess.BeginOutputReadLine();
                    ProcessId=FlashTestProcess.Id;
                    await Task.WhenAny(eventHandled.Task, Task.Delay(300000));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            ClearAllData();
        }

        void ClearAllData()
        {
            try
            {
                ConsoleOutput.Clear();
                SerialOutput.Clear();
                ConsoleMonitorData = string.Empty;
                SerialMonitorData = string.Empty;
                foreach (var model in ResultsCollection)
                {
                    model.IsSelected = false;
                    model.Result = string.Empty;
                }
                IMEINo = null;
                SimNo = null;
                TestResult = string.Empty;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            // var message = System.Text.Encoding.UTF8.GetBytes(Seial_TextBox.Text);
            // serialPort.SendMessage(message);
        }

        private void ConsoleOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                if (!String.IsNullOrEmpty(outLine.Data))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (outLine.Data.Contains(appSettings["ErrorText"]))
                        {
                            TextBlock_ConsoleMonitor.Inlines.Add(new Run(Environment.NewLine +
                                $"{outLine.Data}")
                            { Foreground = Brushes.Red });
                            
                        }
                        else
                        {
                            TextBlock_ConsoleMonitor.Inlines.Add(Environment.NewLine +
                                $"{outLine.Data}");
                        }
                    });
                    // Add the text to the collected output.
                    ConsoleOutput.Append(Environment.NewLine +
                    $"{outLine.Data}");
                    // ConsoleMonitorData = ConsoleOutput.ToString();
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void FlashTestProcess_Exited(object sender, System.EventArgs e)
        {
            try
            {
                Process[] process= Process.GetProcesses();
                process.Where(x => x.Id == ProcessId).FirstOrDefault()?.Kill();
                var appSettings = ConfigurationManager.AppSettings;
                List<char> res = new List<char>();
                if (File.Exists(appSettings["TempFileName"]))
                {
                    var data = File.ReadAllLines(appSettings["TempFileName"]);
                    IMEINo = data[0];
                    SimNo = data[1];
                    res = data[2].ToArray().ToList();
                    //res = data[2].Split(',').ToList();
                }
                for (int i = 0; i < 11; i++)
                {
                    if (res != null && res.Count > 0 && res?[i] == '1')
                    {
                        ResultsCollection[i].IsSelected = true;
                        ResultsCollection[i].Result = "Pass.";
                        if (i == (int)Params.WP_IMEI)
                        {
                            if (IMEINo != null && IMEINo.Length == 14)
                            {
                                ResultsCollection[i].Result = "Pass. IMEI=" + IMEINo;
                            }
                            else
                            {
                                ResultsCollection[i].Result = "Fail.";
                                ResultsCollection[i].IsSelected = false;
                            }

                        }
                        if (i == (int)Params.WP_SIM)
                        {
                            if (SimNo != null && SimNo.Length == 19)
                            {
                                ResultsCollection[i].Result = "Pass. SIM No=" + SimNo;
                            }
                            else
                            {
                                ResultsCollection[i].Result = "Fail.";
                                ResultsCollection[i].IsSelected = false;
                            }
                        }
                    }
                    else
                    {
                        ResultsCollection[i].IsSelected = false;
                        ResultsCollection[i].Result = "Fail.";
                    }
                }
                TestResult = ResultsCollection.All(x => x.IsSelected == true) ? "Pass" : "Fail";
                if (ConsoleMonitorData.Contains(appSettings["RetryText"]))
                {
                    MessageBox.Show(" Please check the connections and retry","Flash Error",MessageBoxButton.OK);

                }
                if (IMEINo != null)
                {
                    File.WriteAllText("Logs/" + IMEINo + ".txt", ConsoleMonitorData);
                }
                else
                {
                    File.WriteAllText("Logs/" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt", ConsoleMonitorData);
                 
                }
                eventHandled.TrySetResult(true);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


        public void RaisePropertyChange(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

    }

    public class ValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null != value)
            {
                if (value.ToString().Contains("Error:"))
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}

