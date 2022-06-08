using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using GDIDB;
using GraphLine;


namespace WinUSBSecondtime
{
    public partial class FormLineFormat : Form
    {

        #region member variables

        private String[] lineDropDown = new string[] { "Analog", "Count1", "Count2" };
        private String[] numberDropDown = new string[] { "1", "2", "3", "4", "5", "6" };
        private String[] styleDropDown = new string[] { "Solid", "Dash", "Dot" , "DashDot", "DashDotDot"};

        private Line[] lineArray = new Line[3] {
                                                 new Line(0, 1023, "Analog", Color.Black, DashStyle.Solid, 1, true),
                                                 new Line(0, 2000, "Count 1", Color.Blue, DashStyle.Dash, 1, true),
                                                 new Line(0, 2000, "Count 2", Color.Green, DashStyle.Solid, 1, true)
                                               };

        private int[] grid = new int[] { 4, 5};

        private int[] UpTime = new int[] { 0, 2, 0, 1};

        private Line demoLine = new Line(0, 1023, "demo", Color.Black, DashStyle.Solid, 1, true);


        private DBGraphics picBoxColor = new DBGraphics();
        private DBGraphics picBoxLineStyle = new DBGraphics();

        private Color demoColor = new Color();

        public delegate void OpenLineEventHandler(object sender, OpenLineEventArgs dvsa);
        public event OpenLineEventHandler OpenLineEvent;

        public delegate void OpenGridEventHandler(object sender, OpenGridEventArgs jvsa);
        public event OpenGridEventHandler OpenGridEvent;

        public delegate void OpenTimeEventHandler(object sender, OpenTimeEventArgs nvsa);
        public event OpenTimeEventHandler OpenTimeEvent;

        #endregion

        public FormLineFormat()
        {
            InitializeComponent();

            comboBoxLine.Items.Clear();
            comboBoxLine.Items.AddRange(lineDropDown);
            comboBoxStyle.Items.Clear();
            comboBoxStyle.Items.AddRange(styleDropDown);
            comboBoxWidth.Items.Clear();
            comboBoxWidth.Items.AddRange(numberDropDown);

            picBoxColor.CreateDoubleBuffer(pictureBoxColor.CreateGraphics(),
                                           pictureBoxColor.Width,
                                           pictureBoxColor.Height);
            picBoxColor.g.SmoothingMode = SmoothingMode.HighQuality;

            picBoxColor.g.Clear(Color.Black);

            picBoxLineStyle.CreateDoubleBuffer(pictureBoxStyle.CreateGraphics(),
                                                pictureBoxStyle.Width,
                                                pictureBoxStyle.Height);
            picBoxLineStyle.g.Clear(Color.White);
            picBoxLineStyle.g.SmoothingMode = SmoothingMode.HighQuality;


            // events
            textBoxMaxValue.KeyPress += TextBoxMaxValue_KeyPress;
            textBoxMinValue.KeyPress += TextBoxMinValue_KeyPress;

            textBoxTimeMax.KeyPress += TextBoxTimeMax_KeyPress;
            textBoxTimeMin.KeyPress += TextBoxTimeMin_KeyPress;

            comboBoxStyle.SelectedIndexChanged += ComboBoxStyle_SelectedIndexChanged;
            comboBoxWidth.SelectedIndexChanged += ComboBoxWidth_SelectedIndexChanged;

            pictureBoxColor.Click += PictureBoxColor_Click;
            pictureBoxColor.Paint += PictureBoxColor_Paint;
            pictureBoxStyle.Paint += PictureBoxStyle_Paint;

            FormClosing += FormLineFormat_FormClosing;
            Load += FormLineFormat_Load;
            comboBoxLine.SelectedIndexChanged += ComboBoxLine_SelectedIndexChanged;
        }

