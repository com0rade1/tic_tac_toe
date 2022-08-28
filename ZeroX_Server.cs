using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace WindowsFormsApp2
{
    public partial class ZeroX_Server : Form
    {
        static int port; // порт для приема входящих запросов
        static string Adress;
        Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipPoint;
        Socket handler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        int[,] PlayField;
        bool IPlayZeroes;
        bool CanIDoStep;
        int Scale;
        int SquareSize;
        int i, j, k;
        int ZeroWins;

        public ZeroX_Server(string ServerAdress, string ServerPort)
        {
            InitializeComponent();
            Graphics g = pictureBox1.CreateGraphics();
            port = Convert.ToInt32(ServerPort);
            Adress = ServerAdress;
            ipPoint = new IPEndPoint(IPAddress.Parse(ServerAdress), port);
            handler.Close();
            listenSocket.Close();
            handler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Task ServerEnabled()
            {
                return Task.Run(() =>
                {
                    try
                    {
                        // связываем сокет с локальной точкой, по которой будем принимать данные
                        listenSocket.Bind(ipPoint);

                        // начинаем прослушивание
                        listenSocket.Listen(10);
                        handler = listenSocket.Accept();
                        bool fl = true;
                        while (fl)
                        {
                          
                            StringBuilder builder = new StringBuilder();
                            int bytes = 0; // количество полученных байтов
                            byte[] data = new byte[256]; // буфер для получаемых данных
                            do
                            {
                                bytes = handler.Receive(data);
                                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                            }
                            while (handler.Available > 0);
                           if (builder.ToString().Contains("ZP"))
                            {
                                builder.Remove(0, 2);
                                if (builder.ToString() == "True")
                                {
                                    this.IPlayZeroes = false;
                                    this.CanIDoStep = false;
                                }
                                else
                                {
                                    this.IPlayZeroes = true;
                                    this.CanIDoStep = true;
                                }
                            }
                           if (builder.ToString().Contains("PF"))
                            {
                                builder.Remove(0, 2);
                                string PlayF = builder.ToString();
                                if (builder.ToString() == "3")
                                {
                                    this.Scale = 3;
                                    this.SquareSize = pictureBox1.Width/3;
                                }
                                else if (builder.ToString() == "4")
                                {
                                    this.Scale = 4;
                                    this.SquareSize = pictureBox1.Width/4;
                                }
                                else
                                {
                                    this.Scale = 5;
                                    this.SquareSize = pictureBox1.Width/5;
                                }
                                this.PlayField = new int[Scale, Scale];
                                PaintField(Scale);
                            }
                           if (builder.ToString().Contains(":"))
                            {
                                string s1 = builder.ToString();
                                s1.Trim();
                                string[] newData = s1.Split(':');
                                this.i = Convert.ToInt32(newData[0]);
                                this.j = Convert.ToInt32(newData[1]);
                                this.k = Convert.ToInt32(newData[2]);
                                CanIDoStep = true;
                                PaintZeroX(i,j,k);
                                if (((CheckWin() == 1) && (IPlayZeroes)) || ((CheckWin() == 0) && (!IPlayZeroes)))
                                {
                                    MessageBox.Show("Сервер, Вы победили!", "Поздравляем!", MessageBoxButtons.OK);
                                    this.Close();
                                }
                                else if (((CheckWin() == 1) && (!IPlayZeroes)) || ((CheckWin() == 0) && (IPlayZeroes)))
                                {
                                    MessageBox.Show("Сервер, Вы проиграли!", "Очень жаль!", MessageBoxButtons.OK);
                                    this.Close();
                                }

                            }
                        }
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(String.Format("Ошибка: {0} {1}",ex.Message,ex.StackTrace),"Внимание",MessageBoxButtons.OK);
                    }
                });

            }
            async void CallingServerEnabled()
            {
                await ServerEnabled();
            }
            CallingServerEnabled();

        }
        private void PaintField(int Scale)
        {
            Graphics g = pictureBox1.CreateGraphics();
            g.Clear(Color.White);
            for (int i = 0; i < Scale; i++)
            {
                for (int j = 0; j < Scale; j++)
                {
                    this.PlayField[i, j] = 10;
                }
            }
            for (int i = 0; i < pictureBox1.Width; i += pictureBox1.Width / Scale)
            {
                g.DrawLine(new Pen(Color.Black), i, 0, i, pictureBox1.Height);
                g.DrawLine(new Pen(Color.Black), 0, i, pictureBox1.Width, i);
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

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Graphics g = pictureBox1.CreateGraphics();
            if (CanIDoStep)
            {
                int i1 = 0;
                int j1 = 0;
                while (i1 * SquareSize < e.X)
                {
                    while (j1 * SquareSize < e.Y)
                    {
                        j1++;
                    }
                    i1++;
                }
                i1--;
                j1--;
                if (!IPlayZeroes && PlayField[i1,j1] == 10)
                {
                    this.PlayField[i1, j1] = 0;
                    g.DrawEllipse(new Pen(Brushes.Black, 2), i1 * SquareSize, j1 * SquareSize, SquareSize, SquareSize);
                    string message = String.Format("{0}:{1}:{2}", i1, j1, PlayField[i1, j1]);
                    this.CanIDoStep = false;
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    handler.Send(data);
                    Thread.Sleep(500);
                    if (((CheckWin() == 3) && (IPlayZeroes)) || ((CheckWin() == 0) && (!IPlayZeroes)))
                    {
                        MessageBox.Show("Сервер, Вы победили!", "Поздравляем!", MessageBoxButtons.OK);
                        this.Close();
                    }
                    else if (((CheckWin() == 3) && (!IPlayZeroes)) || ((CheckWin() == 0) && (IPlayZeroes)))
                    {
                        MessageBox.Show("Сервер, Вы проиграли!", "Очень жаль!", MessageBoxButtons.OK);
                        this.Close();
                    }
                }
                else if (PlayField[i1,j1] == 10)
                {
                    this.PlayField[i1, j1] = 1;
                    g.DrawLine(new Pen(Brushes.Black, 2), i1 * SquareSize, j1 * SquareSize, (i1 + 1) * SquareSize, (j1 + 1) * SquareSize);
                    g.DrawLine(new Pen(Brushes.Black, 2), i1 * SquareSize, (j1 + 1) * SquareSize, (i1 + 1) * SquareSize, j1 * SquareSize);
                    string message = String.Format("{0}:{1}:{2}", i1, j1, PlayField[i1, j1]);
                    this.CanIDoStep = false;
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    handler.Send(data);
                    Thread.Sleep(500);
                    if (((CheckWin() == 1) && (IPlayZeroes)) || ((CheckWin() == 0) && (!IPlayZeroes)))
                    {
                        MessageBox.Show("Сервер, Вы победили!", "Поздравляем!", MessageBoxButtons.OK);
                        this.Close();
                    }
                    else if (((CheckWin() == 1) && (!IPlayZeroes)) || ((CheckWin() == 0) && (IPlayZeroes)))
                    {
                        MessageBox.Show("Сервер, Вы проиграли!", "Очень жаль!", MessageBoxButtons.OK);
                        this.Close();
                    }
                }


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
