using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WINMM_Timer;
using XML_File_Ops;

namespace WinUSBSecondtime
{
    public partial class Form1 : Form
    {
        #region classes

        public class USB_Xfer
        {
            /// <summary>
            /// The name of the command.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The numeric code representing the command.
            /// </summary>
            public byte CommandCode { get; set; }
            /// <summary>
            /// The number of bytes to send, excluding the command.
            /// </summary>
            public int NumSendBytes { get; set; }
            /// <summary>
            /// The number of bytes to receive, excluding the command echo.
            /// </summary>
            public int NumReceiveBytes { get; set; }

            private byte[] SendBytes;
            private byte[] ReceiveBytes;

            public USB_Xfer() { }

            public USB_Xfer(string name, byte commandCode, int numSendBytes, int numReceiveBytes)
            {
                Name = name;
                CommandCode = commandCode;
                NumSendBytes = numSendBytes;
                NumReceiveBytes = numReceiveBytes;
            }

            /// <summary>
            /// Returns the data that is ready to send to the USB device, throws an application exeption if the buffer is not initialized.
            /// </summary>
            /// <returns>byte array of the data to send.</returns>
            public byte[] GetSendBytes()
            {
                byte[] retByte;
                if (SendBytes != null)
                {
                    retByte = new byte[SendBytes.Count()];
                    for (int a = 0; a < SendBytes.Count(); a++)
                    {
                        retByte[a] = SendBytes[a];
                    }
                }
                else
                {
                    retByte = new byte[1];
                    retByte[0] = 0;
                    throw new ApplicationException("send buffer is not initialized, use SetSendBytes to initialize");
                }

                return retByte;
            }

            /// <summary>
            /// Initializes the send buffer, a byte array.
            /// </summary>
            /// <param name="sendBytes, a byte array that holds the data to send to the USB device."></param>
            public void SetSendBytes(byte[] sendBytes)
            {
                SendBytes = new byte[sendBytes.Count()];
                for (int a = 0; a < sendBytes.Count(); a++)
                {
                    SendBytes[a] = sendBytes[a];
                }
            }

            /// <summary>
            /// Returns the data received from the USB device, throws an application exception if the buffer is not initialized.
            /// </summary>
            /// <returns>A byte array filled with the data received from the USB device.</returns>
            public byte[] GetReceiveBytes()
            {
                byte[] retByte;
                if (ReceiveBytes != null)
                {
                    retByte = new byte[ReceiveBytes.Count()];
                    for (int a = 0; a < ReceiveBytes.Count(); a++)
                    {
                        retByte[a] = ReceiveBytes[a];
                    }
                }
                else
                {
                    retByte = new byte[1];
                    retByte[0] = 0;
                    throw new ApplicationException("receive butter is not initialized, use SetReceiveBytes to initialize");
                }

                return retByte;
            }

            /// <summary>
            /// Initializes the receive buffer, a byte array.
            /// </summary>
            /// <param name="receiveBytes, the data received from the USB device."></param>
            public void SetReceiveBytes(byte[] receiveBytes)
            {
                ReceiveBytes = new byte[receiveBytes.Count()];
                for (int a = 0; a < receiveBytes.Count(); a++)
                {
                    ReceiveBytes[a] = receiveBytes[a];
                }
            }
        }

        #endregion

        #region member variables

        private AccurateTimer captTimer;

        private StringBuilder toolBarXMLfileString = new StringBuilder(Application.StartupPath + "\\ToolBar.xml");


        internal Form1 thisForm;
        private const String GUID = "{3C24F757-EDC1-43D2-AD96-E18017F4E913}";
        private DeviceManagement DeviceManagement = new DeviceManagement();
        private IntPtr DeviceNotificationHandle;
        private Boolean DeviceDetected = false;
        private String DevicePathName;
        private WinUsbDevice Device = new WinUsbDevice();

        private USB_Xfer fwVersionXfer;
        private StringBuilder FWversion = new StringBuilder();

        private USB_Xfer getAnalog;
        private List<UInt16> analogValues = new List<UInt16> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private USB_Xfer getTemp;
        private List<UInt16> tempValues = new List<UInt16>();

        private USB_Xfer setLED;
        private Color LEDcolor = new Color();

