using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace CasitServer
{
    #region 异步处理类
    // State object for reading client data asynchronously     
    public class StateObject
    {
        // Client socket.     
        public Socket workSocket = null;
        // Size of receive buffer.     
        public const int BufferSize = 1024;
        // Receive buffer.     
        public byte[] buffer = new byte[BufferSize];
        // Received data string.     
        public StringBuilder sb1 = new StringBuilder();
    }
    #endregion
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btnConnect.Click += new RoutedEventHandler(btnConnect_Click);
            btnCloseConnect.Click += new RoutedEventHandler(btnCloseConnect_Click);
            tbIP.IsEnabled = false;
            ipHostInfo = Dns.Resolve(Dns.GetHostName());
            Ip = ipHostInfo.AddressList[0];
            tbIP.Text = Ip.ToString();
        }

        #region init
        private IPHostEntry ipHostInfo = new IPHostEntry();
        private IPAddress Ip;
        int PortStore = 19841;
        TcpListener TcpList;
        Thread thread;
        //AsynchronousSocketListener acsl = new AsynchronousSocketListener();
        DatabaseControl dbc = new DatabaseControl();
        //private TcpClient tcpClient = null;
        string[] userInfo;  //get user register info
        #endregion

        private void btnConnect_Click(object sender,RoutedEventArgs e)
        {
            PortStore = int.Parse(tbPort.Text);
            thread = new Thread(new ThreadStart(SynchronousProcessing));
            thread.Start();
            lbConnectInfo.Items.Add("端口监听中...");
            btnConnect.IsEnabled = false;
            btnCloseConnect.IsEnabled = true;
            
            #region 同步方式1
            //lbState.Text = "连接中......";
            //try
            //{
            //    //IPAddress Ip = IPAddress.Parse(tbIP.Text.ToString());
            //    TcpListener TcpList = new TcpListener(Ip, int.Parse(tbPort.Text));
            //    TcpList.Start();
                
            //    //MessageBox.Show("连接成功！");
            //    lbState.Text = "连接成功，Server start!";
            //    lbConnectInfo.Items.Add("Server Ip address:" + TcpList.LocalEndpoint);//get server ip and port
            //    while (true)
            //    {
            //        Socket soc = TcpList.AcceptSocket();
            //        lbConnectInfo.Items.Add("Received Connection:" + soc.RemoteEndPoint);//get client ip and port
            //        byte[] b = new byte[100];
            //        int k = soc.Receive(b); //数据的长度
            //        string str = string.Empty;
            //        str = Encoding.BigEndianUnicode.GetString(b,0,k);
            //        //str = str.Replace("\0", "");
            //        UnicodeEncoding AS = new UnicodeEncoding();
            //        if (str.Contains(':'))
            //        {
            //            userInfo = SubStr(str);
            //            if (!dbc.CheckSameID(userInfo[0], "IDnumber", dbc.GetTableNameUserInfomation()))
            //            {
            //                if (dbc.AddUser(userInfo))
            //                {
            //                    lbConnectInfo.Items.Add(str);
            //                    soc.Send(AS.GetBytes("Register Success!"));
            //                }
            //                else
            //                {
            //                    soc.Send(AS.GetBytes("Register failed!"));
            //                }
            //            }
            //            else
            //            { 
            //                soc.Send(AS.GetBytes("Id already exists"));
            //            }
            //            Array.Clear(userInfo, 0, userInfo.Length - 1);
            //        }
            //        else if (str.Contains('+'))
            //        {
            //            userInfo = SubStrAdmin(str);
            //            if (dbc.Login(userInfo[0], userInfo[1], dbc.GetTableNameAdministrator()) == "Success")
            //            {
            //                soc.Send(AS.GetBytes("Admin Login Success!"));
            //            }
            //            else
            //            { soc.Send(AS.GetBytes("Admin Login Failed!失败")); }
            //            Array.Clear(userInfo, 0, userInfo.Length - 1);
            //        }
            //        else if (str.Contains('~'))
            //        {
            //            userInfo = SubStrUser(str);
            //            if (dbc.Login(userInfo[0], userInfo[1], dbc.GetTableNameUserInfomation()) == "Success")
            //            {
            //                soc.Send(AS.GetBytes("User Login Success!"));
            //            }
            //            else
            //            { soc.Send(AS.GetBytes("User Login Failed!")); }
            //            Array.Clear(userInfo, 0, userInfo.Length - 1);
            //        }
            //        else
            //        {
            //            soc.Send(AS.GetBytes("Login Failed"));
            //            Array.Clear(userInfo, 0, userInfo.Length - 1);
            //        }
            //        //soc.Close();
            //    }
            //    TcpList.Stop();
            //}
            //catch (Exception exc)
            //{  }
            #endregion
        }
        public void btnCloseConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TcpList != null)
                {
                    //TcpList.Stop();
                    thread.Abort();
                    TcpList.Stop();
                }
                // 重新开启一个线程等待新的连接
                //Thread acceptThread = new Thread(SynchronousProcessing);
                //acceptThread.Start();
                btnConnect.IsEnabled = true;
                btnCloseConnect.IsEnabled = false;
            }
            catch(Exception exc)
            {
                //MessageBox.Show(exc.Message);
            }
        }


        #region 同步处理接收的字符串处理方法
        private string[] SubStr(string str)/*截取从客户端传的字符串，用于判断username，password,用于注册用户*/
        {
            string[] Str = new string[4];
            int a = str.IndexOf(":");
            Str[0] = str.Substring(0, a);/*ID*/
            str = str.Remove(0, a + 1);
            a = str.IndexOf(":");
            Str[1] = str.Substring(0, a);/*username*/
            str = str.Remove(0, a + 1);
            a = str.IndexOf(":");
            Str[2] = str.Substring(0, a);/*password*/
            Str[3] = str.Substring(a + 1);/*  */
            return Str;
        }

        private string[] SubStrUser(string str)
        {
            string[] Str = new string[2];
            int a = str.IndexOf("~");
            Str[0] = str.Substring(0, a);
            Str[1] = str.Substring(a + 1);
            return Str;
        }

        private string[] SubStrAdmin(string str)
        {
            string[] Str = new string[2];
            int a = str.IndexOf("+");
            Str[0] = str.Substring(0, a);
            Str[1] = str.Substring(a + 1);
            return Str;
        }
        #endregion
        #region 同步处理方法
        public void SynchronousProcessing()
        {
            //this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {  })); 
            try
            {
                //IPAddress Ip = IPAddress.Parse(tbIP.Text.ToString());
                //TcpListener TcpList = new TcpListener(Ip, int.Parse(tbPort.Text));
                TcpList = new TcpListener(Ip, PortStore);
                TcpList.Start();

                //MessageBox.Show("连接成功！");
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { lbConnectInfo.Items.Add("Server Ip address:" + TcpList.LocalEndpoint);}));
                while (true)
                {
                    Socket soc = TcpList.AcceptSocket();
                    //get client ip and port
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { lbConnectInfo.Items.Add("Received Connection:" + soc.RemoteEndPoint + "  " + DateTime.Now.ToString()); }));
                    byte[] b = new byte[100];
                    int k = soc.Receive(b); //数据的长度
                    string str = string.Empty;
                    str = Encoding.Unicode.GetString(b, 0, k);
                    //str = str.Replace("\0", "");
                    UnicodeEncoding AS = new UnicodeEncoding();
                    if (str.Contains(':'))
                    {
                        userInfo = SubStr(str);
                        if (!dbc.CheckSameID(userInfo[0], "IDnumber", dbc.GetTableNameUserInfomation()))
                        {
                            if (dbc.AddUser(userInfo))
                            {
                                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { lbConnectInfo.Items.Add(str); }));
                                soc.Send(AS.GetBytes("Register Success!"));
                            }
                            else
                            {
                                soc.Send(AS.GetBytes("Register failed!"));
                            }
                        }
                        else
                        {
                            soc.Send(AS.GetBytes("Id already exists"));
                        }
                        Array.Clear(userInfo, 0, userInfo.Length - 1);
                    }
                    else if (str.Contains('+'))
                    {
                        userInfo = SubStrAdmin(str);
                        if (dbc.Login(userInfo[0], userInfo[1], dbc.GetTableNameAdministrator()) == "Success")
                        {
                            soc.Send(AS.GetBytes("Admin Login Success!"));
                        }
                        else
                        { soc.Send(AS.GetBytes("Admin Login Failed!")); }
                        Array.Clear(userInfo, 0, userInfo.Length - 1);
                    }
                    else if (str.Contains('~'))
                    {
                        userInfo = SubStrUser(str);
                        if (dbc.Login(userInfo[0], userInfo[1], dbc.GetTableNameUserInfomation()) == "Success")
                        {
                            soc.Send(AS.GetBytes("User Login Success!"));
                        }
                        else
                        { soc.Send(AS.GetBytes("User Login Failed!")); }
                        Array.Clear(userInfo, 0, userInfo.Length - 1);
                    }
                    else
                    {
                        soc.Send(AS.GetBytes("Login Failed"));
                        if (userInfo != null)
                        {
                            Array.Clear(userInfo, 0, userInfo.Length - 1);
                        }
                    }
                    //soc.Close();
                }
                //TcpList.Stop();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
        #endregion
        #region  异步处理方法
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public void StartListening()
        {
            // Data buffer for incoming data.     
            byte[] bytes = new Byte[1024];
            // Establish the local endpoint for the socket.     
            // The DNS name of the computer     
            // running the listener is "host.contoso.com".     
            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = IPAddress.Parse("192.168.1.77");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 19841);
            // Create a TCP/IP socket.     
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the local     
            //endpoint and listen for incoming connections.     
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    // Set the event to nonsignaled state.     
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections.     
                    //Console.WriteLine("Waiting for a connection...");
                    lbConnectInfo.Items.Add("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    // Wait until a connection is made before continuing.     
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.     
            allDone.Set();
            // Get the socket that handles the client request.     
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            // Create the state object.     
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            // Retrieve the state object and the handler socket     
            // from the asynchronous state object.     
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            // Read data from the client socket.     
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.     
                state.sb1.Append(Encoding.Unicode.GetString(state.buffer, 0, bytesRead));
                // Check for end-of-file tag. If it is not there, read     
                // more data.     
                content = state.sb1.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the     
                    // client. Display it on the console.     
                    //Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                    lbConnectInfo.Items.Add("Read" + content.Length + "bytes from socket. \n Data : " + content);
                    // Echo the data back to the client.     
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.     
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }
        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.     
            byte[] byteData = Encoding.Unicode.GetBytes(data);
            // Begin sending the data to the remote device.     
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.     
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.     
                int bytesSent = handler.EndSend(ar);
                lbConnectInfo.Items.Add("Sent " + bytesSent +" bytes to client.");
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion
    }
    
}
