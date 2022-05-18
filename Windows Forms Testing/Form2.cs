using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewTesting
{
    public partial class Form2 : Form
    {
        #region instances
        Form3 form3 = new Form3();
        #endregion

        #region member variables
        public delegate void Form2dataEventHandler(object sender, dataEventArgs e);
        public event Form2dataEventHandler Form2dataEvent;
        #endregion

        #region form2
        public Form2()
        {
            InitializeComponent();

            form3.Form3progressEvent += Form3_Form3progressEvent1;
        }
        #endregion

        
        #region progress event
        private void Form3_Form3progressEvent1(object sender, Form3.progressEventArgs e)
        {
            textBox2.Text = e._progress;
        }
        #endregion

        #region Closing Event
        protected void OndataEvent(dataEventArgs cea)
        {
            if (Form2dataEvent != null) Form2dataEvent(this, cea);
        }
        #endregion

        #region exit form 2
        private void exitToMainPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = false;
        }
        #endregion

        #region Start Simulation Event
        private void startSimulationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.timer1.Start();
            if (progressBar1.Value > 0)
            {
                startSimulationToolStripMenuItem.Text = "Resume";
            }
        }
        #endregion

        #region Timer Event
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.progressBar1.Increment((int)numericUpDown1.Value);
        }
        #endregion

        #region Stop/Resume Simulation Event
        private void stopSimulationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
            // Update stop and resume
            if (progressBar1.Value != 0)
            {
                startSimulationToolStripMenuItem.Text = "Resume";
            }
            else
                startSimulationToolStripMenuItem.Text = "Start Simulation";
        }
        #endregion

        #region Reset Simulation Event
        private void pauseSimulationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            this.timer1.Stop();
            if (progressBar1.Value == 0)
            {
                startSimulationToolStripMenuItem.Text = "Start Simulation";
            }
            else
                startSimulationToolStripMenuItem.Text = "Resume";
        }
        #endregion

        #region dataeventArgs
        public class dataEventArgs : EventArgs
        {
            /// <summary>
            /// closing event
            /// </summary>
            public dataEventArgs(string data)
            {
                _data = data;
            }

            public string _data;

        }
        #endregion

        #region textbox1 event
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            dataEventArgs dea = new dataEventArgs(textBox1.Text);

            OndataEvent(dea);
        }
        #endregion

        private void changeSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form3.Visible = true;
            form3.BringToFront();
        }
    }
}
