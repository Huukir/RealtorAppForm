using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealtorAppForm
{
    public partial class MainForm : Form
    {
        public object referMainForm = MainForm.ActiveForm;
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide(); //закрывает и очищает ресуры текущей формы 
            Form1 form1 = new Form1();
            if (form1.ShowDialog() == DialogResult.Cancel)
                Show();
        }
    }
}
