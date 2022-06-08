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
    public partial class FormGraph : Form
    {

        #region member variables

        private const UInt32 NUM_LINES = 3;

        // List of lines used
        private List<UInt16> analogList = new List<UInt16>();
        private List<UInt16> count1List = new List<UInt16>();
        private List<UInt16> count2List = new List<UInt16>();

        // graph variables
        private DBGraphics graph = new DBGraphics();
        private Int32 viewTime = 300, viewEnd = 300, viewStart = 0, viewMax = 300, checkTime = 0;
        private Int32 arrayElements = 0, constantTime = 0;
        private Boolean graphPause = false;

        private Line[] lineArray = new Line[3] { 
                                                 new Line(0, 1023, "Analog", Color.Black, DashStyle.Solid, 1, true),
                                                 new Line(0, 2000, "Count 1", Color.Blue, DashStyle.Dash, 1, true),
                                                 new Line(0, 2000, "Count 2", Color.Green, DashStyle.Solid, 1, true)
                                               };
        
        private int[] grid = new int[] { 4, 5};
        private int[] UpTime = new int[] { 0, 2, 0, 1};

        bool graphing = false;

        // custom events
        public delegate void OpenLineFormatEventHandler(object sender, OpenLineFormatEventArgs hvsa);
        public event OpenLineFormatEventHandler OpenLineFormatEvent;
        public delegate void OpenGridFormatEventHandler(object sender, OpenGridFormatEventArgs gvsa);
        public event OpenGridFormatEventHandler OpenGridFormatEvent;
        public delegate void OpenTimeFormatEventHandler(object sender, OpenTimeFormatEventArgs mvsa);
        public event OpenTimeFormatEventHandler OpenTimeFormatEvent;

        // new form variable
        Form2 form2 = new Form2();

        #endregion

        public FormGraph()
        {
            InitializeComponent();

            //this.Icon = Properties.Resources.Graph;            

            graph.CreateDoubleBuffer(pictureBoxGraph.CreateGraphics(),
                                     pictureBoxGraph.ClientRectangle.Width,
                                     pictureBoxGraph.ClientRectangle.Height);
            graph.g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // event handlers
            pictureBoxGraph.Paint += PictureBoxGraph_Paint;
            pictureBoxGraph.Resize += PictureBoxGraph_Resize;

            formatGraphToolStripMenuItem.Click += FormatGraphToolStripMenuItem_Click;
            clearGraphToolStripMenuItem.Click += ClearGraphToolStripMenuItem_Click;
            pauseGraphToolStripMenuItem.Click += PauseGraphToolStripMenuItem_Click;
            startToolStripMenuItem.Click += StartToolStripMenuItem_Click;


            hScrollBar1.ValueChanged += HScrollBar1_ValueChanged;
            hScrollBar1.Scroll += HScrollBar1_Scroll;
            Load +=FormGraph_Load;
            FormClosing +=FormGraph_FormClosing;

            
        }

        #region windows events

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0112) // Test for WM_SYSCOMMAND
                if (((int)m.WParam & 0xFFF0) == 0xF020)   // Test for SW_MINIMIZE
                { // Form is being minimized.
                    graphPause = true;
                }
                else if (((int)m.WParam & 0xFFF0) == 0xF030) // Test for SW_MAXIMIZED
                {
                    graphPause = false;
                }
                else if (((int)m.WParam & 0xFFF0) == 0xF010)// Test for SW_NORMAL
                {
                    graph.CreateDoubleBuffer(pictureBoxGraph.CreateGraphics(),
                                      pictureBoxGraph.ClientRectangle.Width,
                                      pictureBoxGraph.ClientRectangle.Height
                                        );
                    graphPause = false;
                    RefreshGraph();

                }// Test for SW_NORMAL
            base.WndProc(ref m);

        }

        #endregion

        #region menu events

        private void FormatGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLineFormatEventArgs olfea = new OpenLineFormatEventArgs(lineArray);
            OpenGridFormatEventArgs dlfea = new OpenGridFormatEventArgs(grid);
            OpenTimeFormatEventArgs blfea = new OpenTimeFormatEventArgs(UpTime);
            OnOpenTimeFormatEvent(blfea);
            OnOpenGridFormatEvent(dlfea);
            OnOpenLineFormatEvent(olfea);

        }

        private void ClearGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearGraph();
        }

        private void PauseGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pauseGraphToolStripMenuItem.Checked == false)
            {
                pauseGraphToolStripMenuItem.Checked = true;
                graphPause = true;
            }
            else
            {
                pauseGraphToolStripMenuItem.Checked = false;
                graphPause = false;
            }
        }

        private void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if(startToolStripMenuItem.Text == "Start")
            {
                startToolStripMenuItem.Text = "Stop";
                graphing = true;
            }
            else if(startToolStripMenuItem.Text == "Stop")
            {
                startToolStripMenuItem.Text = "Start";
                graphing = false;
            }

        }

        private void drawRandomStuffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenForm2();
        }

        private void OpenForm2()
        {
            form2.Visible = true;
            form2.BringToFront();
        }
        #endregion

        #region pictureBox events

        private void PictureBoxGraph_Resize(object sender, EventArgs e)
        {
            graph.CreateDoubleBuffer(pictureBoxGraph.CreateGraphics(),
                                     pictureBoxGraph.ClientRectangle.Width,
                                     pictureBoxGraph.ClientRectangle.Height);

            RefreshGraph();
        }

        private void PictureBoxGraph_Paint(object sender, PaintEventArgs e)
        {
            if (graph.CanDoubleBuffer() == true) graph.Render(e.Graphics);
            else
            {
                graph.CreateDoubleBuffer(pictureBoxGraph.CreateGraphics(),
                                      10,//pictureBoxGraph.ClientRectangle.Width,
                                      10//pictureBoxGraph.ClientRectangle.Height
                                        );

                RefreshGraph();
            }
        }

        #endregion

        #region private methods 

        private void InitGraphics()
        {
            if(graph.g == null)
            {
                do
                {
                    graph.CreateDoubleBuffer(pictureBoxGraph.CreateGraphics(),
                                      10,//pictureBoxGraph.ClientRectangle.Width,
                                      10//pictureBoxGraph.ClientRectangle.Height
                                        );
                } while (graph.g == null);

                RefreshGraph();
            }
        }
        
        private void RefreshGraph()
        {
            if ((Visible == true) && (WindowState != FormWindowState.Minimized))
            {
                UpdateGrid(grid);
                ShowTimeMarks();
            }
        }

        //private void DrawGrid()
        //{

        //    try
        //    {
        //        if (graph.g != null)
        //        {
        //            float majorTime = 4, minorTime = 16, majorVertDiv = 5, minorVertDiv = 25;
        //            float viewLeft, viewTop, viewHeight, viewWidth;
        //            float graphStepMinorH, graphStepMajorH, graphStepMajorV, graphStepMinorV;
        //            Pen minorPen = new Pen(Color.Gray, 1f);
        //            Pen majorPen = new Pen(Color.Gray, 2f);

        //            if (graph.g != null)
        //            {//don't try to draw if graph.g is null                
        //                graph.g.SetClip(pictureBoxGraph.ClientRectangle);
        //                graph.g.Clear(Color.White);

        //                viewLeft = (float)pictureBoxGraph.Width * .02f;//left side of graph area
        //                viewTop = (float)pictureBoxGraph.Height * .05f;//top of graph area
        //                viewHeight = ((float)pictureBoxGraph.Height) - (viewTop * 2);//height of graph area
        //                viewWidth = (float)pictureBoxGraph.Width - (viewLeft * 2);//width of graph area

        //                graphStepMinorV = viewWidth / minorTime;//minor time division
        //                graphStepMajorV = viewWidth / majorTime;//major time division

        //                graphStepMajorH = viewHeight / majorVertDiv;//major scale division (horizontal)
        //                graphStepMinorH = viewHeight / minorVertDiv;//minor scale division

        //                for (float a = 0; a <= viewHeight; a += graphStepMinorH)
        //                {//minor horizontal grid lines
        //                    graph.g.DrawLine(minorPen,
        //                                     viewLeft,
        //                                     viewTop + a,
        //                                     viewWidth + viewLeft,
        //                                     viewTop + a);
        //                }

        //                for (float a = 0; a <= viewHeight; a += graphStepMajorH)
        //                {//major horizontal grid lines
        //                    graph.g.DrawLine(majorPen,
        //                                     viewLeft,
        //                                     viewTop + a,
        //                                     viewWidth + viewLeft,
        //                                     viewTop + a);
        //                }

        //                for (float a = 0; a < viewWidth; a += graphStepMinorV)
        //                {//minor vertical grid lines
        //                    graph.g.DrawLine(minorPen,
        //                                     viewLeft + a,
        //                                     viewTop,
        //                                     viewLeft + a,
        //                                     viewHeight + viewTop);
        //                }

        //                for (float a = 0; a <= viewWidth; a += graphStepMajorV)
        //                {//major vertical grid lines              
        //                    graph.g.DrawLine(majorPen,
        //                                     viewLeft + a,
        //                                     viewTop,
        //                                     viewLeft + a,
        //                                     viewHeight + viewTop);
        //                }
        //            }

        //            minorPen.Dispose();
        //            majorPen.Dispose();
        //            pictureBoxGraph.Invalidate();

        //        }
        //        else InitGraphics();
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message,
        //                        "Error in DrawGrid",
        //                        MessageBoxButtons.OK,
        //                        MessageBoxIcon.Error);
        //    }
        //}

        private void ShowTimeMarks()
        {
            try
            {
                if (graph.g != null)
                {
                    string msString = "00";
                    string hString = "#0";
                    Int32 hours = 0, mins = 0, secs = 0, graphTime = 0;
                    float viewLeft, viewWidth;
                    SolidBrush textBrush = new SolidBrush(Color.Black);
                    PointF timePoint = new PointF();
                    FontFamily fontFam = new FontFamily("Times New Roman");
                    Font fontNumLarge = new Font(fontFam,
                                                12,
                                                FontStyle.Bold,
                                                GraphicsUnit.Point);
                    Font fontNumSmall = new Font(fontFam,
                                                8,
                                                FontStyle.Bold,
                                                GraphicsUnit.Point);
                    Font fontNum;

                    if (pictureBoxGraph.Height > 500) fontNum = fontNumLarge;
                    else fontNum = fontNumSmall;



                    viewLeft = (float)pictureBoxGraph.Width * .02f;
                    viewWidth = (float)pictureBoxGraph.Width - (viewLeft * 2);

                    float timeMark = (viewWidth - viewLeft) / grid[0];

                    if (viewStart < 0) viewStart = 0;
                    graphTime = viewStart;

                    timePoint.X = viewLeft;
                    timePoint.Y = (float)(pictureBoxGraph.Height - (fontNum.Height * 1.5));

                    graph.g.SetClip(pictureBoxGraph.ClientRectangle);

                    do
                    {
                        hours = mins = 0;
                        secs = graphTime;
                        if (graphTime > 59)
                        {
                            mins = graphTime / 60;
                            secs = graphTime - (mins * 60);
                            if (mins > 59)
                            {
                                hours = mins / 60;
                                mins -= hours * 60;
                            }
                        }
                        graph.g.DrawString(hours.ToString(hString) + ":" +
                                           mins.ToString(msString) + ":" +
                                           secs.ToString(msString),
                                           fontNum,
                                           textBrush,
                                           timePoint);
                        if(checkTime == 0)
                        {
                            graphTime += viewTime / grid[0];
                        }
                        else
                        {
                            graphTime += constantTime / grid[0];
                        }
                        timePoint.X += timeMark;

                    } while (graphTime <= viewMax);

                    textBrush.Dispose();
                    fontNum.Dispose();
                    fontNumSmall.Dispose();
                    fontNumLarge.Dispose();
                    pictureBoxGraph.Invalidate();
                }
                else InitGraphics();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                                "Error in ShowTimeMarks",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void DrawGraphLines(HScrollBar HSbar)
        {
            try
            {
                if (graph.g != null)
                {


                    Int32 graphEnd = 0, index = 0;
                    float xStep = 0, range = 0, offSet = 0;
                    float viewLeft, viewTop, viewHeight, viewWidth;
                    RectangleF graphRect = new RectangleF();
                    PointF graphPointL = new PointF();
                    PointF graphPointR = new PointF();
                    GraphicsPath[] linePath = new GraphicsPath[3]{ new GraphicsPath(),
                                                               new GraphicsPath(),
                                                               new GraphicsPath()
                                                             };
                    List<UInt16> dataList = new List<UInt16>();

                    viewLeft = (float)pictureBoxGraph.Width * .02f;//left side of graph area
                    viewTop = (float)pictureBoxGraph.Height * .05f;//top of graph area
                    viewHeight = ((float)pictureBoxGraph.Height) - (viewTop * 2);//height of graph area
                    viewWidth = (float)pictureBoxGraph.Width - (viewLeft * 2);//width of graph area

                    graphRect.X = viewLeft;
                    graphRect.Y = viewTop;
                    graphRect.Height = viewHeight;
                    graphRect.Width = viewWidth;

                    graph.g.SetClip(graphRect);
                    if (checkTime == 0)
                    { 
                        xStep = viewWidth / (float)viewTime;
                        HSbar.Enabled = false;
                    }
                    else 
                    { 
                        xStep = viewWidth / (float)constantTime;
                        HSbar.Enabled = true;
                    }


                    //if (graphPause == false)
                    //{
                    //    if (arrayElements > viewMax)
                    //    {
                    //        while (viewMax < arrayElements)
                    //        {
                    //            if(checkTime == 0)
                    //            {
                    //                viewEnd += viewTime / grid[0];
                    //                //viewStart += viewTime / grid[0];
                    //                viewMax += viewTime / grid[0];
                    //            }
                    //            else
                    //            {
                    //                viewEnd += constantTime / grid[0];
                    //                viewMax += constantTime / grid[0];
                    //            }
                    //            HSbar.Maximum = viewMax;
                    //            HSbar.Value = viewMax;
                    //            textBox1scroll.Text = HSbar.Value.ToString();
                    //        }
                            
                    //    }
                    //}

                    for (UInt32 a = 0; a < NUM_LINES; a++)
                    {
                        if (lineArray[a].showOnGraph == true)
                        {
                            Pen linePen = new Pen(Color.Black);
                            //al.Clear();
                            dataList = SelectList(a);//get the list for the line being graphed
                                                     //get line attributes
                            linePen.Color = lineArray[a].lineColor;
                            linePen.Width = lineArray[a].lineWidth;
                            linePen.DashStyle = lineArray[a].lineStyle;

                            range = (float)(lineArray[a].maxVal - lineArray[a].minVal);
                            offSet = ((float)lineArray[a].minVal / range) * viewHeight;

                            if (graphPause == false) graphEnd = dataList.Count;
                            else graphEnd = viewEnd;
                            index = viewStart;
                            for (Int32 b = 0; b < graphEnd; b++)
                            {
                                if (index < dataList.Count)
                                {
                                    graphPointL.X = viewLeft + (b * xStep);
                                    graphPointL.Y = (viewHeight + viewTop) -
                                                    (((float)Convert.ToDouble(dataList[index].ToString()) / range) * viewHeight) +
                                                    offSet;
                                    if ((index + 1) < dataList.Count)
                                    {
                                        graphPointR.X = viewLeft + ((b + 1) * xStep);
                                        graphPointR.Y = (viewHeight + viewTop) -
                                                        (((float)Convert.ToDouble(dataList[index + 1].ToString()) / range) * viewHeight) +
                                                        offSet;
                                        linePath[a].AddLine(graphPointL, graphPointR);
                                    }
                                    index++;
                                }
                            }
                            graph.g.DrawPath(linePen, linePath[a]);
                            pictureBoxGraph.Invalidate();
                        }//if (lineArray[a].showOnGraph == true)                        
                    }//for (UInt32 a = 0; a < NUM_LINES; a++)
                }
                //else InitGraphics();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                                "Error in DrawGraphLines",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private List<UInt16> SelectList(UInt32 line)
        {
            List<UInt16> list = new List<UInt16>();
            list.Clear();

            switch (line)
            {
                case 0:
                    for(int a = 0;a < analogList.Count; a++)
                    {
                        list.Add(analogList[a]);
                    }
                    break;
                case 1:
                    for(int a = 0;a < count1List.Count; a++)
                    {
                        list.Add(count1List[a]);
                    }
                    break;
                case 2:
                    for(int a = 0;a < count2List.Count; a++)
                    {
                        list.Add(count2List[a]);
                    }
                    break;                
            }

            return list;
        }

        //private void CalculateGraphWidthSeconds()
        //{
        //    try
        //    {
        //        viewTime =  0;
        //        viewTime = (Int32)numericUpDownGraphWidthHours.Value * 3600;
        //        viewTime += (Int32)numericUpDownGraphWidthMins.Value * 60;

        //        if (arrayElements > viewTime)
        //        {
        //            viewTime += 300;
        //            viewMax = ((arrayElements / viewTime) * viewTime) + (viewTime / 4);
        //            viewEnd = viewMax;
        //            viewStart = 0;// viewEnd - viewTime;
        //            if (viewStart < 0) viewStart = 0;
        //        }
        //        else
        //        {
        //            viewEnd = viewTime;
        //            viewStart = 0;
        //            viewMax = viewTime;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message,
        //                        "Error in CalculateGraphWidthSeconds",
        //                        MessageBoxButtons.OK,
        //                        MessageBoxIcon.Error);
        //    }
        //}

        private void ClearGraph()
        {
            analogList.Clear();
            count1List.Clear();
            count2List.Clear();
            arrayElements = 0;

            if (graph.g != null)
            {
                graph.g.Clear(Color.White);
                UpdateGrid(grid);
                ShowTimeMarks();
            }
            else
            {
                graph.CreateDoubleBuffer(pictureBoxGraph.CreateGraphics(),
                                      10,//pictureBoxGraph.ClientRectangle.Width,
                                      10//pictureBoxGraph.ClientRectangle.Height
                                        );

                RefreshGraph();
            }

        }

        private void HScrollBar1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void setDisplay(HScrollBar HSbar)
        {

            //label6.Text = constantTime.ToString();

            //pageInc = viewTime / (grid[0] * 2);
            //viewEnd = ((analogList.Count / pageInc) + 1) * pageInc;

            if (analogList.Count < constantTime)
            {//there is less than one page of data in the arraylist
                viewEnd = constantTime;
                viewStart = 0;
                HSbar.Enabled = false;
                HSbar.Maximum = constantTime;
            }
            else
            {
                viewStart = viewEnd - constantTime;
                HSbar.Enabled = true;
            }

            if(graphPause == false)
            {
                HSbar.Value = HSbar.Maximum;
            }
            
            HSbar.LargeChange = constantTime;
            HSbar.SmallChange = constantTime / (grid[0] * 2);
            label8.Text = analogList.Count.ToString();
        }

        private void HScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            scrollGraph(hScrollBar1);
        }

        private void scrollGraph(HScrollBar HSbar)
        {
            graphPause = true;
            pauseGraphToolStripMenuItem.Checked = true;

            hScrollBar1.Maximum = ((analogList.Count / constantTime) * constantTime) + constantTime;
            label7.Text = HSbar.Value.ToString();
            label6.Text = hScrollBar1.Value.ToString();
            viewEnd = HSbar.Value;
            viewMax = viewEnd;
            viewStart = viewEnd - constantTime;
            if (viewStart <= 0)
            {
                viewStart = 0;
                HSbar.Value = constantTime;
                viewEnd = constantTime;
                viewMax = constantTime;
                viewTime = constantTime;
                
            }
            setDisplay(HSbar);
            CalculateGraphWidthSeconds();
            UpdateGrid(grid);
            ShowTimeMarks();
            DrawGraphLines(hScrollBar1);
            pictureBoxGraph.Invalidate();
        }
        #endregion

        #region public methods

        public void UpdateData(UInt16[] dataArray)
        {
            if (graphing == true)
            {
                analogList.Add(dataArray[0]);
                count1List.Add(dataArray[1]);
                count2List.Add(dataArray[2]);
                arrayElements++;
                if (Visible == true)
                {
                    if (graphPause == false)
                    {
                        //CalculateGraphWidthSeconds();
                        if(constantTime == 0)
                        {
                            hScrollBar1.Maximum = arrayElements;
                            //textBox1scroll.Text = hScrollBar1.Maximum.ToString();
                        }
                        else
                        {
                            //hScrollBar1.Maximum = constantTime + (arrayElements / constantTime);
                            hScrollBar1.Maximum = ((analogList.Count / constantTime) * constantTime) + constantTime;
                            //textBox1scroll.Text = hScrollBar1.Maximum.ToString();
                        }
                        
                        CalculateGraphWidthSeconds();
                        UpdateGrid(grid);
                        ShowTimeMarks();
                        DrawGraphLines(hScrollBar1);
                        pictureBoxGraph.Invalidate();
                    }
                }
            }
        }

        public void UpdateLines(Line[] newLines)
        {
            if(newLines.Length == 3)
            {
                for(int i = 0; i < newLines.Length; i++)
                {
                    lineArray[i].lineStyle = newLines[i].lineStyle;
                    lineArray[i].lineColor = newLines[i].lineColor;
                    lineArray[i].lineWidth = newLines[i].lineWidth;
                    lineArray[i].showOnGraph = newLines[i].showOnGraph;
                    lineArray[i].maxVal = newLines[i].maxVal;
                    lineArray[i].minVal = newLines[i].minVal;
                }
            }
        }

        public void UpdateGrid(int[] newgrid)
        {
            try
            {
                if (graph.g != null)
                { 
                    grid[0] = newgrid[0];
                    float minorTime = newgrid[0] * newgrid[0];
                    grid[1] = newgrid[1];
                    float minorVertDiv = newgrid[1] * newgrid[1];
                    float viewLeft, viewTop, viewHeight, viewWidth;
                    float graphStepMinorH, graphStepMajorH, graphStepMajorV, graphStepMinorV;
                    Pen minorPen = new Pen(Color.Gray, 1f);
                    Pen majorPen = new Pen(Color.Gray, 2f);

                    if (graph.g != null)
                    {//don't try to draw if graph.g is null                
                        graph.g.SetClip(pictureBoxGraph.ClientRectangle);
                        graph.g.Clear(Color.White);

                        viewLeft = (float)pictureBoxGraph.Width * .02f;//left side of graph area
                        viewTop = (float)pictureBoxGraph.Height * .05f;//top of graph area
                        viewHeight = ((float)pictureBoxGraph.Height) - (viewTop * 2);//height of graph area
                        viewWidth = (float)pictureBoxGraph.Width - (viewLeft * 2);//width of graph area

                        graphStepMinorV = viewWidth / minorTime;//minor time division
                        graphStepMajorV = viewWidth / grid[0];//major time division

                        graphStepMajorH = viewHeight / grid[1];//major scale division (horizontal)
                        graphStepMinorH = viewHeight / minorVertDiv;//minor scale division

                        for (float a = 0; a <= viewHeight; a += graphStepMinorH)
                        {//minor horizontal grid lines
                            graph.g.DrawLine(minorPen,
                                             viewLeft,
                                             viewTop + a,
                                             viewWidth + viewLeft,
                                             viewTop + a);
                        }

                        for (float a = 0; a <= viewHeight; a += graphStepMajorH)
                        {//major horizontal grid lines
                            graph.g.DrawLine(majorPen,
                                             viewLeft,
                                             viewTop + a,
                                             viewWidth + viewLeft,
                                             viewTop + a);
                        }

                        for (float a = 0; a < viewWidth; a += graphStepMinorV)
                        {//minor vertical grid lines
                            graph.g.DrawLine(minorPen,
                                             viewLeft + a,
                                             viewTop,
                                             viewLeft + a,
                                             viewHeight + viewTop);
                        }

                        for (float a = 0; a <= viewWidth; a += graphStepMajorV)
                        {//major vertical grid lines              
                            graph.g.DrawLine(majorPen,
                                             viewLeft + a,
                                             viewTop,
                                             viewLeft + a,
                                             viewHeight + viewTop);
                        }
                    }

                    minorPen.Dispose();
                    majorPen.Dispose();
                    pictureBoxGraph.Invalidate();

                }
                else InitGraphics();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                                "Error in DrawGrid",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        public void CalculateGraphWidthSeconds()
        {
            try
            {
                if (graphPause == false)
                {
                    if (arrayElements > viewTime)
                    {
                        if (checkTime == 0)
                        {
                            if (viewTime == 0)
                            {
                                viewTime += 5 * 60;
                                viewTime += (viewTime / grid[0]);
                                viewMax = ((arrayElements / viewTime) * viewTime) + (viewTime / grid[0]);
                                viewEnd = viewMax;
                                viewStart = 0;// viewEnd - viewTime;
                                if (viewStart < 0) viewStart = 0;
                            }
                            else
                            {
                                viewTime += (viewTime / grid[0]);
                                viewMax = ((arrayElements / viewTime) * viewTime) + (viewTime / grid[0]);
                                //viewMax += (viewTime / grid[0]);
                                viewEnd = viewMax;
                                viewStart = 0;// viewEnd - viewTime;
                                if (viewStart < 0) viewStart = 0;
                            }
                        }
                        else
                        {
                            label1.Text = "true";
                            if (viewTime == 0)
                            {
                                viewStart = viewTime;
                                label1.Text = viewStart.ToString();
                                label2.Text = arrayElements.ToString();
                                viewMax += viewTime;
                                label4.Text = viewMax.ToString();
                                viewTime += viewTime;
                                label3.Text = viewTime.ToString();
                                viewEnd = viewMax;
                                label5.Text = viewEnd.ToString();
                            }
                            else
                            {
                                hScrollBar1.Value = viewTime;
                                viewStart = viewTime;
                                label1.Text = viewStart.ToString();
                                label2.Text = arrayElements.ToString();
                                viewMax += viewTime;
                                label4.Text = viewMax.ToString();
                                viewTime += constantTime;
                                label3.Text = viewTime.ToString();
                                //viewMax += (viewTime / grid[0]);
                                viewEnd = viewMax;
                                label5.Text = viewEnd.ToString();
                            }
                        }
                        while (viewMax < arrayElements)
                        {
                            if (checkTime == 0)
                            {
                                viewEnd += viewTime / grid[0];
                                //viewStart += viewTime / grid[0];
                                viewMax += viewTime / grid[0];
                            }
                            else
                            {
                                viewEnd += constantTime / grid[0];
                                viewMax += constantTime / grid[0];
                            }
                            hScrollBar1.Maximum = viewMax;
                            hScrollBar1.Value = viewMax;
                            textBox1scroll.Text = hScrollBar1.Value.ToString();
                        }

                    }
                    else
                    {
                        textBox1scroll.Text = hScrollBar1.Value.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                                "Error in CalculateGraphWidthSeconds",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        public void UpdateTime(int[] upTime)
        {
            if (upTime[0] == 0 && upTime[1] == 0 && upTime[2] == 0 && upTime[3] == 1)
            {
                upTime[0] = 0;
                upTime[1] = 2;
                upTime[2] = 0;
                upTime[3] = 1;
                string message = "Sorry, Constant Time must have a Time greater than zero. Using Default values.";
                string title = "Close Window";
                MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                {
                    this.Close();
                }
            }
            //viewTime = (Int32)numericUpDown3.Value;
            //viewTime = (Int32)numericUpDown2.Value * 3600;
            //viewTime += (Int32)numericUpDown1.Value * 60;
            UpTime[0] = upTime[0];
            UpTime[1] = upTime[1];
            UpTime[2] = upTime[2];
            UpTime[3] = upTime[3];
            viewTime = (Int32)upTime[0] * 3600;// Hour
            viewTime += (Int32)upTime[1] * 60; // Min
            viewTime += (Int32)upTime[2]; // Sec
            checkTime = (Int32)upTime[3];
            constantTime = viewTime;
            //hScrollBar1.Value = viewTime;
            hScrollBar1.LargeChange = constantTime;
            hScrollBar1.SmallChange = constantTime / (grid[0] * 2);
            if (checkTime == 0)
            {
                viewEnd = viewTime;
                viewStart = 0;
                viewMax = viewTime;
            }
            else
            {
                viewEnd = viewTime;
                viewMax = viewTime;
            }
        }
        #endregion

        #region form events

        private void FormGraph_Load(object sender, EventArgs e)
        {// this must be done here to initialize graph.g
            graph.g.Clear(Color.White);

            InitGraphics();
            RefreshGraph();
            DrawGraphLines(hScrollBar1);
        }

        private void FormGraph_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;//cancel close if user requested
            PropertyInfo pi = typeof(Form).GetProperty("CloseReason", BindingFlags.NonPublic | BindingFlags.Instance);
            pi.SetValue(this, CloseReason.None, null);
        }

        #endregion

        #region custom events

        protected void OnOpenLineFormatEvent(OpenLineFormatEventArgs slea)
        {
            OpenLineFormatEvent?.Invoke(this, slea);
        }

        protected void OnOpenGridFormatEvent(OpenGridFormatEventArgs llea)
        {
            OpenGridFormatEvent?.Invoke(this, llea);
        }

        protected void OnOpenTimeFormatEvent(OpenTimeFormatEventArgs clea)
        {
            OpenTimeFormatEvent?.Invoke(this, clea);
        }
        #endregion

        #region custom eventArguments

        public class OpenLineFormatEventArgs : EventArgs
        {// this event args class is used to open the FormLineFormat form
            public OpenLineFormatEventArgs(Line[] lines)
            {
                _lines = lines;
            }
            public Line[] _lines;
        }

        public class OpenGridFormatEventArgs : EventArgs
        {// this event args class is used to open the FormLineFormat form
            public OpenGridFormatEventArgs(int[] grids)
            {
                _grids = grids;
            }
            public int[] _grids;
        }

        public class OpenTimeFormatEventArgs : EventArgs
        {// this event args class is used to open the FormLineFormat form
            public OpenTimeFormatEventArgs(int[] times)
            {
                _times = times;
            }
            public int[] _times;
        }
        #endregion
    }
}
