using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RJCP.IO.Ports;
using System.IO;
using Ini;

namespace DS28E05_ID_Reader
{
    class Program
    {
        static SerialPortStream openPort;
        static bool _continue;
        //static bool DeviceFound = false;

        static void Main(string[] args)
        {
            Console.WriteLine("DS28E05 Unique ID reader\n");
            Console.WriteLine("Loading port configuration...\n");

            /* getting list of available ports
            foreach (string port in GetAllPorts())
            {
                Console.WriteLine(port);
            }
            

            //For manual com port selection
            //Console.WriteLine("\rSelect port to open: ");

            //string com = Console.ReadLine();
            
            string com = SearchDevice();
            if (com != null) Console.WriteLine("Device found on port " + com);
            else 
            {
                Console.WriteLine("Device not found!");
                return;
            }
            */

            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

            try
            {
                openPort = new SerialPortStream(LoadComPort(), 9600, 8, RJCP.IO.Ports.Parity.None, RJCP.IO.Ports.StopBits.One);
                openPort.DataReceived += Serialport_DataReceived;

                // Set the read/write timeouts
                openPort.ReadTimeout = 500;
                openPort.WriteTimeout = 500;

                if (openPort.IsOpen == false) //if not open, open the port
                    openPort.Open();
                else
                {
                    Console.WriteLine("Port is already opened! Aborting...\n");
                    return;
                }
                _continue = true;

                //Console.WriteLine("Type QUIT to exit");
                string message = "";

                while (_continue)
                {
                    message = "E3 C9";
                    byte[] bytes = message.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                    openPort.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(200);

                    message = "E1 CC";
                    bytes = message.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                    openPort.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(200);

                    message = "F0 78 00";
                    bytes = message.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                    openPort.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(200);

                    message = "FF FF FF FF FF FF FF FF";
                    bytes = message.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                    openPort.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(200);

                    _continue = false;

                    return;

                    message = Console.ReadLine();

                    if (stringComparer.Equals("quit", message))
                    {
                        _continue = false;
                    }
                    else
                    {
                        //byte[] bytes = message.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray(); //Enable this to output to byte
                        //openPort.Write(bytes, 0, bytes.Length);

                        openPort.Write(message);
                    }
                }

                openPort.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening the port! " + ex.Message);
            }
            
        }

        /*
        static List<string> GetAllPorts()
        {
            List<String> allPorts = new List<String>();
            foreach (String portName in SerialPortStream.GetPortNames())
            {
                allPorts.Add(portName);
            }
            return allPorts;
        }
        */

        static void Serialport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            try
            {
                int message;
                int i = 0;
                while(openPort.BytesToRead > 0)
                {
                    message = openPort.ReadByte();
                    i++;

                    Console.Write(message.ToString("X2"));
                    Console.Write(message.ToString(" "));
                }

                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                ///project specific code here...
            }
        }

        /*
        /// <summary>
        /// Search for DS28E05 device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Serialport_DataReceived_Search(object sender, SerialDataReceivedEventArgs e)
        {

            try
            {
                string response = "";
                int message;
                int i = 0;
                while (openPort.BytesToRead > 0)
                {
                    message = openPort.ReadByte();
                    i++;

                    response += message.ToString("X2");
                }

                if (response == "F5") DeviceFound = true;
            }
            catch (Exception)
            {
                ///project specific code here...
            }
        }

        static string SearchDevice()
        {            
            List<string> devices = GetAllPorts();

            foreach(string device in devices)
            {
                openPort = new SerialPortStream(device, 9600, 8, Parity.None, StopBits.One);
                openPort.DataReceived += Serialport_DataReceived_Search;
                // Set the read/write timeouts
                openPort.ReadTimeout = 500;
                openPort.WriteTimeout = 500;
                openPort.Open();

                int count = 0;
                openPort.Write("b");
                Thread.Sleep(200);
                string message = "E3 C9";
                byte[] bytes = message.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                openPort.Write(bytes, 0 , bytes.Length);
                Thread.Sleep(200);

                while (count < 1000000)
                {
                    if (DeviceFound)
                    {
                        openPort.Close(); openPort.Dispose(); openPort = null;
                        return device;
                    }
                    count++;
                }
                
                openPort.Close(); openPort.Dispose(); openPort = null;
            }
            return null;
        }
        */

        static string LoadComPort()
        {
            try
            {
                //get local path of configuration
                string path = Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\config.ini";

                //load configuration
                IniFile ini = new IniFile(path);
                return ini.IniReadValue("Setting", "Port");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error when loading configuration file. " + ex.Message + "\n");
                return null;
            }
        }
    }
}