        private USB_Xfer getCount;
        private List<UInt16> countValues = new List<UInt16>() { 0, 0 };

        private USB_Xfer comp_Cmd;
        private List<UInt16> complexValues = new List<UInt16>() { 0, 0 };

        private USB_Xfer set_led_bits;

        private const byte GET_VERSION = 0x10;
        private const byte GET_ANALOG = 0x11;
        private const byte GET_TEMP = 0x12;
        private const byte SET_LED = 0x13;
        private const byte GET_COUNTS = 0x14;
        private const byte SET_PWM = 0x15;
        private const byte SEND_FREQ = 0x16;
        private const byte COMP_CMD = 0x17;
        private const byte SET_LED_BITS = 0x18;

        private const UInt32 CHANGE_LED_COUNT = 5;
        private UInt32 ledChangeCounter = 0;
        private byte ledState = 0;

        PictureBox LED1 = new PictureBox();
        PictureBox LED2 = new PictureBox();
        PictureBox LED3 = new PictureBox();
        PictureBox LED4 = new PictureBox();

        // forms
        FormGraph formGraph = new FormGraph();
        FormLineFormat formLineFormat = new FormLineFormat();

        private string[] combo1dropdown = new string[4] { "0", "1", "2", "3" };
        private string[] comboboxdrop1 = new string[4] { "1", "10", "100", "1000" };

        private UInt32 timeCounter = 0;
        private UInt32 countSum = 0;

        private bool compbutton = false;
        private bool buttoncounter = false;

        #endregion

        public Form1()
        {
            InitializeComponent();

            ComplexComboBox1.Items.Clear();
            ComplexComboBox1.Items.AddRange(combo1dropdown);

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(comboboxdrop1);

            // dock the tool strip panels
            toolStripPanelTop.Dock = DockStyle.Top;
            toolStripPanelLeft.Dock = DockStyle.Left;
            toolStripPanelRight.Dock = DockStyle.Right;
            toolStripPanelBottom.Dock = DockStyle.Bottom;

            // if the ToolBar XML file doesn't exist create a default
            if (File.Exists(toolBarXMLfileString.ToString()) == false)
            {
                XMLFileOps.createNewToolBarXML(toolBarXMLfileString.ToString());
            }

            // add the status strip to the bottom tool strip panel
            toolStripPanelBottom.Join(statusStripStatus);

            // read the position of the tool strip from the XML file
            ToolBarNode tbn = new ToolBarNode();
            XMLFileOps.readToolBarXML(out tbn, toolBarXMLfileString.ToString(), "Tools");
            // join the tool strip to the appropiate tool strip panel          
            if (tbn.dock == "Left") toolStripPanelLeft.Join(toolStripTools);
            else if (tbn.dock == "Right") toolStripPanelRight.Join(toolStripTools);
            else if (tbn.dock == "Bottom") toolStripPanelBottom.Join(toolStripTools);
            else toolStripPanelTop.Join(toolStripTools);// the default panel is top

            // add the menu to the top tool strip panel
            toolStripPanelTop.Join(menuStripMain);
            
            captTimer = new AccurateTimer(this, new Action(TimerCallBack), 100);

            thisForm = this;

            fwVersionXfer = new USB_Xfer()
            {
                Name = "GetVersion",
                CommandCode = GET_VERSION,
                NumSendBytes = 0,
                NumReceiveBytes = 2
            };
            byte[] send = new byte[1] { GET_VERSION };
            fwVersionXfer.SetSendBytes(send);

            getAnalog = new USB_Xfer()
            {
                Name = "GetAnalog",
                CommandCode = GET_ANALOG,
                NumSendBytes = 0,
                NumReceiveBytes = 20
            };
            send[0] = GET_ANALOG;
            getAnalog.SetSendBytes(send);

            getCount = new USB_Xfer()
            {
                Name = "GetCount",
                CommandCode = GET_COUNTS,
                NumSendBytes = 0,
                NumReceiveBytes = 20
            };
            send[0] = GET_COUNTS;
            getCount.SetSendBytes(send);

            getTemp = new USB_Xfer()
            {
                Name = "getTemp",
                CommandCode = GET_TEMP,
                NumSendBytes = 0,
                NumReceiveBytes = 2,
            };

            setLED = new USB_Xfer()
            {
                Name = "SetLED",
                CommandCode = SET_LED,
                NumSendBytes = 1,
                NumReceiveBytes = 0
            };

            comp_Cmd = new USB_Xfer()
            {
                Name = "Comp_CMD",
                CommandCode = COMP_CMD,
                NumSendBytes = 1,
                NumReceiveBytes = 4
            };

            set_led_bits = new USB_Xfer()
            {
                Name = "Set_LED_Bits",
                CommandCode = SET_LED_BITS,
                NumSendBytes = 4,
                NumReceiveBytes = 1
            };

            LED4.Height = 22;
            LED4.Width = 29;
            LED4.BackColor = Color.Black;
            LED4.Cursor = Cursors.Default;
            LED4.BorderStyle = BorderStyle.Fixed3D;
            statusStripStatus.Items.Add(new ToolStripControlHost(LED4));

            LED3.Height = 22;
            LED3.Width = 29;
            LED3.BackColor = Color.Black;
            LED3.Cursor = Cursors.Default;
            LED3.BorderStyle = BorderStyle.Fixed3D;
            statusStripStatus.Items.Add(new ToolStripControlHost(LED3));

            LED2.Height = 22;
            LED2.Width = 29;
            LED2.BackColor = Color.Black;
            LED2.Cursor = Cursors.Default;
            LED2.BorderStyle = BorderStyle.Fixed3D;
            statusStripStatus.Items.Add(new ToolStripControlHost(LED2));

            LED1.Height = 22;
            LED1.Width = 29;
            LED1.BackColor = Color.Black;
            LED1.Cursor = Cursors.Default;
            LED1.BorderStyle = BorderStyle.Fixed3D;
            statusStripStatus.Items.Add(new ToolStripControlHost(LED1));

            LEDcolor = Color.DeepSkyBlue;

            FormClosing += Form1_FormClosing;

            formGraph.OpenLineFormatEvent += FormGraph_OpenLineFormatEvent;
            formGraph.OpenGridFormatEvent += FormGraph_OpenGridFormatEvent;
            formGraph.OpenTimeFormatEvent += FormGraph_OpenTimeFormatEvent;

            formLineFormat.OpenLineEvent += FormLineFormat_OpenLineEvent;
            formLineFormat.OpenGridEvent += FormLineFormat_OpenGridEvent;
            formLineFormat.OpenTimeEvent += FormLineFormat_OpenTimeEvent;
        }

