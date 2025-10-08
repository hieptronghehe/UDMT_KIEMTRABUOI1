using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        public Form1()
        {
            InitializeComponent();
            serialPort = new SerialPort();
            bool isManualMode = false;    // true = thủ công, false = tự động
            bool isLogging = false;       // trạng thái đang ghi log
            System.Windows.Forms.Timer logTimer; // dùng cho chế độ tự động
            string logFilePath = "log.txt"; // nơi lưu file log

            string[] Baudrate = { "9600", "14400", "19200", "38400", "57600", "115200" };
            comboBox2.Items.AddRange(Baudrate);
            comboBox1.DataSource = SerialPort.GetPortNames();
            Control.CheckForIllegalCrossThreadCalls = false;
            string[] Mode = {"Thủ công", "Tự động" };
            comboBox3.Items.AddRange(Mode);
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
            isLogging = true;
            File.AppendAllText(logFilePath, "=== Bắt đầu ghi log ===" + Environment.NewLine);

            if (isManualMode)
            {
                // Ghi log liên tục trong thread riêng
                Task.Run(() =>
                {
                    while (isLogging)
                    {
                        string log = DateTime.Now.ToString("HH:mm:ss") + " - Dữ liệu mode thủ công";
                        File.AppendAllText(logFilePath, log + Environment.NewLine);
                        Thread.Sleep(500); // mỗi 0.5s ghi 1 lần
                    }
                });
            }
            else
            {
                // Chế độ tự động: chỉ ghi 1 lần rồi đợi Timer tắt
                string log = DateTime.Now.ToString("HH:mm:ss") + " - Dữ liệu mode tự động";
                File.AppendAllText(logFilePath, log + Environment.NewLine);

                logTimer.Start(); // đếm 2s rồi gọi LogTimer_Tick
            }
        }

        private void StopLogging()
        {
            isLogging = false;
            logTimer.Stop();
            File.AppendAllText(logFilePath, "=== Dừng ghi log ===" + Environment.NewLine);
        }

        private void LogTimer_Tick(object sender, EventArgs e)
        {
            // Sau 2s thì tự dừng log
            StopLogging();
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            // Kiểm tra chế độ từ ComboBox hoặc RadioButton
            if (comboBoxMode.SelectedItem.ToString() == "Thủ công")
                isManualMode = true;
            else
                isManualMode = false;

            StartLogging();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
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
            }
            else
            {
                comboBox1.Items.Add("No port");
                comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
          
            serialPort.PortName = comboBox1.SelectedItem.ToString();  // Lấy tên COM
            serialPort.BaudRate = int.Parse(comboBox2.SelectedItem.ToString());  // Lấy baudrate

            try
            {
                serialPort.Open(); // Mở cổng
                label6.Text = "Kết nối thành công!";
            }
            catch (Exception ex)
            {
                label6.Text ="Lỗi khi mở cổng: " + ex.Message;
            }

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

