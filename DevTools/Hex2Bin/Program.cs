using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hex2Bin
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string inputFile;
                string outputFile;

                if (!Program.TryParseParameters(args, out inputFile, out outputFile))
                {
                    Console.WriteLine();
                    Console.WriteLine("Usage instructions:");
                    Console.WriteLine();
                    Console.WriteLine("    Hex2Bin <input-file> [output-file]");
                    Console.WriteLine();
                    Console.WriteLine("input-file: name of the file to read from. Required.");
                    Console.WriteLine("output-file: name of the file to write to. Optional.");
                    Console.WriteLine();
                    Console.WriteLine("If no output file name is provided, the input name will be used, with '.bin' appended.");
                    return;
                }

                Program.Convert(inputFile, outputFile);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        private static bool TryParseParameters(string[] args, out string inputFile, out string outputFile)
        {
            switch (args.Length)
            {
                case 1:
                    inputFile = args[0];
                    outputFile = inputFile + ".bin";
                    return true;

                case 2:
                    inputFile = args[0];
                    outputFile = args[1];
                    return true;

                default:
                    inputFile = null;
                    outputFile = null;
                    return false;
            }
        }

        private static void Convert(string inputFile, string outputFile)
        {
            File.Delete(outputFile);

            using (Stream input = File.OpenRead(inputFile))
            using (Stream output = File.OpenWrite(outputFile))
            {
                int state = 0;
                int newCharacter = 0;
                int newByte = 0;

                while((newCharacter = input.ReadByte()) != -1)
                {
                    int hex = Program.GetHex((char)newCharacter);
                    if (hex == -1)
                    {
                        state = 0;
                        continue;
                    }
                    else
                    {
                        switch (state)
                        {
                            case 0:
                                newByte = hex;
                                state = 1;
                                continue;

                            case 1:
                                newByte <<= 4;
                                newByte += hex;
                                output.WriteByte((byte)newByte);
                                state = 0;
                                break;
                        }
                    }
                }
            }
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
