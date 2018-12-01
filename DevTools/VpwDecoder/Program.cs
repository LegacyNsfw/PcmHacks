using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VpwDecoder
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string inputFile;

                if (!Program.TryParseParameters(args, out inputFile))
                {
                    Console.WriteLine();
                    Console.WriteLine("Usage instructions:");
                    Console.WriteLine();
                    Console.WriteLine("    VpwDecoder <input-file>");
                    Console.WriteLine();
                    Console.WriteLine("input-file: name of the file to read from. Required.");
                    Console.WriteLine();
                    Console.WriteLine("Decoded results will be written to files with names derived from the input file name.");
                    return;
                }

                if (inputFile.StartsWith("COM"))
                {
                    Program.DecodeFromSerial(inputFile);
                }
                else
                {
                    Program.DecodeFromFile(inputFile);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        private static bool TryParseParameters(string[] args, out string inputFile)
        {
            switch (args.Length)
            {
                case 1:
                    inputFile = args[0];
                    return true;
                    
                default:
                    inputFile = null;
                    return false;
            }
        }

        private static void DecodeFromSerial(string portName)
        {
            SerialPort port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            port.Open();
            SendCommand(port, "AT AL");
            SendCommand(port, "AT SP2");
            SendCommand(port, "AT AR");
            SendCommand(port, "AT ST 20");
            SendCommand(port, "AT MA");
            string line = string.Empty;
            while (line != null)
            {
                line = port.ReadLine();
                Console.WriteLine(line ?? string.Empty);
            }
        }

        private static void SendCommand(SerialPort port, string message)
        {
            Console.WriteLine(message);
            byte[] buffer = Encoding.ASCII.GetBytes(message + "\r\n");
            port.Write(buffer, 0, buffer.Length);
            string response = ReadElmLine(port);
            Console.WriteLine(response);
        }

        /// <summary>
        /// Reads and filteres a line from the device, returns it as a string
        /// </summary>
        /// <remarks>
        /// Strips Non Printable, >, and Line Feeds. Converts Carriage Return to Space. Strips leading and trailing whitespace.
        /// </remarks>
        private static string ReadElmLine(SerialPort port)
        {
            const int MaxReceiveSize = 512;
            int buffersize = (MaxReceiveSize * 3) + 7; // payload with spaces (3 bytes per character) plus the longest possible prompt
            byte[] buffer = new byte[buffersize];

            // collect bytes until we hit the end of the buffer or see a CR or LF
            int i = 0;
            int b;
            do
            {
                b = port.ReadByte();
                if (b == -1)
                {
                    break;
                }

                buffer[i] = (byte)b;
                i++;
            } while ((i < buffersize) && (b != (byte)'>')); // continue until the next prompt

            //this.Logger.AddDebugMessage("Found terminator '>'");

            // count the wanted bytes and replace CR with space
            int wanted = 0;
            int j;
            for (j = 0; j < i; j++)
            {
                if (buffer[j] == 13) buffer[j] = 32; // CR -> Space
                if (buffer[j] >= 32 && buffer[j] <= 126 && buffer[j] != '>') wanted++; // printable characters only, and not '>'
            }

            //this.Logger.AddDebugMessage(wanted + " bytes to keep from " + i);

            // build a message of the correct length
            // i is the length of the data in the original buffer
            // j is pointer to the offset in the filtered buffer
            // k is the pointer in to the original buffer
            int k;
            byte[] filtered = new byte[wanted]; // create a new filtered buffer of the correct size
            for (k = 0, j = 0; k < i; k++) // start both buffers from 0, always increment the original buffer 
            {
                if (buffer[k] >= 32 && buffer[k] <= 126 && buffer[k] != '>') // do we want THIS byte?
                {
                    b = buffer[k];
                    //this.Logger.AddDebugMessage("Filtered Byte: " + buffer[k].ToString("X2") + " Ascii: " + System.Text.Encoding.ASCII.GetString(b));
                    filtered[j++] = buffer[k];  // save it, and increment the pointer in the filtered buffer
                }
            }

            //this.Logger.AddDebugMessage("built filtered string kept " + j + " bytes filtered is " + filtered.Length + " long");
            //this.Logger.AddDebugMessage("filtered: " + filtered.ToHex());
            string line = System.Text.Encoding.ASCII.GetString(filtered).Trim(); // strip leading and trailing whitespace, too

            //this.Logger.AddDebugMessage("Read \"" + line + "\"");

            return line;
        }

        private static void DecodeFromFile(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                char[] whitespace = new char[] { ' ' };
                int lineNumber = 0;
                string line;

                Parser parser = new Parser(fileName, 1);

                while (true)
                {
                    lineNumber++;
                    line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    line = line.Trim();
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    string[] hexStrings = line.Split(whitespace);
                    for (int index = 0; index < hexStrings.Length; index++)
                    {
                        string hex = hexStrings[index];
                        if (hex.Length != 2)
                        {
                            Console.WriteLine("Line {0} byte size syntax error: {1}", lineNumber, line);
                            continue;
                        }

                        int value = GetByte(hex[0], hex[1]);
                        if (value < 0)
                        {
                            Console.WriteLine("Line {0} byte value sytax error: {1}", lineNumber, line);
                            continue;
                        }

                        if (index == hexStrings.Length - 1)
                        {
                            if (parser.CheckCrc((byte)value))
                            {
                                parser.Dispose();
                                parser = new Parser(fileName, lineNumber);
                            }
                            else
                            {
                                parser.ToString();
                            }
                        }
                        else
                        {
                            parser.Push(hex, (byte)value);
                        }
                    }
                }

                Console.WriteLine("End of input file.");
            }
        }

        private static int GetByte(char c1, char c2)
        {
            return (GetHex(c1) << 4) + GetHex(c2);
        }
        
        private static int GetHex(char newCharacter)
        {
            if (newCharacter >= '0' && (newCharacter <= '9'))
            {
                return newCharacter - '0';
            }

            if (newCharacter >= 'a' && (newCharacter <= 'f'))
            {
                return 10 + (newCharacter - 'a');
            }

            if (newCharacter >= 'A' && (newCharacter <= 'F'))
            {
                return 10 + (newCharacter - 'A');
            }

            return -1;
        }
    }
}

