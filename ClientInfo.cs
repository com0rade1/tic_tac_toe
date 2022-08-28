using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class ClientInfo : Form
    {
        public ClientInfo()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string Server_Adress = textBox1.Text;
            string Server_Port = textBox2.Text;
            ZeroX_Client _ZeroX_Client = new ZeroX_Client(Server_Adress, Server_Port);
            this.Visible = false;
            _ZeroX_Client.ShowDialog();
            this.Visible = true;
        }
    }
}
