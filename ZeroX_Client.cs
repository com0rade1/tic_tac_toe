using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WindowsFormsApp2
{
    public partial class ZeroX_Client : Form
    {
        static int port; // порт сервера
        static string address; // адрес сервера
        string message = "";
        IPEndPoint ipPoint;
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        bool IPlayZeroes;
        bool CanIDoStep;
        int Scale = 0;
        int SquareSize;
        int[,] PlayField;
        int ZeroWins;
        public ZeroX_Client(string ServerAdress, string ServerPort)
        {
            InitializeComponent();
            port = Convert.ToInt32(ServerPort);
            address = ServerAdress;
            this.ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            socket.Connect(ipPoint);
            Task Listening()
            {
                return Task.Run(() =>
                {
                    while (true)
                    {
                        int bytes = 0;
                        byte[] data = new byte[256];
                        StringBuilder builder = new StringBuilder();
                        do
                        {
                            bytes = socket.Receive(data, data.Length, 0);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (socket.Available > 0);
                        if (builder.ToString().Contains(":"))
                        {
                            string[] newData = builder.ToString().Split(':');
                            int i = Convert.ToInt32(newData[0]);
                            int j = Convert.ToInt32(newData[1]);
                            int k = Convert.ToInt32(newData[2]);
                            this.CanIDoStep = true;
                            PaintZeroX(i, j, k);
                            if (((CheckWin() == 1) && (IPlayZeroes)) || ((CheckWin() == 0) && (!IPlayZeroes)))
                            {
                                MessageBox.Show("Клиент, Вы победили!", "Поздравляем!", MessageBoxButtons.OK);
                                this.Close();
                            }
                            else if (((CheckWin() == 1) && (!IPlayZeroes)) || ((CheckWin() == 0) && (IPlayZeroes)))
                            {
                                MessageBox.Show("Клиент, Вы проиграли!", "Очень жаль!", MessageBoxButtons.OK);
                                this.Close();
                            }

                        }
                    }

                });
            }
            async void ListeningEnabled()
            {
                await Listening();
            }
                ListeningEnabled();


        }

        private void Button1_Click(object sender, EventArgs e)
        {

            Graphics g = pictureBox1.CreateGraphics();
            g.Clear(Color.White);
            switch (comboBox1.SelectedItem)
            {
                case "3x3":
                    this.Scale = 3;
                    this.SquareSize = pictureBox1.Width / Scale;
                    break;
                case "4x4":
                    this.Scale = 4;
                    this.SquareSize = pictureBox1.Width / Scale;
                    break;
                case "5x5":
                    this.Scale = 5;
                    this.SquareSize = pictureBox1.Width / Scale;
                    break;
                default:
                    this.Scale = 3;
                    this.SquareSize = pictureBox1.Width / Scale;
                    break;
            }
            for (int i = 0; i < pictureBox1.Width; i += pictureBox1.Width / Scale)
            {
                g.DrawLine(new Pen(Color.Black), i, 0, i, pictureBox1.Height);
                g.DrawLine(new Pen(Color.Black), 0, i, pictureBox1.Width, i);
            }
            if (Scale == 3)
            {
               this.PlayField = new int[3, 3];
            }
            else if (Scale == 4)
            {
               this.PlayField = new int[4, 4];
            }
            else if (Scale == 5)
            {
              this.PlayField = new int[5, 5];
            }
            message = String.Format("PF{0}", Scale);
            byte[] data = Encoding.Unicode.GetBytes(message);
            socket.Send(data);
            Thread.Sleep(1000);
            for (int i = 0; i<Scale;i++)
            {
                for(int j = 0; j<Scale; j++)
                {
                    this.PlayField[i, j] = 10;
                }
            }
            comboBox1.Enabled = false;
            button1.Enabled = false;
            DialogResult result = MessageBox.Show("Играете крестиками?", "Определитесь", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                this.IPlayZeroes = true;
            }
            else
            {
                this.IPlayZeroes = false;
            }
            message = String.Format("ZP{0}", IPlayZeroes);
            data = Encoding.Unicode.GetBytes(message);
            socket.Send(data);
            Thread.Sleep(1000);
            if (IPlayZeroes)
            {
                this.CanIDoStep = true;
                MessageBox.Show("Вы ходите первым!", "Поздравляем", MessageBoxButtons.OK);
            }
            else
            {
                this.CanIDoStep = false;
                MessageBox.Show("Вы ходите вторым!", "Поздравляем", MessageBoxButtons.OK);
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Graphics g = pictureBox1.CreateGraphics();
            switch (comboBox1.SelectedItem)
            {
                case "3x3":
                    this.Scale = 3;
                    break;
                case "4x4":
                    this.Scale = 4;
                    break;
                case "5x5":
                    this.Scale = 5;
                    break;
                default:
                    this.Scale = 3;
                    break;
            }
            for (int i = 0; i < pictureBox1.Width; i += pictureBox1.Width / Scale)
            {
                g.DrawLine(new Pen(Color.Black), i, 0, i, pictureBox1.Height);
                g.DrawLine(new Pen(Color.Black), 0, i, pictureBox1.Width, i);
            }
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

           
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Graphics g = pictureBox1.CreateGraphics();
            if (CanIDoStep)
            {
                int i = 0;
                int j = 0;
                while (i * SquareSize <e.X)
                {
                    while (j*SquareSize < e.Y)
                    {
                        j++;
                    }
                    i++;
                }
                i--;
                j--;
                if (!IPlayZeroes && PlayField[i,j]==10)
                {
                    this.PlayField[i, j] = 0;
                    g.DrawEllipse(new Pen(Brushes.Black, 2), i*SquareSize, j*SquareSize, SquareSize, SquareSize);
                    this.CanIDoStep = false;
                    message = String.Format("{0}:{1}:{2}", i, j, PlayField[i, j], CheckWin());
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    socket.Send(data);
                    Thread.Sleep(500);
                    if (((CheckWin() == 3) && (IPlayZeroes)) || ((CheckWin() == 0) && (!IPlayZeroes)))
                    {
                        MessageBox.Show("Клиент, Вы победили!", "Поздравляем!", MessageBoxButtons.OK);
                        this.Close();
                    }
                    else if (((CheckWin() == 3) && (!IPlayZeroes)) || ((CheckWin() == 0) && (IPlayZeroes)))
                    {
                        MessageBox.Show("Клиент, Вы проиграли!", "Очень жаль!", MessageBoxButtons.OK);
                        this.Close();
                    }

                }
                else if (PlayField[i,j] == 10)
                {
                    this.PlayField[i, j] = 1;
                    g.DrawLine(new Pen(Brushes.Black, 2), i * SquareSize, j * SquareSize, (i + 1) * SquareSize, (j + 1) * SquareSize);
                    g.DrawLine(new Pen(Brushes.Black, 2), i * SquareSize, (j+1) * SquareSize, (i + 1) * SquareSize, j * SquareSize);
                    this.CanIDoStep = false;
                    message = String.Format("{0}:{1}:{2}", i, j, PlayField[i, j],CheckWin());
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    socket.Send(data);
                    Thread.Sleep(500);
                    if (((CheckWin() == 1) && (IPlayZeroes)) || ((CheckWin() == 0) && (!IPlayZeroes)))
                    {
                        MessageBox.Show("Клиент, Вы победили!", "Поздравляем!", MessageBoxButtons.OK);
                        this.Close();
                    }
                    else if (((CheckWin() == 1) && (!IPlayZeroes)) || ((CheckWin() == 0) && (IPlayZeroes)))
                    {
                        MessageBox.Show("Клиент, Вы проиграли!", "Очень жаль!", MessageBoxButtons.OK);
                        this.Close();
                    }
                }

            }
        }
        private void PaintZeroX(int i, int j, int k)
        {
            Graphics g = pictureBox1.CreateGraphics();
            this.PlayField[i, j] = k;
            if (k == 1)
            {
                g.DrawLine(new Pen(Brushes.Black, 2), i * SquareSize, j * SquareSize, (i + 1) * SquareSize, (j + 1) * SquareSize);
                g.DrawLine(new Pen(Brushes.Black, 2), i * SquareSize, (j + 1) * SquareSize, (i + 1) * SquareSize, j * SquareSize);
            }
            else
            {
                g.DrawEllipse(new Pen(Brushes.Black, 2), i * SquareSize, j * SquareSize, SquareSize, SquareSize);
            }
        }
        private int CheckWin()
        {
            int sum = 0;
            int sum1 = 0;
            if (Scale == 3)
            {
                for (int i = 0; i < Scale; i++)
                {
                    sum = 0;
                    sum1 = 0;
                    for (int j = 0; j < Scale; j++)
                    {
                        sum += PlayField[i, j];
                        sum1 += PlayField[i, j];
                    }
                    if ((sum == 3) || (sum1 == 3))
                    {
                        this.ZeroWins = 1;
                        goto HaveAWinner;

                    }
                    if ((sum == 0) || (sum1 == 0))
                    {
                        this.ZeroWins = 0;
                        goto HaveAWinner;

                    }
                    else this.ZeroWins = 2;
                }
                sum = 0;
                if (ZeroWins == 2)
                {
                    sum = PlayField[0, 0] + PlayField[1, 1] + PlayField[2, 2];
                    sum1 = PlayField[0, 2] + PlayField[1, 1] + PlayField[2, 0];
                    if ((sum == 0) || (sum1 == 0))
                    {
                        this.ZeroWins = 0;
                        goto HaveAWinner;
                    }
                    if ((sum == 3) || (sum1 == 3))
                    {
                        this.ZeroWins = 1;
                        goto HaveAWinner;
                    }
                    else this.ZeroWins = 2;
                }
            }
        HaveAWinner:
            return ZeroWins;
        }
    }
}
