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

        private System.Windows.Forms.Timer timerOefening4 = new System.Windows.Forms.Timer()
        {
            Interval = 1000,
            Enabled = false
        };

        private System.Windows.Forms.Timer timerOefening5 = new System.Windows.Forms.Timer()
        {
            Interval = 1000,
            Enabled = false
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

                // Subscribe to trackbar scroll events for PWM controls
                trackBarPWM9.Scroll += trackBarPWM9_Scroll;
                trackBarPWM10.Scroll += trackBarPWM10_Scroll;
                trackBarPWM11.Scroll += trackBarPWM11_Scroll;

                // Subscribe to timer events
                timerOefening4.Tick += timerOefening4_Tick;
                timerOefening5.Tick += timerOefening5_Tick;
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

                    serialPortArduino.DtrEnable = true;
                    serialPortArduino.RtsEnable = true;

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
                labelStatus.Text = "Fout: " + ex.Message;
            }
        }

        private bool VerifyArduinoConnection()
        {
            try
            {
                System.Threading.Thread.Sleep(3500); // Wacht 3.5 seconden zodat Arduino bootloader kan opstarten
                serialPortArduino.WriteLine("ping");
                string response = serialPortArduino.ReadLine();
                
                if (response != null && response.Trim().Equals("pong", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    labelStatus.Text = "Fout: Arduino antwoordde onverwacht: '" + response + "'";
                    return false;
                }
            }
            catch (TimeoutException)
            {
                labelStatus.Text = "Fout: Arduino reageert niet (timeout). Controleer of sketch 'ping' ondersteunt.";
                return false;
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout: " + ex.Message;
                return false;
            }
        }

        private void checkBoxDigital2_Checked(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    labelStatus.Text = "Fout: Geen seriële verbinding geopend.";
                    checkBoxDigital2.Checked = false;
                    return;
                }

                string command = checkBoxDigital2.Checked ? "set d2 high" : "set d2 low";
                serialPortArduino.WriteLine(command);
                labelStatus.Text = "Commando verstuurd: " + command;
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout: " + ex.Message;
                checkBoxDigital2.Checked = false;
            }
        }

        private void checkBoxDigital3_CheckedChanged(object sender, EventArgs e)
        {

            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    labelStatus.Text = "Fout: Geen seriële verbinding geopend.";
                    checkBoxDigital3.Checked = false;
                    return;
                }

                string command = checkBoxDigital3.Checked ? "set d3 high" : "set d3 low";
                serialPortArduino.WriteLine(command);
                labelStatus.Text = "Commando verstuurd: " + command;
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout: " + ex.Message;
                checkBoxDigital3.Checked = false;
            }
        }

        private void checkBoxDigital4_CheckedChanged(object sender, EventArgs e)
        {
            
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    labelStatus.Text = "Fout: Geen seriële verbinding geopend.";
                    checkBoxDigital4.Checked = false;
                    return;
                }

                string command = checkBoxDigital4.Checked ? "set d4 high" : "set d4 low";
                serialPortArduino.WriteLine(command);
                labelStatus.Text = "Commando verstuurd: " + command;
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout: " + ex.Message;
                checkBoxDigital4.Checked = false;
            }
        }

        private void trackBarPWM9_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    labelStatus.Text = "Fout: Geen seriële verbinding geopend.";
                    return;
                }

                int pwmValue = trackBarPWM9.Value;
                string command = "set pwm9 " + pwmValue;
                serialPortArduino.WriteLine(command);
                labelStatus.Text = "Commando verstuurd: " + command;
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout: " + ex.Message;
            }
        }

        private void trackBarPWM10_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    labelStatus.Text = "Fout: Geen seriële verbinding geopend.";
                    return;
                }

                int pwmValue = trackBarPWM10.Value;
                string command = "set pwm10 " + pwmValue;
                serialPortArduino.WriteLine(command);
                labelStatus.Text = "Commando verstuurd: " + command;
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout: " + ex.Message;
            }
        }

        private void trackBarPWM11_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    labelStatus.Text = "Fout: Geen seriële verbinding geopend.";
                    return;
                }

                int pwmValue = trackBarPWM11.Value;
                string command = "set pwm11 " + pwmValue;
                serialPortArduino.WriteLine(command);
                labelStatus.Text = "Commando verstuurd: " + command;
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout: " + ex.Message;
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl.SelectedTab == tabControl.TabPages["tabPageOefening3"])
                {
                    timerOefening3.Enabled = true;
                }
                else
                {
                    timerOefening3.Enabled = false;
                }

                if (tabControl.SelectedTab == tabControl.TabPages["tabPageOefening4"])
                {
                    timerOefening4.Enabled = true;
                }
                else
                {
                    timerOefening4.Enabled = false;
                }

                if (tabControl.SelectedTab == tabControl.TabPages["tabPageOefening5"])
                {
                    timerOefening5.Enabled = true;
                }
                else
                {
                    timerOefening5.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout: " + ex.Message;
            }
        }

        private void timerOefening3_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    return;
                }

                // Clear any existing data
                if (serialPortArduino.BytesToRead > 0)
                {
                    serialPortArduino.ReadExisting();
                }

                // Query and display digital pin 5
                serialPortArduino.WriteLine("get d5");
                string response5 = ReadSerialResponse();
                if (response5 != null)
                {
                    response5 = response5.Trim();
                    radioButtonDigital5.Checked = response5.Equals("1");
                }

                // Query and display digital pin 6
                serialPortArduino.WriteLine("get d6");
                string response6 = ReadSerialResponse();
                if (response6 != null)
                {
                    response6 = response6.Trim();
                    radioButtonDigital6.Checked = response6.Equals("1");
                }

                // Query and display digital pin 7
                serialPortArduino.WriteLine("get d7");
                string response7 = ReadSerialResponse();
                if (response7 != null)
                {
                    response7 = response7.Trim();
                    radioButtonDigital7.Checked = response7.Equals("1");
                }
            }
            catch (TimeoutException)
            {
                labelStatus.Text = "Fout: Arduino antwoord timeout (geen respons van digitale pinnen)";
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout timer oefening 3: " + ex.Message;
            }
        }

        private string ReadSerialResponse()
        {
            try
            {
                // Wait a bit for response
                System.Threading.Thread.Sleep(50);
                
                if (serialPortArduino.BytesToRead > 0)
                {
                    return serialPortArduino.ReadLine();
                }
                return null;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        private void timerOefening4_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    return;
                }

                // Clear any existing data
                if (serialPortArduino.BytesToRead > 0)
                {
                    serialPortArduino.ReadExisting();
                }

                // Query and display analog pin 0
                serialPortArduino.WriteLine("get a0");
                string response = ReadSerialResponse();
                if (response != null)
                {
                    response = response.Trim();
                    response = response.Replace("a0:", "").Trim();
                    labelAnalog0.Text = response;
                }
            }
            catch (TimeoutException)
            {
                labelStatus.Text = "Fout: Arduino antwoord timeout (geen respons van analog0)";
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout timer oefening 4: " + ex.Message;
            }
        }

        private double GetDesiredTemperature(int rawValue)
        {
            // Scale 0..1023 to 5..45 °C
            double scaledValue = (rawValue / 1023.0) * 40 + 5;
            return Math.Round(scaledValue, 1);
        }

        private double GetCurrentTemperature(int rawValue)
        {
            // Scale 0..1023 to 0..500 °C
            double scaledValue = (rawValue / 1023.0) * 500;
            return Math.Round(scaledValue, 1);
        }

        private void timerOefening5_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    labelGewensteTemp.Text = "N/A";
                    labelHuidigeTemp.Text = "N/A";
                    return;
                }

                // Clear any existing data
                if (serialPortArduino.BytesToRead > 0)
                {
                    serialPortArduino.ReadExisting();
                }

                // Read desired temperature from analog pin 0
                serialPortArduino.WriteLine("get a0");
                string responseA0 = ReadSerialResponse();
                double desiredTemp = 0;
                if (responseA0 != null)
                {
                    responseA0 = responseA0.Trim();
                    responseA0 = responseA0.Replace("a0:", "").Trim();
                    if (int.TryParse(responseA0, out int rawA0))
                    {
                        desiredTemp = GetDesiredTemperature(rawA0);
                        labelGewensteTemp.Text = string.Format("{0:F1} °C", desiredTemp);
                    }
                }

                // Read current temperature from analog pin 1
                serialPortArduino.WriteLine("get a1");
                string responseA1 = ReadSerialResponse();
                double currentTemp = 0;
                if (responseA1 != null)
                {
                    responseA1 = responseA1.Trim();
                    responseA1 = responseA1.Replace("a1:", "").Trim();
                    if (int.TryParse(responseA1, out int rawA1))
                    {
                        currentTemp = GetCurrentTemperature(rawA1);
                        labelHuidigeTemp.Text = string.Format("{0:F1} °C", currentTemp);
                    }
                }

                // Control LED: turn on if current temp < desired temp
                if (currentTemp < desiredTemp)
                {
                    serialPortArduino.WriteLine("set d2 high");
                }
                else
                {
                    serialPortArduino.WriteLine("set d2 low");
                }
            }
            catch (TimeoutException)
            {
                labelStatus.Text = "Fout: Arduino antwoord timeout (geen respons van thermostaat sensoren)";
                labelGewensteTemp.Text = "N/A";
                labelHuidigeTemp.Text = "N/A";
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Fout timer oefening 5: " + ex.Message;
                labelGewensteTemp.Text = "N/A";
                labelHuidigeTemp.Text = "N/A";
            }
        }
    }
}
