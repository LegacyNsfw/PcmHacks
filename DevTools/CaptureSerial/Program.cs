using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptureSerial
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string portName;
                int baud;
                string fileName;
                if (!Program.TryParseParameters(args, out portName, out baud, out fileName))
                {
                    Console.WriteLine();
                    Console.WriteLine("Usage instructions:");
                    Console.WriteLine();
                    Console.WriteLine("    CaptureSerial <port-name> <baud> [file-name]");
                    Console.WriteLine();
                    Console.WriteLine("port-name: name of the serial port, e.g. COM1, COM2, COM3, etc. Required.");
                    Console.WriteLine("baud: Baud rate, e.g. 9600, 19200, etc. Required.");
                    Console.WriteLine("file-name: name of the file to write to. Optional.");
                    Console.WriteLine("If no file name is provided, a name based on the date and time will be used.");
                    Console.WriteLine("For example, 20171231_235959.txt");
                    Console.WriteLine("Available ports are: " + string.Join(", ", SerialPort.GetPortNames()));
                    return;
                }

                Program.Capture(portName, baud, fileName);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                Console.WriteLine("Press enter.");
                Console.ReadLine();
            }
        }

        private static bool TryParseParameters(string[] args, out string portName, out int baud, out string fileName)
        {
            switch (args.Length)
            {
                case 2:
                    portName = args[0];

                    if (!int.TryParse(args[1], out baud))
                    {
                        fileName = null;
                        return false;
                    }

                    fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                    return true;

                case 3:
                    portName = args[0];

                    if (!int.TryParse(args[1], out baud))
                    {
                        fileName = null;
                        return false;
                    }

                    fileName = args[2];
                    return true;

                default:
                    portName = null;
                    baud = 0;
                    fileName = null;
                    return false;
            }
        }

        private static void Capture(string portName, int baud, string fileName)
        {
            Console.WriteLine("Press any key to stop.");
            byte[] buffer = new byte[1024 * 64];
            int totalBytes = 0;

            bool useCallback = false;

            using (Stream output = File.OpenWrite(fileName))
            using (SerialPort port = new SerialPort(portName, baud, Parity.None, 8, StopBits.One))
            { 
                if (useCallback)
                {
                    port.DataReceived +=
                        async delegate (object sender, SerialDataReceivedEventArgs e)
                        {
                            int bytes = Math.Min(port.BytesToRead, buffer.Length);
                            totalBytes += bytes;
                            port.Read(buffer, 0, bytes);
                            await output.WriteAsync(buffer, 0, bytes);
                        };
                }

                port.ReadTimeout = 500;
                port.Open();
                Console.WriteLine("Port {0} opened at {1} baud.", portName, baud);

                while (!Console.KeyAvailable)
                {
                    if (!useCallback)
                    {
                        try
                        {
                            int bytesReceived = port.Read(buffer, 0, 25);// buffer.Length);
                            if (bytesReceived > 0)
                            {
                                totalBytes += bytesReceived;
                                output.Write(buffer, 0, bytesReceived);
                            }
                        }
                        catch (TimeoutException)
                        {

                        }
                    }

                    System.Threading.Thread.Sleep(100);
                    Console.CursorLeft = 0;
                    Console.Write(string.Format("{0} bytes captured.  ", totalBytes));
                }

                port.Close();

                // Let the last buffer flush.
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}
