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
    public partial class Form3 : Form
    {
        #region member variables
        public delegate void Form3progressEventHandler(object sender, progressEventArgs e);
        public event Form3progressEventHandler Form3progressEvent;
        #endregion

        public Form3()
        {
            InitializeComponent();
        }

        #region progress Event
        protected void OnprogressEvent(progressEventArgs aea)
        {
            if (Form3progressEvent != null) Form3progressEvent(this, aea);
        }
        #endregion

        #region progresseventArgs
        public class progressEventArgs : EventArgs
        {
            /// <summary>
            /// closing event
            /// </summary>
            public progressEventArgs(String progress)
            {
                _progress = progress;
            }

            public string _progress;

        }
        #endregion

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            progressEventArgs mea = new progressEventArgs(textBox2.Text);

            OnprogressEvent(mea);
        }
    }
}