        #region Consumer Methods
        private void FormGraph_OpenLineFormatEvent(object sender, FormGraph.OpenLineFormatEventArgs hvsa)
        {
            formLineFormat.Visible = true;
            formLineFormat.BringToFront();
            formLineFormat.setLines(hvsa._lines);
        }

        private void FormGraph_OpenTimeFormatEvent(object sender, FormGraph.OpenTimeFormatEventArgs mvsa)
        {
            formLineFormat.Visible = true;
            formLineFormat.BringToFront();
            formLineFormat.setTime(mvsa._times);
        }

        private void FormGraph_OpenGridFormatEvent(object sender, FormGraph.OpenGridFormatEventArgs gvsa)
        {
            formLineFormat.Visible = true;
            formLineFormat.BringToFront();
            formLineFormat.setgrid(gvsa._grids);
        }

        private void FormLineFormat_OpenTimeEvent(object sender, FormLineFormat.OpenTimeEventArgs nvsa)
        {
            formGraph.UpdateTime(nvsa._someTime);

        }

        private void FormLineFormat_OpenGridEvent(object sender, FormLineFormat.OpenGridEventArgs jvsa)
        {
            formGraph.UpdateGrid(jvsa._someGrid);
        }

        private void FormLineFormat_OpenLineEvent(object sender, FormLineFormat.OpenLineEventArgs dvsa)
        {
            formGraph.UpdateLines(dvsa._somelines);
        }
        #endregion

        #region TimerCallback

