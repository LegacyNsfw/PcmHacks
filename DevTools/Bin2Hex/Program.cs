using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bin2Hex
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
                    Console.WriteLine("    Bin2Hex <input-file> [output-file]");
                    Console.WriteLine();
                    Console.WriteLine("input-file: name of the file to read from. Required.");
                    Console.WriteLine("output-file: name of the file to write to. Optional.");
                    Console.WriteLine();
                    Console.WriteLine("If no output file name is provided, the input name will be used, with '.hex.txt' appended.");
                    return;
                }

                Task.Run(async () =>
                {
                    await Program.Convert(inputFile, outputFile);
                }).GetAwaiter().GetResult();
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
                    outputFile = inputFile + ".hex.txt";
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

        private static async Task Convert(string inputFile, string outputFile)
        {
            File.Delete(outputFile);

            using (Stream input = File.OpenRead(inputFile))
            using (TextWriter output = new StreamWriter(File.OpenWrite(outputFile)))
            {
                int newByte = 0;
                int bytesWritten = 0;

                while ((newByte = input.ReadByte()) != -1)
                {
                    await output.WriteAsync(string.Format("{0:X2} ", newByte));
                    bytesWritten++;

                    // Consider writing newlines for VPW packets?
                    if(bytesWritten % 16 == 0)
                    {
                        await output.WriteLineAsync();
                    }
                }
            }
        }
    }
}
