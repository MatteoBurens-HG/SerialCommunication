using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SerialCommunication
{
    public partial class Form1 : Form
    {
        private SerialPort serialPortArduino = new SerialPort()
        {
            ReadTimeout = 1000,
            WriteTimeout = 1000
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();
                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;

                comboBoxBaudrate.SelectedIndex = comboBoxBaudrate.Items.IndexOf("115200");

                buttonConnect.BackColor = Color.Blue;
                buttonConnect.ForeColor = Color.White;
            }
            catch (Exception)
            { }
        }

        private void cboPoort_DropDown(object sender, EventArgs e)
        {
            try
            {
                string selected = (string)comboBoxPoort.SelectedItem;
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();

                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);

                comboBoxPoort.SelectedIndex = comboBoxPoort.Items.IndexOf(selected);
            }
            catch (Exception)
            {
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    // Disconnect
                    serialPortArduino.Close();
                    radioButtonVerbonden.Checked = false;
                    buttonConnect.Text = "Connect";
                    labelStatus.Text = "Verbinding verbroken.";
                }
                else
                {
                    // Connect
                    serialPortArduino.PortName = comboBoxPoort.SelectedItem.ToString();
                    serialPortArduino.BaudRate = int.Parse(comboBoxBaudrate.SelectedItem.ToString());
                    serialPortArduino.DataBits = (int)numericUpDownDatabits.Value;

                    if (radioButtonParityEven.Checked) serialPortArduino.Parity = Parity.Even;
                    else if (radioButtonParityOdd.Checked) serialPortArduino.Parity = Parity.Odd;
                    else if (radioButtonParityMark.Checked) serialPortArduino.Parity = Parity.Mark;
                    else if (radioButtonParitySpace.Checked) serialPortArduino.Parity = Parity.Space;
                    else serialPortArduino.Parity = Parity.None;

                    if (radioButtonStopbitsTwo.Checked) serialPortArduino.StopBits = StopBits.Two;
                    else if (radioButtonStopbitsOnePointFive.Checked) serialPortArduino.StopBits = StopBits.OnePointFive;
                    else if (radioButtonStopbitsNone.Checked) serialPortArduino.StopBits = StopBits.None;
                    else serialPortArduino.StopBits = StopBits.One;

                    if (radioButtonHandshakeXonXoff.Checked) serialPortArduino.Handshake = Handshake.XOnXOff;
                    else if (radioButtonHandshakeRTSXonXoff.Checked) serialPortArduino.Handshake = Handshake.RequestToSendXOnXOff;
                    else if (radioButtonHandshakeRTS.Checked) serialPortArduino.Handshake = Handshake.RequestToSend;
                    else serialPortArduino.Handshake = Handshake.None;

                    serialPortArduino.DtrEnable = checkBoxDtrEnable.Checked;
                    serialPortArduino.RtsEnable = checkBoxRtsEnable.Checked;

                    serialPortArduino.Open();

                    // Verify Arduino connection with ping/pong
                    if (VerifyArduinoConnection())
                    {
                        radioButtonVerbonden.Checked = true;
                        buttonConnect.Text = "Disconnect";
                        labelStatus.Text = "Verbinding tot stand gebracht.";
                    }
                    else
                    {
                        serialPortArduino.Close();
                        radioButtonVerbonden.Checked = false;
                        buttonConnect.Text = "Connect";
                        labelStatus.Text = "Arduino verificatie mislukt.";
                    }
                }
            }
            catch (Exception ex)
            {
                if (serialPortArduino.IsOpen) serialPortArduino.Close();
                radioButtonVerbonden.Checked = false;
                buttonConnect.Text = "Connect";
                labelStatus.Text = $"Fout: {ex.Message}";
            }
        }

        private bool VerifyArduinoConnection()
        {
            try
            {
                serialPortArduino.WriteLine("ping");
                string response = serialPortArduino.ReadLine();
                return response != null && response.Trim().Equals("pong", StringComparison.OrdinalIgnoreCase);
            }
            catch (TimeoutException)
            {
                labelStatus.Text = "Fout: Arduino reageert niet (timeout).";
                return false;
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Fout: {ex.Message}";
                return false;
            }
        }
    }
}
