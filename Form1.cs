using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace Baithituan3
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private bool isConnected = false;
        private bool isLogging = false;
        private bool isManualMode = false; // true = thủ công, false = tự động
        private System.Windows.Forms.Timer logTimer;
        private string logFilePath = "log.txt";
        private double tempThreshold = 30.0; // ngưỡng nhiệt độ
        private RollingPointPairList tempList;
        private LineItem tempLine;
        private int timeIndex = 0;

        public Form1()
        {
            InitializeComponent();
            serialPort = new SerialPort();
            serialPort.NewLine = "\r\n"; // Định dạng kết thúc dòng
            serialPort.DataReceived += SerialPort_DataReceived;

            // Khởi tạo timer 1 lần duy nhất
            logTimer = new System.Windows.Forms.Timer();
            logTimer.Interval = 2000; // 2s
            logTimer.Tick += LogTimer_Tick;

            // Thiết lập các combo 
            string[] Baudrate = { "9600", "14400", "19200", "38400", "57600", "115200" };
            comboBox2.Items.AddRange(Baudrate);
            string[] Mode = {"Thủ công", "Tự động" }; // đã có
            comboBox3.Items.AddRange(Mode); // đã có 
            comboBox3.SelectedIndex = 0;
            Control.CheckForIllegalCrossThreadCalls = false;
            LoadAvailablePorts();
            InitChart();
        }

        private void InitChart()
        {
            GraphPane pane = zedGraphControl1.GraphPane;
            pane.Title.Text = "Nhiệt độ theo thời gian";
            pane.XAxis.Title.Text = "Thời gian (s)";
            pane.YAxis.Title.Text = "Nhiệt độ (°C)";
            tempList = new RollingPointPairList(600); // Lưu 600 điểm dữ liệu
            tempLine = pane.AddCurve("Nhiệt độ", tempList, Color.Red, SymbolType.None);
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine();

                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        textBox1.AppendText(data + Environment.NewLine);
                        
                        double temp;
                        if (double.TryParse(data, out temp))
                        {
                            timeIndex++;
                            tempList.Add(timeIndex, temp);
                            zedGraphControl1.AxisChange();
                            zedGraphControl1.Invalidate();
                            if (temp > tempThreshold)
                            {
                                MessageBox.Show("Cảnh báo: Nhiệt độ vượt ngưỡng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }));
                }
            }
            catch { /* Bỏ qua lỗi nhỏ khi đóng form */ }
        }
        private void serCOM_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                serialPort.PortName = comboBox1.SelectedItem.ToString();
               
                serialPort.Open();
                isConnected = true;
                label6.Text = "Kết nối thành công!";
            }
            catch (Exception ex)
            {
                label6.Text = "Lỗi khi mở cổng: " + ex.Message;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            logTimer = new System.Windows.Forms.Timer();
            logTimer.Interval = 2000; // 2000ms = 2 giây
            logTimer.Tick += LogTimer_Tick;
        }

        private void StartLogging()
        {
            if (isLogging) return;
            isLogging = true;

            try
            {
                File.AppendAllText(logFilePath, "=== Bắt đầu ghi log ===" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở file log: " + ex.Message);
                isLogging = false;
                return;
            }

            if (isManualMode)
            {
                // Ghi log liên tục trong thread riêng (manual)
                Task.Run(() =>
                {
                    while (isLogging)
                    {
                        string log = DateTime.Now.ToString("HH:mm:ss") + " - Dữ liệu mode thủ công: " + textBox1.Text;
                        try { File.AppendAllText(logFilePath, log + Environment.NewLine); } catch { }
                        Thread.Sleep(500); // mỗi 0.5s ghi 1 lần
                    }
                });
            }
            else
            {
                // Automatic: ghi 1 lần rồi đợi timer 2s để tự dừng (theo yêu cầu của bạn)
                string log = DateTime.Now.ToString("HH:mm:ss") + " - Dữ liệu mode tự động: " + textBox1.Text;
                try { File.AppendAllText(logFilePath, log + Environment.NewLine); } catch { }
                logTimer.Start();
            }
        }

        private void StopLogging()
        {
            if (!isLogging) return;
            isLogging = false;
            try { if (logTimer.Enabled) logTimer.Stop(); } catch { }
            try { File.AppendAllText(logFilePath, "=== Dừng ghi log ===" + Environment.NewLine); } catch { }

            button2.Enabled = false;
        }

        private void LogTimer_Tick(object sender, EventArgs e)
        {
            // Sau 2s thì tự dừng log
            StopLogging();
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
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length > 0)
            {
                comboBox1.Items.AddRange(ports);
                comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = true;
                button4.Enabled = true; // cho phép nút kết nối
            }
            else
            {
                comboBox1.Items.Add("No port");
                comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = false;
                button4.Enabled = false;
            }
        }
     

        private void button4_Click(object sender, EventArgs e)
        {

            if (comboBox1.SelectedItem == null || comboBox1.SelectedItem.ToString() == "No port")
            {
                label6.Text = "Không có cổng COM để kết nối.";
                return;
            }
            if (comboBox2.SelectedItem != null)
            {
                if (int.TryParse(comboBox2.SelectedItem.ToString(), out int baud))
                    serialPort.BaudRate = baud;
            }
            else
            {
                serialPort.BaudRate = 9600;
            }

            try
            {
                if (!serialPort.IsOpen)
                    serialPort.Open();

                isConnected = true;
                label6.Text = $"Kết nối {serialPort.PortName} @{serialPort.BaudRate}";
                button4.Enabled = false;
                button3.Enabled = true;
            }
            catch (Exception ex)
            {
                label6.Text = "Lỗi khi mở cổng: " + ex.Message;
            }

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                isConnected = false;
                label6.Text = "Đã ngắt kết nối.";
            }
            else
            {
                label6.Text = "Cổng đã đóng.";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            isLogging = true;
            File.AppendAllText(logFilePath, "=== Bắt đầu ghi log ===" + Environment.NewLine);
            if (comboBox3.Text == "Thủ công")
            {
               // Manual mode logging
                Task.Run(() =>
                {
                    while (isLogging)
                    {
                        string log = DateTime.Now.ToString("HH:mm:ss") + " - Dữ liệu mode thủ công" + textBox1.Text;
                        File.AppendAllText(logFilePath, log + Environment.NewLine);
                        Thread.Sleep(500); // mỗi 0.5s ghi 1 lần
                    }
                });
            }
            else
            {
                // Automatic mode logging
                string log = DateTime.Now.ToString("HH:mm:ss") + " - Dữ liệu mode tự động" + textBox1.Text;
                File.AppendAllText(logFilePath, log + Environment.NewLine);
                logTimer.Interval = 2000; // 2000ms = 2 giây
                logTimer.Tick += (s, ev) => { StopLogging(); };
                logTimer.Start(); // đếm 2s rồi gọi LogTimer_Tick
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopLogging();
        }
    }
}

