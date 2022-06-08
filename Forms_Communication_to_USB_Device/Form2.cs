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
    public partial class Form2 : Form
    {
        #region member variables

        private String[] shapedropdown = new string[4] { "Rectangle", "Circle", "Triangle", "line" };
        private String[] drawingdropdown = new string[] { "drawsolid", "drawdot", "drawdashdot" };

        private DBGraphics drawSomething = new DBGraphics();

        #endregion

        public Form2()
        {
            InitializeComponent();

            comboBoxShapes.Items.Clear();
            comboBoxShapes.Items.AddRange(shapedropdown);
            comboBoxDrawing.Items.Clear();
            comboBoxDrawing.Items.AddRange(drawingdropdown);

            drawSomething.CreateDoubleBuffer(pictureBox1.CreateGraphics(), pictureBox1.ClientRectangle.Width, pictureBox1.Height);
            drawSomething.g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // events
            textBoxX1.KeyPress += TextBoxX1_KeyPress;
            textBoxY1.KeyPress += TextBoxY1_KeyPress;
            textBoxX2.KeyPress += TextBoxX2_KeyPress;
            textBoxY2.KeyPress += TextBoxY2_KeyPress;

            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.Resize += PictureBox1_Resize;

            FormClosing += Form2_FormClosing;
        }

        #region form2closing/load
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;//cancel close if user requested
            PropertyInfo pi = typeof(Form).GetProperty("CloseReason", BindingFlags.NonPublic | BindingFlags.Instance);
            pi.SetValue(this, CloseReason.None, null);
        }


        private void Form2_Load(object sender, EventArgs e)
        {
            drawSomething.g.Clear(Color.White);

            InitGraphics();
            RefreshPictureBox();

        }

        #endregion

        #region pictureboxEvent
        private void PictureBox1_Resize(object sender, EventArgs e)
        {
            drawSomething.CreateDoubleBuffer(pictureBox1.CreateGraphics(),
                                     pictureBox1.ClientRectangle.Width,
                                     pictureBox1.ClientRectangle.Height);

            RefreshPictureBox();
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (drawSomething.CanDoubleBuffer() == true) drawSomething.Render(e.Graphics);
            else
            {
                drawSomething.CreateDoubleBuffer(pictureBox1.CreateGraphics(),
                                      10,//pictureBoxGraph.ClientRectangle.Width,
                                      10//pictureBoxGraph.ClientRectangle.Height
                                        );

                RefreshPictureBox();
            }
        }

        private void InitGraphics()
        {
            if (drawSomething.g == null)
            {
                do
                {
                    drawSomething.CreateDoubleBuffer(pictureBox1.CreateGraphics(),
                                      10,//pictureBoxGraph.ClientRectangle.Width,
                                      10//pictureBoxGraph.ClientRectangle.Height
                                        );
                } while (drawSomething.g == null);

                RefreshPictureBox();
            }
        }

        private void RefreshPictureBox()
        {
            if ((Visible == true) && (WindowState != FormWindowState.Minimized))
            {
                DrawOnPictureBox();
            }
        }

        private void ClearPictureBox()
        {

            if (drawSomething.g != null)
            {
                drawSomething.g.Clear(Color.White);

            }
            else
            {
                drawSomething.CreateDoubleBuffer(pictureBox1.CreateGraphics(), pictureBox1.ClientRectangle.Width, pictureBox1.ClientRectangle.Height);

                RefreshPictureBox();
            }

        }
        #endregion

        #region comboboxevent

        private void comboBoxShapes_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxDrawing_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        #endregion

        #region Draw Stuff

        private void DrawOnPictureBox()
        {
            try
            {
                if (drawSomething.g != null)
                {
                    GraphicsPath myGraphicsPath = new GraphicsPath();
                    PointF graphPointL = new PointF();
                    PointF graphPointR = new PointF();
                    int RectX, RectY, RectWidth, RectHeight = 0;

                    switch (comboBoxShapes.SelectedIndex)
                    {
                        case 0:
                            RectX = (int)Convert.ToInt32(textBoxX1.Text);
                            RectY = (int)Convert.ToInt32(textBoxY1.Text);
                            RectWidth = (int)Convert.ToInt32(textBoxX2.Text);
                            RectHeight = (int)Convert.ToInt32(textBoxY2.Text);
                            myGraphicsPath.AddRectangle(new Rectangle(RectX, RectY, RectWidth, RectHeight));
                            break;
                        case 1:
                            RectX = (int)Convert.ToInt32(textBoxX1.Text);
                            RectY = (int)Convert.ToInt32(textBoxY1.Text);
                            RectWidth = (int)Convert.ToInt32(textBoxX2.Text);
                            RectHeight = (int)Convert.ToInt32(textBoxY2.Text);
                            myGraphicsPath.AddEllipse(new Rectangle(RectX, RectY, RectWidth, RectHeight));
                            break;
                        case 2:
                            graphPointL.X = (float)Convert.ToInt32(textBoxX1.Text);
                            graphPointL.Y = (float)Convert.ToInt32(textBoxY1.Text);
                            graphPointR.X = (float)Convert.ToInt32(textBoxX2.Text);
                            break;
                        case 3:
                            graphPointL.X = (float)Convert.ToInt32(textBoxX1.Text);
                            graphPointL.Y = (float)Convert.ToInt32(textBoxY1.Text);
                            graphPointR.X = (float)Convert.ToInt32(textBoxX2.Text);
                            graphPointL.Y = (float)Convert.ToInt32(textBoxY2.Text);
                            myGraphicsPath.AddLine(graphPointL, graphPointR);
                            break;
                    }

                    Pen thePen = new Pen(Color.BurlyWood, 2);
                    drawSomething.g.DrawPath(thePen, myGraphicsPath);
                    pictureBox1.Invalidate(true);
                    thePen.Dispose();
                    drawSomething.g.Dispose();
                }
                else InitGraphics();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                                "Error in DrawingOnPictureBox",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Click Events
        private void buttonApplyChanges_Click(object sender, EventArgs e)
        {
            DrawOnPictureBox();
        }

        private void ClearPicture_Click(object sender, EventArgs e)
        {
            ClearPictureBox();
        }
        #endregion

        #region textBox events

        private void TextBoxX2_KeyPress(object sender, KeyPressEventArgs e)
        {//allow only numbers and the backspace in the textbox

            //allow only numbers, 1 decimal point, and the backspace in the textboxes preserve copy and paste
            if ((e.KeyChar != 22) && // CNTRL + v
                (e.KeyChar != 24) && // CNTRL + x
                (e.KeyChar != 3))    // CNTRL + c
            {

                if (textBoxX2.Text.Contains(".") == true)
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != 8))
                        e.Handled = true;
                }
                else
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != '.') && (e.KeyChar != 8))
                        e.Handled = true;
                }
            }
        }
        private void TextBoxX1_KeyPress(object sender, KeyPressEventArgs e)
        {//allow only numbers and the backspace in the textbox

            //allow only numbers, 1 decimal point, and the backspace in the textboxes preserve copy and paste
            if ((e.KeyChar != 22) && // CNTRL + v
                (e.KeyChar != 24) && // CNTRL + x
                (e.KeyChar != 3))    // CNTRL + c
            {

                if (textBoxX1.Text.Contains(".") == true)
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != 8))
                        e.Handled = true;
                }
                else
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != '.') && (e.KeyChar != 8))
                        e.Handled = true;
                }
            }
        }

        private void TextBoxY2_KeyPress(object sender, KeyPressEventArgs e)
        {//allow only numbers and the backspace in the textbox

            //allow only numbers, 1 decimal point, and the backspace in the textboxes preserve copy and paste
            if ((e.KeyChar != 22) && // CNTRL + v
                (e.KeyChar != 24) && // CNTRL + x
                (e.KeyChar != 3))    // CNTRL + c
            {

                if (textBoxY2.Text.Contains(".") == true)
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != 8))
                        e.Handled = true;
                }
                else
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != '.') && (e.KeyChar != 8))
                        e.Handled = true;
                }
            }
        }
        private void TextBoxY1_KeyPress(object sender, KeyPressEventArgs e)
        {//allow only numbers and the backspace in the textbox

            //allow only numbers, 1 decimal point, and the backspace in the textboxes preserve copy and paste
            if ((e.KeyChar != 22) && // CNTRL + v
                (e.KeyChar != 24) && // CNTRL + x
                (e.KeyChar != 3))    // CNTRL + c
            {

                if (textBoxY1.Text.Contains(".") == true)
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != 8))
                        e.Handled = true;
                }
                else
                {
                    if (((e.KeyChar < 48) || (e.KeyChar > 57)) && (e.KeyChar != '.') && (e.KeyChar != 8))
                        e.Handled = true;
                }
            }
        }
        #endregion
    }
}
