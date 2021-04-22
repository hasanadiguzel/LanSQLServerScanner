using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LanSqlServerScanner
{
    class Program
    {
        internal static void Logger(string type, string message)
        {
            switch (type)
            {
                case "Success":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[Success]\t");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case "Error":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[Error]  \t");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case "Fail":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[Fail]   \t");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case "Warning":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[Warning]\t");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case "Info":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[Info]   \t");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                default:
                    break;
            }
            Console.WriteLine(message);
        }

        private static bool TestConnection_WithSocket(string ipAddress, int port, ProtocolType protocolType)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
            try
            {
                s.Connect(ipAddress, port);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                s.Disconnect(true);
                s.Dispose();
                s = null;
            }
        }

        private static bool TestConnection_WithTCP(string ipAddress, int port)
        {
            TcpClient tcp = null;
            try
            {
                tcp = new TcpClient(ipAddress, port);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TestConnection_WithUDP(string ipAddress, int port)
        {
            UdpClient tcp = null;
            try
            {
                tcp = new UdpClient(ipAddress, port);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static void Main(string[] args)
        {
            int portTCP_SqlServer = 1433, portUDP_SqlServer = 1434;

            Console.WriteLine("=========================================== LAN SQL Server Scanner ===========================================");
            Console.WriteLine("Developer: Hasan Adiguzel");
            Console.WriteLine("Version: 0.0.1");
            Console.WriteLine("Version Code: Kisa Zurefa");
            Console.WriteLine("Release Date: 22.04.2021");
            Console.WriteLine("");
            Console.WriteLine("Default TCP Port: 1433");
            Console.WriteLine("Default UDP Port: 1434");
            Console.WriteLine("==============================================================================================================");

            bool loopStatus = true;
            while (loopStatus)
            {
                Console.WriteLine("-Operations-");
                Console.WriteLine("1. Change TCP Port");
                Console.WriteLine("2. Change UPD Port");
                Console.WriteLine("3. Start LAN Scanning");
                Console.WriteLine("4. Quit Program");
                Console.WriteLine("");
                Console.Write("Select Operation: ");
                int operationNumber = Convert.ToInt32(Console.ReadLine());

                switch (operationNumber)
                {
                    case 1:
                        Console.Write("\nEnter the new TCP port: ");
                        try
                        {
                            portTCP_SqlServer = Convert.ToInt32(Console.ReadLine());
                            Logger("Info", "New TCP port: " + portTCP_SqlServer);
                        }
                        catch (System.FormatException)
                        {
                            Logger("Error", "Please enter a correct port number.");
                        }
                        break;
                    case 2:
                        Console.Write("\nEnter the new UDP port: ");
                        try
                        {
                            portUDP_SqlServer = Convert.ToInt32(Console.ReadLine());
                            Logger("Info", "New UDP port: " + portUDP_SqlServer);
                        }
                        catch (System.FormatException)
                        {
                            Logger("Error", "Please enter a correct port number.");
                        }
                        break;
                    case 3:
                        loopStatus = false;
                        break;
                    case 4:
                        Environment.Exit(0);
                        break;
                }
                Console.WriteLine("--------------------------------------------------------------------------------------------------------------");
            }


            #region ScanProcBegin
            Console.WriteLine("\t\tProcDate \t\t\t IPAddress \t Port \t Status");

            //Get my host name and IP Address
            string hostName = Dns.GetHostName();
            IPHostEntry myHostEntry = Dns.GetHostEntry(hostName);
            IPAddress[] myIPAddresses = myHostEntry.AddressList;
            string[] myNetworkAddress = myIPAddresses[myIPAddresses.Length - 1].ToString().Split('.');


            //Local Area Network Scanning
            using (Ping ping = new Ping())
            {
                for (int i = 0; i <= 255; i++)
                {
                    string selectedIPAddress = $"{myNetworkAddress[0]}.{myNetworkAddress[1]}.{myNetworkAddress[2]}.{i}";

                    PingReply pingReply = ping.Send(selectedIPAddress, 4000);
                    if (pingReply.Status == IPStatus.Success)
                    {
                        bool statusBit = false;
                        bool status = TestConnection_WithTCP(selectedIPAddress, portTCP_SqlServer);
                        if (status)
                        {
                            statusBit = true;
                            Logger("Success", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} \t {selectedIPAddress} \t {portTCP_SqlServer} \t TCP Connection Successful");
                        }

                        status = TestConnection_WithUDP(selectedIPAddress, portUDP_SqlServer);
                        if (status)
                        {
                            statusBit = true;
                            Logger("Success", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} \t {selectedIPAddress} \t {portUDP_SqlServer} \t UDP Connection Successful");
                        }

                        if (!statusBit)
                        {
                            Logger("Info", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} \t {selectedIPAddress} \t NULL \t Accessible Device");
                        }
                    }
                    else
                    {
                        Logger("Fail", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} \t {selectedIPAddress} \t NULL \t Inaccessible Device");
                    }
                }

                ping.Dispose();
            }
            #endregion


            Console.ReadLine();
        }
    }
}
