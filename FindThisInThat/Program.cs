using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThisInThat
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string pattern;
                string data;
                uint matchLength;

                if (!Program.TryParseParameters(args, out pattern, out data, out matchLength))
                {
                    Console.WriteLine();
                    Console.WriteLine("Usage instructions:");
                    Console.WriteLine();
                    Console.WriteLine("    FindThisInThat <pattern-file> <data-file> [run-length]");
                    Console.WriteLine();
                    Console.WriteLine("pattern-file: name of the file containing pattern to search for. Required.");
                    Console.WriteLine("data-file: name of the file containing data to search in. Required.");
                    Console.WriteLine("run-length: minimum number of bytes to consider a match.");
                    return;
                }

                Program.Search(pattern, data, matchLength);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        private static bool TryParseParameters(string[] args, out string pattern, out string data, out uint matchLength)
        {
            switch (args.Length)
            {
                case 2:
                    pattern = args[0];
                    data = args[1];
                    matchLength = 5;
                    return true;

                case 3:
                    pattern = args[0];
                    data = args[1];
                    if(!uint.TryParse(args[3], out matchLength))
                    {
                        return false;
                    }
                    return true;

                default:
                    pattern = null;
                    data = null;
                    matchLength = 0;
                    return false;
            }
        }

        private static void Search(string patternPath, string dataPath, uint requiredMatchLength)
        {
            using (Stream patternStream = File.OpenRead(patternPath))
            using (Stream dataStream = File.OpenRead(dataPath))
            {
                byte[] pattern = new byte[patternStream.Length];
                byte[] data = new byte[dataStream.Length];

                int patternLength = patternStream.Read(pattern, 0, pattern.Length);
                int dataLength = dataStream.Read(data, 0, data.Length);

                Console.WriteLine("Searching for {0} bytes in {1} bytes.", patternLength, dataLength);

                int patternIndex = 0;
                int matchLength = 0;
                int lastPatternIndex = 0;
                bool recordBlock = false;

                int matchDataStart = 0;
                int matchPatternStart = 0;
                bool inMatch = false;

                for (int dataIndex = 0; dataIndex < data.Length; dataIndex++)
                {
                    if (data[dataIndex] == pattern[patternIndex])
                    {
                        if (!inMatch)
                        {
                            matchDataStart = dataIndex;
                            matchPatternStart = patternIndex;
                            inMatch = true;
                        }

                        matchLength++;
                        patternIndex++;

                        if (patternIndex == pattern.Length)
                        {
                            Console.WriteLine("Reached end of data to search for.");
                            if (matchLength >= requiredMatchLength)
                            {
                                Console.WriteLine("Found {0} matching bytes starting at index {1} of pattern file, {2} of data file.", matchLength, matchPatternStart, matchDataStart);
                            }

                            return;
                        }

                        if (matchLength >= requiredMatchLength)
                        {
                            recordBlock = true;
                        }

                        continue;
                    }

                    inMatch = false;

                    if (recordBlock)
                    {
                        recordBlock = false;
                        Console.WriteLine("Found {0} matching bytes starting at index {1} of pattern file, {2} of data file.", matchLength, matchPatternStart, matchDataStart);
                        lastPatternIndex = patternIndex;
                    }

                    patternIndex = lastPatternIndex;
                    matchLength = 0;
                }

                Console.WriteLine("Reached end of data to search in.");
            }
        }
    }
}
