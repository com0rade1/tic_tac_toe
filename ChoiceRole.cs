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
    public partial class ChoiceRole : Form
    {
        public ChoiceRole()
        {
            InitializeComponent();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ClientInfo _ClientInfo = new ClientInfo();
            this.Visible = false;
            _ClientInfo.ShowDialog();
            this.Visible = true;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ServerInfo _ServerInfo = new ServerInfo();
            this.Visible = false;
            _ServerInfo.ShowDialog();
            this.Visible = true;
        }
    }
}