        private void TimerCallBack()
        {
            UInt16[] rawData = new UInt16[3];

            DeviceDetected = FindDevice();
            if (DeviceDetected == true)
            {
                if (FWversion.Length == 0)
                {
                    if (USBxfer(fwVersionXfer) == true)
                    {
                        byte[] r = fwVersionXfer.GetReceiveBytes();
                        FWversion.Append(r[0].ToString() + "." + r[1].ToString());
                        toolStripStatusLabelCon.Text = "FW Version: " + FWversion.ToString();
                    }
                }

                if (USBxfer(getAnalog) == true)
                {
                    byte[] aVals = getAnalog.GetReceiveBytes();
                    for (int i = 0, o = 0; i < analogValues.Count; i++, o += 2)
                    {
                        analogValues[i] = aVals[o];
                        analogValues[i] += (UInt16)(analogValues[i] >> 8);
                    }
                    AnalogTextBox.Text = String.Empty;
                    UInt16 sum = 0;
                    for (int i = 0; i < analogValues.Count; i++)
                    {
                        sum += analogValues[i];
                    }
                    sum /= 10;
                    AnalogTextBox.Text = sum.ToString();
                    rawData[0] = sum;
                    USBxfer(getCount);
                    byte[] cnts = getCount.GetReceiveBytes();

                    countValues[0] = (UInt16)(cnts[0] * 256);
                    countValues[0] += cnts[1];
                    countValues[1] = (UInt16)(cnts[2] * 256);
                    countValues[1] += cnts[3];
                    textBoxCnt1.Text = countValues[0].ToString();
                    textBoxCnt2.Text = countValues[1].ToString();
                    rawData[1] = countValues[0];
                    rawData[2] = countValues[1];
                    formGraph.UpdateData(rawData);
                }

                if (buttoncounter == true) {
                    if (USBxfer(getCount) == true)
                    {
                        byte[] cnts = getCount.GetReceiveBytes();

                        countValues[0] = (UInt16)(cnts[0] * 256);
                        countValues[0] += cnts[1];
                        countValues[1] = (UInt16)(cnts[2] * 256);
                        countValues[1] += cnts[3];
                        formGraph.UpdateData(rawData);
                        textBoxCnt1.Text = countValues[0].ToString();
                        textBoxCnt2.Text = countValues[1].ToString();
                        rawData[1] = countValues[0];
                        rawData[2] = countValues[1];
                        countSum += countValues[1];
                        textBox2.Text += countValues[1].ToString() + Environment.NewLine;
                        if (++timeCounter >= Convert.ToUInt32(comboBox1.SelectedItem))
                        {
                            countSum /= timeCounter;
                            textBox1.Text = countSum.ToString();
                            countSum = 0;
                            timeCounter = 0;
                            buttoncounter = false;
                        }
                        label3.Text = timeCounter.ToString();
                    }
                }

                if (checkBox5.Checked == false)
                {
                    if (++ledChangeCounter == CHANGE_LED_COUNT)
                    {
                        ledChangeCounter = 0;
                        byte[] states = new byte[1];
                        states[0] = ledState;
                        setLED.SetSendBytes(states);
                        USBxfer(setLED);

                        Int32 mask = ledState & 0x0001;
                        if (mask == 1) LED1.BackColor = LEDcolor;
                        else LED1.BackColor = Color.Black;
                        mask = ledState & 0x0002;
                        if (mask >> 1 == 1) LED2.BackColor = LEDcolor;
                        else LED2.BackColor = Color.Black;
                        mask = ledState & 0x0004;
                        if (mask >> 2 == 1) LED3.BackColor = LEDcolor;
                        else LED3.BackColor = Color.Black;
                        mask = ledState & 0x0008;
                        if (mask >> 3 == 1) LED4.BackColor = LEDcolor;
                        else LED4.BackColor = Color.Black;

                        if (ledState++ == 15) ledState = 0;
                    }
                }
                else
                {
                    byte[] lednum = new byte[4];
                    if (checkBox1.Checked == true)
                    {
                        lednum[0] = 1;
                        LED1.BackColor = LEDcolor;
                    }
                    else
                    {
                        lednum[0] = 0;
                        LED1.BackColor = Color.Black;
                    }
                    if (checkBox2.Checked == true)
                    {
                        lednum[1] = 1;
                        LED2.BackColor = LEDcolor;
                    }
                    else
                    {
                        lednum[1] = 0;
                        LED2.BackColor = Color.Black;
                    }
                    if (checkBox3.Checked == true)
                    {
                        lednum[2] = 1;
                        LED3.BackColor = LEDcolor;
                    }
                    else
                    {
                        lednum[2] = 0;
                        LED3.BackColor = Color.Black;
                    }
                    if (checkBox4.Checked == true)
                    {
                        lednum[3] = 1;
                        LED4.BackColor = LEDcolor;
                    }
                    else
                    {
                        lednum[3] = 0;
                        LED4.BackColor = Color.Black;
                    }
                    set_led_bits.SetSendBytes(lednum);
                    USBxfer(set_led_bits);
                }

                if (compbutton == true)
                {
                    compbutton = false;

                    byte[] complexcommand = new byte[1];
                    if (ComplexComboBox1.SelectedIndex != -1)
                    {
                        complexcommand[0] = (byte)ComplexComboBox1.SelectedIndex;


                    }
                    else
                        complexcommand[0] = 0;
                    comp_Cmd.SetSendBytes(complexcommand);

                    if (USBxfer(comp_Cmd) == true)
                    {
                        byte[] complexcommand1 = comp_Cmd.GetReceiveBytes();
                        ComplexTextBox1.Text = complexcommand1[0].ToString();
                        UInt16 Something = complexcommand1[1];
                        Something += (UInt16)(complexcommand1[2] * 256);
                        ComplexTextBox2.Text = Something.ToString();
                    }
                }
            }
            else
            {
                FWversion.Clear();
                toolStripStatusLabelCon.Text = "Device not found.";
            }

        }

