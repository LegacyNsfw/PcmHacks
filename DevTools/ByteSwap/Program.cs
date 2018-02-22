using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteSwap
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
                    Console.WriteLine("    ByteSwap <input-file> [output-file]");
                    Console.WriteLine();
                    Console.WriteLine("input-file: name of the file to read from. Required.");
                    Console.WriteLine("output-file: name of the file to write to. Optional.");
                    Console.WriteLine();
                    Console.WriteLine("If no output file name is provided, the input name will be used, with '.swap' appended.");
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
                    outputFile = inputFile + ".swap";
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
                while (true)
                {
                    int b1 = input.ReadByte();
                    if (b1 == -1)
                    {
                        return;
                    }

                    int b2 = input.ReadByte();
                    if (b2 == -1)
                    {
                        Console.WriteLine("The input file is not an even number of bytes.");
                        return;
                    }

                    output.WriteByte((byte)b2);
                    output.WriteByte((byte)b1);
                }
            }
        }
    }
}
