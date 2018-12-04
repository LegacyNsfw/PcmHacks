/*
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SRecordToKernel
{
    /// <summary>
    /// Reads SRecord objects from an SRecord file.
    /// </summary>
    class SRecordReader : IDisposable
    {
        private string path;
        private StreamReader reader;
        private int lineNumber;
        private int recordCount;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SRecordReader(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Disposes the underlying StreamReader.
        /// </summary>
        public void Dispose()
        {
            if (this.reader != null)
            {
                this.reader.Dispose();
                this.reader = null;
            }
        }

        /// <summary>
        /// Open the file reader.
        /// </summary>
        public void Open()
        {
            this.reader = new StreamReader(path, Encoding.ASCII);
        }

        /// <summary>
        /// Close and re-open the reader.
        /// </summary>
        public void Reset()
        {
            this.reader.Dispose();
            this.Open();
        }

        /// <summary>
        /// Read the next record from an SRecord file.
        /// </summary>
        /// <returns>True if there are more records, false if EOF.</returns>
        public bool TryReadNextRecord(out SRecord record)
        {
            string line = this.reader.ReadLine();
            if (line == null)
            {
                record = null;
                return false;
            }

            this.lineNumber++;

            if (line == string.Empty)
            {
                record = new SRecord('?', line);
                return true;
            }

            char s = line[0];
            if (s != 'S')
            {
                record = new SRecord('?', line);
                return true;
            }

            char typeCode = line[1];
            int addressBytes = 0;
            bool header = false;
            bool data = false;
            bool totalCount = false;
            bool startAddress = false;

            switch (typeCode)
            {
                case '0':
                    addressBytes = 2;
                    header = true;
                    break;

                case '1':
                    addressBytes = 2;
                    data = true;
                    break;

                case '2':
                    addressBytes = 3;
                    data = true;
                    break;

                case '3':
                    addressBytes = 4;
                    data = true;
                    break;

                case '5':
                    addressBytes = 2;
                    totalCount = true;
                    break;

                case '7':
                    addressBytes = 4;
                    startAddress = true;
                    break;

                case '8':
                    addressBytes = 3;
                    startAddress = true;
                    break;

                case '9':
                    addressBytes = 2;
                    startAddress = true;
                    break;

                default:
                    record = new SRecord(typeCode, line, lineNumber);
                    return true;
            }

            int index = 2;
            int checksum = 0;
            int count = GetByte(line, ref index, ref checksum);

            uint address = GetByte(line, ref index, ref checksum);
            address <<= 8;

            address += GetByte(line, ref index, ref checksum);

            if (addressBytes >= 3)
            {
                address <<= 8;
                address += GetByte(line, ref index, ref checksum);
            }

            if (addressBytes == 4)
            {
                address <<= 8;
                address += GetByte(line, ref index, ref checksum);
            }

            if (startAddress)
            {
                record = new SRecord(typeCode, address);
                this.recordCount++;
                return true;
            }

            if (totalCount)
            {
                record = new SRecord(typeCode, this.recordCount, (int) address);
                return true;
            }

            List<byte> bytes = new List<byte>();
            StringBuilder headerBuilder = new StringBuilder();
            
            for (int payloadIndex = 0; payloadIndex < count - (addressBytes + 1); payloadIndex++)
            {
                byte temp = GetByte(line, ref index, ref checksum);
                bytes.Add(temp);
            }

            int unused = 0;
            byte actualChecksum = (byte) (0xFF & checksum);
            actualChecksum ^= 0xFF;
            byte expectedChecksum = GetByte(line, ref index, ref unused);

            if (actualChecksum != expectedChecksum)
            {
                string message = string.Format(
                    "On line {0}, the actual checksum is 0x{1:X2}, but should be 0x{2:X2}",
                    lineNumber,
                    actualChecksum,
                    expectedChecksum);
                throw new System.Exception(message + Environment.NewLine + line);
            }

            if (header)
            {
                byte[] array = bytes.ToArray();
                string headerText = ASCIIEncoding.ASCII.GetString(array);
                record = new SRecord(typeCode, headerText);
                this.recordCount++;
                return true;
            }

            if (data)
            {
                List<string> byteStrings = new List<string>();
                foreach (byte b in bytes)
                {
                    byteStrings.Add(b.ToString("X2"));
                }

                record = new SRecord(typeCode, address, bytes.ToArray());
                this.recordCount++;
                return true;
            }

            throw new NotSupportedException("SRecordParser is confused by this line: " + line);
        }

        /// <summary>
        /// Get a single byte from an SRecord row.
        /// </summary>
        private static byte GetByte(string record, ref int index, ref int checksum)
        {
            if (record.Length < index + 1)
            {
                Console.WriteLine("index out of bounds");
                return 0;
            }

            char c1 = record[index];
            index++;
            char c2 = record[index];
            index++;

            string s = string.Format("{0}{1}", c1, c2);
            byte b = byte.Parse(s, System.Globalization.NumberStyles.HexNumber);
            checksum += b;
            return b;
        }
    }
}