        #endregion

        #region form events

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            captTimer.Stop();
        }

        #endregion

        #region USB comms

        private bool USBxfer(USB_Xfer xferData)
        {
            bool commOK = false;

            try
            {
                byte[] inBuffer = new byte[64];
                byte[] outBuffer = new byte[64];

                outBuffer[0] = xferData.CommandCode;
                byte[] s = xferData.GetSendBytes();
                if (xferData.NumSendBytes == 1)
                {
                    outBuffer[1] = s[0];
                }
                else
                {
                    // for (int a = 1; a < xferData.NumSendBytes + 1; a++)
                    for (int a = 1; a < xferData.NumSendBytes; a++)
                    {
                        outBuffer[a] = s[a - 1];
                    }
                }

                commOK = USBcomm(Device,
                                 DeviceDetected,
                                 Convert.ToUInt32(xferData.NumSendBytes + 1),// number of bytes to send
                                 Convert.ToUInt32(xferData.NumReceiveBytes + 1),// number of bytes to receive
                                 outBuffer,
                                 out inBuffer);

                if ((commOK == true) && (inBuffer[0] == xferData.CommandCode))
                {
                    byte[] r = new byte[xferData.NumReceiveBytes];
                    for (int a = 1; a < inBuffer.Count(); a++)
                    {
                        r[a - 1] = inBuffer[a];
                    }
                    xferData.SetReceiveBytes(r);
                }
            }
            catch (Exception e)
            {
                commOK = false;
                captTimer.Stop();
                MessageBox.Show(e.Message,
                                "Error in USBxfer",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }

            return commOK;

        }

        private bool getFWVersion(out string version)
        {
            bool commOK = false;
            byte[] inBuffer = new byte[64];
            byte[] outBuffer = new byte[64];

            outBuffer[0] = GET_VERSION;

            commOK = USBcomm(Device,
                             DeviceDetected,
                             Convert.ToUInt32(1),
                             Convert.ToUInt32(3),
                             outBuffer,
                             out inBuffer);

            if ((commOK == true) && (inBuffer[0] == GET_VERSION))
                version = inBuffer[1].ToString() + "." + inBuffer[2].ToString();
            else version = "";

            return commOK;

        }

        #endregion

        #region primitive USB comms

        internal void OnDeviceChange(Message m)
        {
            try
            {
                if ((m.WParam.ToInt32() == DeviceManagement.DBT_DEVICEARRIVAL))
                {//  If WParam contains DBT_DEVICEARRIVAL, a device has been attached.
                    //  Find out if it's the device we're communicating with.

                    if (DeviceManagement.DeviceNameMatch(m, DevicePathName))
                    {//it is our device 

                    }

                }
                else if ((m.WParam.ToInt32() ==
                    DeviceManagement.DBT_DEVICEREMOVECOMPLETE))
                {//  If WParam contains DBT_DEVICEREMOVAL, a device has been removed.
                    //  Find out if it's the device we're communicating with.

                    if (DeviceManagement.DeviceNameMatch(m, DevicePathName))
                    {//  Set Detected false so on the next data-transfer attempt,
                        //  findFSUSB() will be called to look for the device 
                        //  and get a new handle.                                                
                        DeviceDetected = false;
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error in OnDeviceChange",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Boolean FindDevice()
        {
            Boolean deviceFound;
            String devicePathName = "";
            Boolean success;

            try
            {
                if (DeviceDetected == false)
                {
                    //  Convert the device interface GUID String to a GUID object: 
                    System.Guid newGUID =
                        new System.Guid(GUID);
                    // Fill an array with the device path names of all attached devices with matching GUIDs.
                    deviceFound = DeviceManagement.FindDeviceFromGuid
                        (newGUID, ref devicePathName);

                    if (deviceFound == true)
                    {
                        success = Device.GetDeviceHandle(devicePathName);

                        if (success == true)
                        {
                            DeviceDetected = true;
                            // Save DevicePathName so OnDeviceChange() knows which name is my device.
                            DevicePathName = devicePathName;
                        }
                        else
                        {
                            // There was a problem in retrieving the information.
                            DeviceDetected = false;
                            Device.CloseDeviceHandle();
                        }
                    }

                    if (DeviceDetected == true)
                    {
                        // The device was detected.
                        // Register to receive notifications if the device is removed or attached.
                        success = DeviceManagement.RegisterForDeviceNotifications
                            (DevicePathName,
                            thisForm.Handle,
                            newGUID,
                            ref DeviceNotificationHandle);
                        if (success == true)
                        {
                            Device.InitializeDevice();
                            //Commented out due to unreliable response from WinUsb_QueryDeviceInformation.                            
                            //DisplayDeviceSpeed(); 
                        }
                    }
                }
                return DeviceDetected;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool USBcomm(WinUsbDevice device, Boolean connected, UInt32 bytesToWrite, UInt32 bytesToRead, byte[] outData, out byte[] inData)
        {
            Boolean writeSuccess = false, readSuccess = false, funcSuccess = false;
            UInt32 bytesToSend = bytesToWrite;
            UInt32 bytesRead = 0;
            Byte[] writeBuffer;
            Byte[] readBuffer = new Byte[bytesToRead];

            try
            {
                writeBuffer = outData;//put the command in the buffer                                 

                if (connected == true)
                {
                    //send the command to the reader
                    writeSuccess = device.SendViaBulkTransfer(ref writeBuffer, bytesToSend);
                    if (writeSuccess == true)
                    {//if the command was sent successfully read the return data 
                        device.ReadViaBulkTransfer(device.myDevInfo.bulkInPipe,
                                                   bytesToRead,
                                                   ref readBuffer,
                                                   ref bytesRead,
                                                   ref readSuccess);

                        if (readSuccess == true)
                        {//if the read was successful the function is successful
                            funcSuccess = true;
                        }
                        else
                        {//if the read was unsuccessful the function is unsuccessful
                            funcSuccess = false;
                        }
                    }
                    else
                    {//if the write was unsuccessful the function is unsuccessful
                        funcSuccess = false;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error in USBcomm",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                funcSuccess = false;

            }

            inData = readBuffer;
            return funcSuccess;

        }

        protected override void WndProc(ref Message m)
        {//check for USB device change message
            try
            {
                if (m.Msg == DeviceManagement.WM_DEVICECHANGE)
                {//process WM_DEVICECHANGE messages.
                    OnDeviceChange(m);
                }
                // Let the base form process the message.
                base.WndProc(ref m);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error in WndProc",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            compbutton = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            buttoncounter = true;
            textBox2.Text = string.Empty;
        }

        private void openFormGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFormGraph();
        }

        private void OpenFormGraph()
        {
            formGraph.Visible = true;
            formGraph.BringToFront();
        }
    }
}
