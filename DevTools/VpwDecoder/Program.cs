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
            port.NewLine = "\r";
            port.Open();
            SendCommand(port, "AT Z"); // reset
            SendCommand(port, "AT AL");
            SendCommand(port, "AT SP2"); // set protocol to VPW
            SendCommand(port, "AT AR"); 
            SendCommand(port, "AT ST 20"); // timeout
            SendCommand(port, "AT H 1"); // show headers
            SendCommand(port, "AT S 0"); // no spaces
            SendCommand(port, "AT MA"); // start monitoring

            string line = string.Empty;
            DateTime lastMessage = DateTime.Now;
            while (line != null)
            {
                line = port.ReadLine();
                UInt32 elapsed = (UInt32) DateTime.Now.Subtract(lastMessage).TotalMilliseconds;
                lastMessage = DateTime.Now;

                if (line == null)
                {
                    break;
                }

                if ((line == "BUFFER FULL") || (line == "OUT OF MEMORY"))
                {
                    SendCommand(port, "AT MA", false);
                }

                line = line.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                line = Program.Decode(line);
                Console.WriteLine(string.Format("{0:D4} {1}", elapsed, line));

/*                while (Console.KeyAvailable)
                {
                    Console.ReadKey();
                    Console.WriteLine("Key pressed. Re-opening port.");
                    port.Close();
                    port.NewLine = "\r";
                    port.Open();
                }
*/
            }
        }

        private static void SendCommand(SerialPort port, string message, bool show = true)
        {
            Console.WriteLine(message);
            byte[] buffer = Encoding.ASCII.GetBytes(message + "\r\n");
            port.Write(buffer, 0, buffer.Length);
            string response = ReadElmLine(port);

            if (show)
            {
                Console.WriteLine(response);
            }
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

                    line = Decode(line);
                    Console.WriteLine(line);
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

        private static string Decode(string line)
        {
            if (!IsHex(line))
            {
                return line;
            }

            // This will be incremented before it gets used.
            int position = -1;

            byte priorityByte = 0;

            byte sourceByte = 0;
            string source = "";

            byte targetByte = 0;
            string target = "";

            byte modeByte = 0;
            string mode = "";

            byte submodeByte = 0;
            bool haveSubmode = false;

            int length = 0;
            int address = 0;

            List<byte> rawData = new List<byte>();
            List<byte> payload = new List<byte>();

            StringBuilder rawBuilder = new StringBuilder();
            StringBuilder tailBuilder = new StringBuilder();
            StringBuilder descriptionBuilder = new StringBuilder();
    
            byte b = 0;
            for (int index = 0; index < line.Length; index++)
            {
                char c = line[index];
                if (index % 2 == 0)
                {
                    b = CharToValue(c);
                    continue;
                }
                else
                {
                    b <<= 4;
                    b |= CharToValue(c);
                }

                position++;
                
                if (position < 12)
                {
                    rawBuilder.Append(b.ToString("X02"));
                }
                else if (position == 12)
                {
                    rawBuilder.Append("...");
                    tailBuilder.Append("  ...");
                    tailBuilder.Append(b.ToString("X02"));
                }
                else
                {
                    tailBuilder.Append(b.ToString("X02"));
                }

                // Skip the last character, it's just the checksum
                if (position >= ((line.Length / 2) - 1))
                {
                    if (!haveSubmode)
                    {
                        mode = Data.DecodeMode(priorityByte, modeByte, submodeByte, haveSubmode);
                        descriptionBuilder.Append(mode);
                    }

                    continue;
                }

                switch (position)
                {
                    case 0:
                        priorityByte = b;
                        descriptionBuilder.Append(b.ToString("X2"));
                        descriptionBuilder.Append(" ");
                        descriptionBuilder.Append(Data.DecodePriority(b));
                        descriptionBuilder.Append(", ");
                        break;

                    case 1:
                        targetByte = b;
                        target = Data.DecodeDevice(b);
                        break;

                    case 2:
                        sourceByte = b;
                        source = Data.DecodeDevice(b);
                        string route = string.Format("{0:X2} {1,-20} --> {2:X2} {3,-20}  ", sourceByte, source, targetByte, target);
                        descriptionBuilder.Append(route);
                        break;

                    case 3:
                        modeByte = b;
                        break;

                    case 4:
                        submodeByte = b;
                        haveSubmode = true;
                        mode = Data.DecodeMode(priorityByte, modeByte, submodeByte, haveSubmode);
                        descriptionBuilder.Append(string.Format("{0,-22}", mode));
                        break;

                    case 5:
                        if (MessageContainsAddressAndLength(modeByte, submodeByte))
                        {
                            length = b << 8;
                        }
                        else
                        {
                            payload.Add(b);
                        }
                        break;

                    case 6:
                        if (MessageContainsAddressAndLength(modeByte, submodeByte))
                        {
                            length |= b;
                            descriptionBuilder.Append(string.Format("  Length: {0:X4} ({0}), ", length, length));
                        }
                        else
                        {
                            payload.Add(b);
                        }
                        break;

                    case 7:
                        if (MessageContainsAddressAndLength(modeByte, submodeByte))
                        {
                            address = b << 16;
                        }
                        else
                        {
                            payload.Add(b);
                        }
                        break;

                    case 8:
                        if (MessageContainsAddressAndLength(modeByte, submodeByte))
                        {
                            address |= b << 8;
                        }
                        else
                        {
                            payload.Add(b);
                        }
                        break;

                    case 9:
                        if (MessageContainsAddressAndLength(modeByte, submodeByte))
                        {
                            address |= b;
                            descriptionBuilder.Append(string.Format("Address: {0:X06}", address));
                        }
                        else
                        {
                            payload.Add(b);
                        }
                        break;

                    default:
                        payload.Add(b);
                        break;
                }
            }

            return string.Format("{0,-30} {1} {2}", rawBuilder.ToString(), descriptionBuilder.ToString(), tailBuilder.ToString());
        }

        private static bool MessageContainsAddressAndLength(byte mode, byte submode)
        {
            switch(mode)
            {
                case 0x34:
                    return true;

                case 0x36:
                    return true;
            }

            return false;
        }
        
        private static byte CharToValue(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }

            if (c >= 'a' && c <= 'f')
            {
                return (byte)((c - 'a') + 10);
            }

            if (c >= 'A' && c <= 'F')
            {
                return (byte)((c - 'A') + 10);
            }

            throw new InvalidDataException("Can't convert '" + c + "' to hex.");
        }

        private static bool IsHex(string line)
        {
            foreach (char c in line)
            {
                if (char.IsNumber(c))
                {
                    continue;
                }

                if (c >= 'a' && c <= 'f')
                {
                    continue;
                }

                if (c >= 'A' && c <= 'F')
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}