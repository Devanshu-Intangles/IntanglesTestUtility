﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.IO.Ports;
using System.IO;
using SerialPortLib;

namespace IntanglesTestUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static int numOutputLines = 0;

        public string consoleMonitorData ;
        public string serialMonitorData;

        private StringBuilder consoleOutput;
        private StringBuilder serialOutput;
        SerialPortInput serialPort;

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
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            serialPort = new SerialPortInput();
            // Listen to Serial Port events

            serialPort.ConnectionStatusChanged += delegate (object sender, ConnectionStatusChangedEventArgs args)
            {
                Debug.WriteLine("Connected = {0}", args.Connected);
            };

            serialPort.MessageReceived += delegate (object sender, MessageReceivedEventArgs args)
            {
                string tempData= string.Empty;
                tempData = BitConverter.ToString(args.Data);
                if (!String.IsNullOrEmpty(tempData))
                {
                    // Add the text to the collected output.
                    SerialOutput.Append(Environment.NewLine +
                    $"{tempData}");
                    SerialMonitorData = SerialOutput.ToString();

                }
                
            };

            // Set port options
            serialPort.SetPort("COM13", 115200);

            // Connect the serial port
            serialPort.Connect();
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            try
            {
                ConsoleOutput.Clear();
                using (Process myProcess = new Process())
                {
                    myProcess.StartInfo.UseShellExecute = false;
                    // You can start any process, HelloWorld is a do-nothing example.
                    myProcess.StartInfo.FileName = "C:\\Users\\Int01\\Source\\Repos\\ConsoleApp1\\ConsoleApp1\\bin\\Release\\ConsoleApp1.exe";
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.Arguments = "Devanshu";
                    myProcess.OutputDataReceived += SortOutputHandler;
                    myProcess.StartInfo.RedirectStandardOutput = true;
                    myProcess.Start();
                    myProcess.BeginOutputReadLine();
                    // This code assumes the process you are starting will terminate itself. 
                    // Given that is is started without a window so you cannot terminate it 
                    // on the desktop, it must terminate itself or you can do it programmatically
                    // from this application using the Kill method.
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Clear();
            ConsoleMonitorData =string.Empty;
            SerialMonitorData = string.Empty;
        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            var message = System.Text.Encoding.UTF8.GetBytes(Seial_TextBox.Text);
            serialPort.SendMessage(message);
        }

        private void SortOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            try
            {

            
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                    // Add the text to the collected output.
                    ConsoleOutput.Append(Environment.NewLine +
                    $"{outLine.Data}");
               ConsoleMonitorData = ConsoleOutput.ToString();
                
            }
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