        #region comboBox events
        private void ComboBoxLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxMaxValue.Text = lineArray[comboBoxLine.SelectedIndex].maxVal.ToString();
            textBoxMinValue.Text = lineArray[comboBoxLine.SelectedIndex].minVal.ToString();
            comboBoxStyle.SelectedIndex = (int)lineArray[comboBoxLine.SelectedIndex].lineStyle;
            comboBoxWidth.SelectedIndex = (int)lineArray[comboBoxLine.SelectedIndex].lineWidth - 1;
            Color tempColor = lineArray[comboBoxLine.SelectedIndex].lineColor;
            picBoxColor.g.Clear(tempColor);
            pictureBoxColor.BackColor = lineArray[comboBoxLine.SelectedIndex].lineColor;
            pictureBoxColor.Invalidate(true);
            checkBoxInclude.Checked = lineArray[comboBoxLine.SelectedIndex].showOnGraph;
            textBoxTimeMax.Text = "4";
            textBoxTimeMin.Text = "5";
            numericUpDownHrs.Value = 0;
            numericUpDownMin.Value = 2;
            numericUpDownSec.Value = 0;
            checkBoxConstantTime.Checked = true;
        }

        private void ComboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDemoLine();
        }

        private void ComboBoxWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDemoLine();
        }

        #endregion

        #region private methods

        private void UpdateDemoLine()
        {
            try
            {
                demoLine.lineColor = pictureBoxColor.BackColor;
                demoLine.lineWidth = (uint)comboBoxWidth.SelectedIndex + 1;
                demoLine.maxVal = Convert.ToInt32(textBoxMaxValue.Text);
                demoLine.minVal = Convert.ToInt32(textBoxMinValue.Text);
                demoLine.showOnGraph = !checkBoxInclude.Checked;
                grid[0] = 4;
                grid[1] = 5;
                picBoxLineStyle.g.SetClip(pictureBoxStyle.ClientRectangle);
                picBoxLineStyle.g.Clear(Color.White);

                Pen tempPen = new Pen(demoLine.lineColor, demoLine.lineWidth);
                switch (comboBoxStyle.SelectedIndex)
                {
                    case 0:
                        tempPen.DashStyle = DashStyle.Solid;
                        break;
                    case 1:
                        tempPen.DashStyle = DashStyle.Dash;
                        break;
                    case 2:
                        tempPen.DashStyle = DashStyle.Dot;
                        break;
                    case 3:
                        tempPen.DashStyle = DashStyle.DashDot;
                        break;
                    case 4:
                        tempPen.DashStyle |= DashStyle.DashDotDot;
                        break;
                }

                tempPen.Width = comboBoxWidth.SelectedIndex + 1;

                picBoxLineStyle.g.DrawLine(tempPen,
                                           0,
                                           pictureBoxStyle.Height / 2,
                                           pictureBoxStyle.Width,
                                           pictureBoxStyle.Height / 2);
                tempPen.Dispose();
                pictureBoxStyle.Invalidate(true);
            }
            catch
            {
                string message = "Sorry, Please enter a Line to use.";
                string title = "Close Window";
                MessageBoxButtons buttons = MessageBoxButtons.RetryCancel;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                {
                    this.Close();
                }
            }
        }

        #endregion

        #region public methods

        public void setLines(Line[] setNewLines)
        {
            if (setNewLines.Length == 3)
            {
                for (int i = 0; i < setNewLines.Length; i++)
                {
                    lineArray[i].lineStyle = setNewLines[i].lineStyle;
                    lineArray[i].lineColor = setNewLines[i].lineColor;
                    lineArray[i].lineWidth = setNewLines[i].lineWidth;
                    lineArray[i].showOnGraph = setNewLines[i].showOnGraph;
                    lineArray[i].maxVal = setNewLines[i].maxVal;
                    lineArray[i].minVal = setNewLines[i].minVal;
                }
            }
        }

        public void setgrid(int[] setnewgrid)
        {
            grid[0] = setnewgrid[0];
            grid[1] = setnewgrid[1];
        }

        public void setTime(int[] setnewTime)
        {
            UpTime[0] = setnewTime[0];
            UpTime[1] = setnewTime[1];
            UpTime[2] = setnewTime[2];
            UpTime[3] = setnewTime[3];
        }
        #endregion

        #region textBox events

        private void TextBoxMinValue_KeyPress(object sender, KeyPressEventArgs e)
        {//allow only numbers and the backspace in the textbox

            if ((char.IsNumber(e.KeyChar) == false) && (e.KeyChar != (char)Keys.Back))
            {
                e.Handled = true;
            }
        }

        private void TextBoxMaxValue_KeyPress(object sender, KeyPressEventArgs e)
        {//allow only numbers and the backspace in the textbox

            if ((char.IsNumber(e.KeyChar) == false) && (e.KeyChar != (char)Keys.Back))
            {
                e.Handled = true;
            }
        }

        private void TextBoxTimeMax_KeyPress(object sender, KeyPressEventArgs e)
        {
            //allow only numbers, 1 decimal point, and the backspace in the textboxes preserve copy and paste
            if ((e.KeyChar != 22) && // CNTRL + v
                (e.KeyChar != 24) && // CNTRL + x
                (e.KeyChar != 3))    // CNTRL + c
            {

                if (textBoxTimeMax.Text.Contains(":") == true)
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != 8))
                        e.Handled = true;
                }
                else
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != ':') && (e.KeyChar != 8))
                        e.Handled = true;
                }
            }
        }

        private void TextBoxTimeMin_KeyPress(object sender, KeyPressEventArgs e)
        {
            //allow only numbers, 1 decimal point, and the backspace in the textboxes preserve copy and paste
            if ((e.KeyChar != 22) && // CNTRL + v
                (e.KeyChar != 24) && // CNTRL + x
                (e.KeyChar != 3))    // CNTRL + c
            {

                if (textBoxTimeMax.Text.Contains(":") == true)
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != 8))
                        e.Handled = true;
                }
                else
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != ':') && (e.KeyChar != 8))
                        e.Handled = true;
                }
            }
        }
        #endregion

        #region pictureBox events

        private void PictureBoxColor_Click(object sender, EventArgs e)
        {
            ColorDialog cl = new ColorDialog();

            if(cl.ShowDialog() == DialogResult.OK)
            {
                demoColor = cl.Color;
                pictureBoxColor.BackColor = cl.Color;
                picBoxColor.g.Clear(demoColor);
                pictureBoxColor.Invalidate(true);
            }

        }

        private void PictureBoxColor_Paint(object sender, PaintEventArgs e)
        {
            if (picBoxColor.CanDoubleBuffer() == true) picBoxColor.Render(e.Graphics);
            else
            {
                picBoxColor.CreateDoubleBuffer(pictureBoxColor.CreateGraphics(),
                                               pictureBoxColor.ClientRectangle.Width,
                                               pictureBoxColor.ClientRectangle.Height
                                              );
            }
        }

        private void PictureBoxStyle_Paint(object sender, PaintEventArgs e)
        {
            if (picBoxLineStyle.CanDoubleBuffer() == true) picBoxLineStyle.Render(e.Graphics);
            else
            {
                picBoxLineStyle.CreateDoubleBuffer(pictureBoxStyle.CreateGraphics(),
                                                   pictureBoxStyle.ClientRectangle.Width,
                                                   pictureBoxStyle.ClientRectangle.Height
                                                  );
            }
        }

        #endregion

        #region form events

        private void FormLineFormat_Load(object sender, EventArgs e)
        {
           
        }

        private void FormLineFormat_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;//cancel close if user requested
            PropertyInfo pi = typeof(Form).GetProperty("CloseReason", BindingFlags.NonPublic | BindingFlags.Instance);
            pi.SetValue(this, CloseReason.None, null);
        }


        #endregion

        #region button events
        private void buttonApply_Click(object sender, EventArgs e)
        {
            try
            {
                lineArray[comboBoxLine.SelectedIndex].lineColor = pictureBoxColor.BackColor;
                lineArray[comboBoxLine.SelectedIndex].lineWidth = (uint)comboBoxWidth.SelectedIndex + 1;
                lineArray[comboBoxLine.SelectedIndex].showOnGraph = checkBoxInclude.Checked;
                lineArray[comboBoxLine.SelectedIndex].maxVal = Convert.ToInt32(textBoxMaxValue.Text);
                lineArray[comboBoxLine.SelectedIndex].minVal = Convert.ToInt32(textBoxMinValue.Text);
                lineArray[comboBoxLine.SelectedIndex].lineStyle = (DashStyle)(uint)comboBoxStyle.SelectedIndex;


                grid[0] = Convert.ToInt32(textBoxTimeMax.Text);
                grid[1] = Convert.ToInt32(textBoxTimeMin.Text);

                UpTime[0] = (Int32)numericUpDownHrs.Value;
                UpTime[1] = (Int32)numericUpDownMin.Value;
                UpTime[2] = (Int32)numericUpDownSec.Value;

                if (checkBoxConstantTime.Checked == false)
                {
                    UpTime[3] = 0;
                }
                else UpTime[3] = 1;

                OpenLineEventArgs alea = new OpenLineEventArgs(lineArray);
                OpenGridEventArgs flea = new OpenGridEventArgs(grid);
                OpenTimeEventArgs plea = new OpenTimeEventArgs(UpTime);


                OnOpenLineEvent(alea);
                OnOpenGridEvent(flea);
                OnOpenTimeEvent(plea);
            }
            catch
            {
                string message = "Sorry, Please enter a Line to use.";
                string title = "Close Window";
                MessageBoxButtons buttons = MessageBoxButtons.RetryCancel;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                {
                    this.Close();
                }
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Visible = false;
            PropertyInfo pi = typeof(Form).GetProperty("CloseReason", BindingFlags.NonPublic | BindingFlags.Instance);
            pi.SetValue(this, CloseReason.None, null);
        }

        private void buttonDefault_Click(object sender, EventArgs e)
        {
            try
            {
                lineArray[comboBoxLine.SelectedIndex].lineColor = Color.Black;
                lineArray[comboBoxLine.SelectedIndex].lineWidth = 1;
                lineArray[comboBoxLine.SelectedIndex].showOnGraph = true;
                lineArray[comboBoxLine.SelectedIndex].maxVal = 1000;
                lineArray[comboBoxLine.SelectedIndex].minVal = 0;



                ComboBoxLine_SelectedIndexChanged(sender, e);

                grid[0] = 4;
                grid[1] = 5;
                UpTime[0] = 0;
                UpTime[1] = 2;
                UpTime[2] = 0;
                UpTime[3] = 1;
                OpenLineEventArgs alea = new OpenLineEventArgs(lineArray);
                OpenGridEventArgs flea = new OpenGridEventArgs(grid);
                OpenTimeEventArgs plea = new OpenTimeEventArgs(UpTime);


                OnOpenLineEvent(alea);
                OnOpenGridEvent(flea);
                OnOpenTimeEvent(plea);
            }
            catch
            {
                string message = "Sorry, Please enter a Line to use.";
                string title = "Close Window";
                MessageBoxButtons buttons = MessageBoxButtons.RetryCancel;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                {
                    this.Close();
                }
            }
        }
        #endregion

        #region LineEvent
        protected void OnOpenLineEvent(OpenLineEventArgs alea)
        {
            OpenLineEvent?.Invoke(this, alea);
        }

        protected void OnOpenGridEvent(OpenGridEventArgs flea)
        {
            OpenGridEvent?.Invoke(this, flea);
        }

        protected void OnOpenTimeEvent(OpenTimeEventArgs plea)
        {
            OpenTimeEvent?.Invoke(this, plea);
        }

        public class OpenLineEventArgs : EventArgs
        {
            
           public OpenLineEventArgs(Line[] somelines)
           {
                _somelines = somelines;
           }
           public Line[] _somelines;
        }

        public class OpenGridEventArgs : EventArgs
        {

            public OpenGridEventArgs(int[] someGrid)
            {
                _someGrid = someGrid;
            }
            public int[] _someGrid;
        }

        public class OpenTimeEventArgs : EventArgs
        {

            public OpenTimeEventArgs(int[] someTime)
            {
                _someTime = someTime;
            }
            public int[] _someTime;
        }
        #endregion
    }
}