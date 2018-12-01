using System;
using System.IO;

namespace SRecordToKernel
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: SRecordToKernel <input_srecord_file> <output_kernel_file>");
                return;
            }

            string inputPath = args[0];
            string outputPath = args[1];

            using (Stream output = File.OpenWrite(outputPath))
            {
                SRecord record;
                SRecordReader reader = new SRecordReader(inputPath);
                reader.Open();
                while (reader.TryReadNextRecord(out record))
                {
                    Console.Write(record.ToString());
                    Console.Write(" - ");

                    if (!record.IsValid)
                    {
                        Console.WriteLine("Giving up.");
                        return;
                    }

                    if (record.Address == 0)
                    {
                        Console.WriteLine("Skipping, no payload.");
                    }

                    if (record.Address < 0xFF9000)
                    {
                        Console.WriteLine("Skipping, address too low.");
                        continue;
                    }

                    if (record.Address >= 0xFFC000)
                    {
                        Console.WriteLine("Skipping, address too high.");
                        continue;
                    }

                    Console.WriteLine("Writing");
                    output.Write(record.Payload, 0, record.Payload.Length);
                }
            }
        }
    }
}
