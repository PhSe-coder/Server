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
using System.Net.NetworkInformation;
namespace 服务器
{
    public partial class Form1 : Form
    {
        static Message rec_Message = new Message();
        public IPAddress IPaddress;//服务器IP地址
        public int IPport;//服务器IP端口
        static Socket serverSocket;
        public int PreClientNumber = 0;
        static SQLServer MessageWrite = new SQLServer();
        public static string AllInsert = "";
        public static string s1 = "";
        public string SurTemperature="";
        public string Humidity = "";
        public string Longitude1 ;
        public string Latitude1 ;
        public string Altitude ;
        public string Speed;
        public string Time;
        public string Date;
        public string WindSpeed;
        public string WaterLevel;
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btn_OpenServer_Click(object sender, EventArgs e)
        {
            StartServer();
            btn_OpenServer.Enabled = false;
            return;
        }
        private IPAddress GetIPAddress()
        {
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (_IPAddress.AddressFamily == AddressFamily.InterNetwork)
                    AddressIP = _IPAddress.ToString();
            return IPAddress.Parse(AddressIP);
        }
        /// <summary>
        /// 检测端口是否被占用
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        internal static bool PortInUse(int port)
        {
            bool inUse = false;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
        /// <summary>
        /// 获取空闲IP端口
        /// </summary>
        /// <returns></returns>
        private int GetIpPort()
        {
            int port;
            for (port = 5000; port <= 65535; port++)
            {
                if (!PortInUse(port))
                    return port;
            }
            return -1;
        }
        /// <summary>
        /// 客户端列表
        /// </summary>
        public static class ClientList
        {
            public static List<Socket> lst = new List<Socket>();
        }
        /// <summary>
        /// 建立服务器
        /// </summary>
        private void StartServer()
        {
            IPaddress = GetIPAddress();
            IPport = GetIpPort();
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//实例化一个Socket           
            IPEndPoint iPEndPoint = new IPEndPoint(IPaddress, IPport);
            textBox1.Text = IPaddress.ToString();
            textBox2.Text = IPport.ToString();
            serverSocket.Bind(iPEndPoint);//绑定ip和端口号           
            //MessageBox.Show("    服务器打开成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            serverSocket.Listen(0);// 等待队列(开始监听端口号)
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);//异步接受客户端连接
        }
        private void BeginReceiveMessages(Socket toClientsocket)
        {
            toClientsocket.BeginReceive(rec_Message.Data, rec_Message.StartIndex, rec_Message.RemindSize, SocketFlags.None, ReceiveCallBack, toClientsocket);
        }
        /// <summary>
        /// 当客户端连接到服务器时执行的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallBack(IAsyncResult ar)
        {

            Socket toClientsocket = serverSocket.EndAccept(ar); //这里获取到的是向客户端收发消息的Socket
            ClientList.lst.Add(toClientsocket);
            if(ClientList.lst.Count > 1)
            {
                for (int i = 1; i < ClientList.lst.Count; i++)
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[i-1].Cells[0].Value = ((IPEndPoint)ClientList.lst[i].RemoteEndPoint).Address;
                    dataGridView1.Rows[i-1].Cells[1].Value = ((IPEndPoint)ClientList.lst[i].RemoteEndPoint).Port;
                }
            }
            MessageBox.Show("    客户端连接成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            BeginReceiveMessages(toClientsocket);
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);//继续等待下一个客户端的链接
        }
        /// <summary>
        /// 接收到来自客户端消息的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            Socket toClientsocket = null;
            try
            {
                toClientsocket = ar.AsyncState as Socket;
                int count = toClientsocket.EndReceive(ar);//本次接收数据的字节数
                if (count == 0)
                {
                    toClientsocket.Close();
                    MessageBox.Show("Connect Error!");
                    for(int i=0;i < ClientList.lst.Count; i++)
                    {
                        if(Equals(toClientsocket, ClientList.lst[i]))
                        {
                            ClientList.lst.Remove(toClientsocket);
                        }
                    }                    
                    return;
                }
                string s = Encoding.UTF8.GetString(rec_Message.Data, 0, count);//打印来自客户端的消息
                
                if (s.StartsWith("STM32"))
                {
                    s1 = s;
                    IPAddress remote_ip = ((IPEndPoint)toClientsocket.RemoteEndPoint).Address;
                    textBox3.Text = remote_ip.ToString();//显示客户端IP
                    int port1 = ((IPEndPoint)toClientsocket.RemoteEndPoint).Port;
                    textBox4.Text = port1.ToString();//显示客户端端口
                    if (s.Length >= 10)//加入if语句保证数据传输出错时，Substring索引不出错
                        SurTemperature = s.Substring(6, 4) + "°"+"C";
                    if (s.Length >= 15)
                        Humidity = s.Substring(11, 4) + "%";
                    if (s.Length >= 26)
                        Longitude1 = s.Substring(16, 10);
                    if (s.Length >= 37)
                        Latitude1 = s.Substring(27, 10);
                    if (s.Length >= 45)
                        Altitude = s.Substring(38, 7);
                    if (s.Length >= 57)
                        Speed = s.Substring(46, 11);
                    if (s.Length >= 66)
                        Time = s.Substring(58, 8);
                    if (s.Length >= 77)
                        Date = s.Substring(67, 10);
                    if (s.Length >= 86)
                        WindSpeed = s.Substring(78, 8);
                    if (s.Length >= 93)
                        WaterLevel = s.Substring(87, 6);
                    AllInsert = "INSERT INTO DYNAMICSTATISTICS (SurTemperature,Humidity,Longitude,Latitude,Altitude,Speed,Time,Date,WindSpeed,WaterLevel)" +
            " VALUES('" + SurTemperature + "','" + Humidity + "','" + Longitude1 + "','" + Latitude1 + "','" + Altitude + "','" + Speed + "','" + Time + "','" + Date + "','" + WindSpeed + "','" + WaterLevel + "')";
                    MessageWrite.ExecuteUpdate(SQLServer.StrDynamicStatistic, AllInsert);
                }
                if(s.StartsWith("PHONE"))
                {
                    if (SurTemperature == "")
                    {
                        toClientsocket.Send(Encoding.UTF8.GetBytes("硬件客户端还未连接服务器"));
                        ClientList.lst.Clear();
                        dataGridView1.Rows.Clear();
                    }
                    else
                    {
                        toClientsocket.Send(Encoding.UTF8.GetBytes(s1));
                    }                  
                }
                if (s.StartsWith("PC"))
                {
                    if (SurTemperature == "")
                    {
                        toClientsocket.Send(Encoding.Default.GetBytes("硬件客户端还未连接服务器"));
                        ClientList.lst.Clear();
                        dataGridView1.Rows.Clear();
                    }
                    else
                    {
                        toClientsocket.Send(Encoding.Default.GetBytes(s1));
                    }
                }
                    toClientsocket.BeginReceive(rec_Message.Data, rec_Message.StartIndex, rec_Message.RemindSize, SocketFlags.None, ReceiveCallBack, toClientsocket);//继续监听来自客户端的消息
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                if (toClientsocket != null)
                {
                    toClientsocket.Close();
                }
            }
            finally
            {
            }
        }
    }
}
