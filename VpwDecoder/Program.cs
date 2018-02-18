using System;
using System.Collections.Generic;
using System.IO;
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

                Program.Decode(inputFile);
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

        private static void Decode(string fileName)
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
                    string[] hexStrings = line.Split(whitespace);
                    using (Parser parser = new Parser(fileName, lineNumber))
                    {
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
                                parser.CheckCrc((byte)value);
                            }
                            else
                            {
                                parser.Push(hex, (byte)value);
                            }
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

