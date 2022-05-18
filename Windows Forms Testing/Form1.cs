namespace NewTesting
{
    public partial class Form1 : Form
    {
        #region instances
        Form2 form2 = new Form2();
        #endregion

        #region Form1
        public Form1()
        {
            InitializeComponent();

            form2.Form2dataEvent += Form2_Form2dataEvent1;
        }
        #endregion

        #region data event
        private void Form2_Form2dataEvent1(object sender, Form2.dataEventArgs e)
        {
            textBox1.Text = e._data;
        }
        #endregion

        #region open graph event
        private void OpenGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form2.Visible = true;
            form2.BringToFront();
        }
        #endregion

        #region Exit Application
        private void exitApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

    }
}