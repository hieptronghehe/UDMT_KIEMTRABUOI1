using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Baithituan3
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private bool isConnected = false;
        private bool isLogging = false;
        private bool isManualMode = true; // true = thủ công, false = tự động
        private System.Windows.Forms.Timer logTimer;
        private string logFilePath = "log.txt";
        private double tempThreshold = 30.0; // Ngưỡng cảnh báo
        private RollingPointPairList tempList;
        private LineItem tempLine;
        private int timeIndex = 0;
        private readonly object logLock = new object();
        private DateTime lastWarningShown = DateTime.MinValue;
        public Form1()
        {
            InitializeComponent();

            serialPort = new SerialPort();
            serialPort.NewLine = "\r\n";
            serialPort.DataReceived += SerialPort_DataReceived;

            logTimer = new System.Windows.Forms.Timer();
            logTimer.Interval = 2000; // 2 giây
            logTimer.Tick += LogTimer_Tick;

            string[] baudrates = { "9600", "14400", "19200", "38400", "57600", "115200" };
            comboBox2.Items.AddRange(baudrates);
            comboBox2.SelectedIndex = 0;

            string[] modes = { "Thủ công", "Tự động" };
            comboBox3.Items.AddRange(modes);
            comboBox3.SelectedIndex = 0;

            LoadAvailablePorts();
            InitChart();

            label6.Text = "Disconnected";

        }

        private void InitChart()
        {
            GraphPane pane = zedGraphControl1.GraphPane;
            pane.Title.Text = "Nhiệt độ theo thời gian";
            pane.XAxis.Title.Text = "Thời gian (s)";
            pane.YAxis.Title.Text = "Nhiệt độ (°C)";
            tempList = new RollingPointPairList(600);
            tempLine = pane.AddCurve("Nhiệt độ", tempList, Color.Red, SymbolType.None);
            zedGraphControl1.AxisChange();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine().Trim();
                if (double.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out double temperature))
                {
                    this.BeginInvoke((Action)(() => UpdateTemperature(temperature)));
                }
            }
            catch { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            isManualMode = comboBox3.SelectedIndex == 0;
        }
        private void StartLogging()
        {
            if (!isConnected)
            {
                label6.Text = "Chưa kết nối Arduino!";
                return;
            }

            isLogging = true;
            File.AppendAllText(logFilePath, $"=== Bắt đầu ghi log ({DateTime.Now}) ==={Environment.NewLine}");

            if (isManualMode)
            {
                label6.Text = "Ghi log thủ công...";
            }
            else
            {
                label6.Text = "Ghi log tự động (2s)...";
                logTimer.Start();
            }
        }

        private void StopLogging()
        {
            if (!isLogging) return;
            logTimer.Stop();
            isLogging = false;
            File.AppendAllText(logFilePath, $"=== Dừng ghi log ({DateTime.Now}) ==={Environment.NewLine}");
            label6.Text = "Đã dừng ghi log.";
        }

        private void LogTimer_Tick(object sender, EventArgs e)
        {
            StopLogging(); // Sau 2s thì dừng
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadAvailablePorts();
        }
        private void LoadAvailablePorts()
        {
            comboBox1.Items.Clear();
            List<string> activePorts = new List<string>();

            foreach (string port in SerialPort.GetPortNames())
            {
                try
                {
                    using (SerialPort testPort = new SerialPort(port))
                    {
                        testPort.Open();
                        testPort.Close();
                        activePorts.Add(port);
                    }
                }
                catch
                {
                    // Cổng đang bị chiếm hoặc không thực sự tồn tại
                }
            }

            comboBox1.Items.AddRange(activePorts.ToArray());
            if (activePorts.Count > 0)
                comboBox1.SelectedIndex = 0;
            else
                label6.Text = "Không tìm thấy cổng COM nào.";
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                label6.Text = "Đã kết nối!";
                return;
            }

            if (comboBox1.SelectedItem == null)
            {
                label6.Text = "Chưa chọn cổng COM!";
                return;
            }

            try
            {
                serialPort.PortName = comboBox1.SelectedItem.ToString();
                serialPort.BaudRate = int.Parse(comboBox2.SelectedItem.ToString());
                serialPort.Open();
                isConnected = true;
                label6.Text = "Đang kết nối...";
                button3.Enabled = false;
                button4.Enabled = true;
            }
            catch (Exception ex)
            {
                label6.Text = "Lỗi kết nối: " + ex.Message;
            }
            StartLogging();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort.IsOpen) serialPort.Close();
            }
            catch { }

            isConnected = false;
            StopLogging();
            label6.Text = "Disconnected";
            button3.Enabled = true;
            button4.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopLogging();
        }
    

private void UpdateTemperature(double temperature)
        {
            textBox1.Text = temperature.ToString("0.0") + " °C";

            tempList.Add(timeIndex++, temperature);
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            if (temperature > tempThreshold)
            {
                if ((DateTime.Now - lastWarningShown).TotalSeconds > 5)
                {
                    label6.Text = "⚠️ Cảnh báo: Nhiệt độ vượt ngưỡng!";
                    label6.ForeColor = Color.Red;
                    lastWarningShown = DateTime.Now;
                }
            }
            else
            {
                label6.Text = "Đang giám sát...";
                label6.ForeColor = Color.Black;
            }

            if (isLogging)
            {
                WriteLog(temperature);
            }
        }


        private void WriteLog(double temperature)
        {
            lock (logLock)
            {
                try
                {
                    string logLine = $"{DateTime.Now:HH:mm:ss}, {temperature:0.00}";
                    File.AppendAllText(logFilePath, logLine + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    label6.Text = "Lỗi ghi log: " + ex.Message;
                }
            }
        }
    } 
}