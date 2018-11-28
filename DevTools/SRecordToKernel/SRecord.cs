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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SRecordToKernel
{
    /// <summary>
    /// Represents a single row of a Motorola S-Record file.
    /// </summary>
    class SRecord
    {
        private readonly char TypeCode;

        /// <summary>
        /// If false, the row was corrupt.
        /// </summary>
        public readonly bool IsValid;

        /// <summary>
        /// Indicates whether the row contains an entry-point address.
        /// </summary>
        public readonly bool IsEntryPoint;

        /// <summary>
        /// Address of the data (or entry-point).
        /// </summary>
        public readonly uint Address;

        /// <summary>
        /// Content of a header record.
        /// </summary>
        public readonly string Header;

        /// <summary>
        /// Payload of a data record.
        /// </summary>
        public readonly byte[] Payload;

        /// <summary>
        /// Actual number of records in the file.
        /// </summary>
        public readonly int ActualRecordCount;

        /// <summary>
        /// Expected number of records in the file.
        /// </summary>
        public readonly int ExpectedRecordCount;

        /// <summary>
        /// Raw SRecord text.
        /// </summary>
        public readonly string RawData;

        /// <summary>
        /// Only used for invalid records - will be zero otherwise.
        /// </summary>
        public readonly int LineNumber;
        
        /// <summary>
        /// Constructor for data payload records.
        /// </summary>
        public SRecord(char typeCode, uint address, byte[] payload)
        {
            if ((typeCode != '0') &&
                (typeCode != '1') &&
                (typeCode != '2') &&
                (typeCode != '3'))
            {
                throw new InvalidOperationException("SRecord payload ctor only supports record types 0, 1, 2, and 3.");
            }

            this.TypeCode = typeCode;
            this.IsValid = true;
            this.Address = address;
            this.Payload = payload;
        }

        /// <summary>
        /// Constructor for entry-point records.
        /// </summary>
        public SRecord(char typeCode, uint address)
        {
            if ((typeCode != '7') &&
                (typeCode != '8') &&
                (typeCode != '9'))
            {
                throw new InvalidOperationException("SRecord entry-point ctor only supports record types 7, 8, and 9.");
            }

            this.TypeCode = typeCode;
            this.IsValid = true;
            this.IsEntryPoint = true;
            this.Address = address;
        }

        /// <summary>
        /// Constructor for header records.
        /// </summary>
        public SRecord(char typeCode, string header)
        {
            if (typeCode != '0')
            {
                throw new InvalidOperationException("SRecord header ctor only supports record type 0.");
            }

            this.TypeCode = typeCode;
            this.IsValid = true;
            this.Header = header;
        }

        /// <summary>
        /// Constructor for record-count records.
        /// </summary>
        public SRecord(char typeCode, int actualRecordCount, int expectedRecordCount)
        {
            if (typeCode != '5')
            {
                throw new InvalidOperationException("SRecord count ctor only supports record type 5.");
            }

            this.TypeCode = typeCode;
            this.ActualRecordCount = actualRecordCount;
            this.ExpectedRecordCount = expectedRecordCount;
        }

        /// <summary>
        /// Constructor for unsupported records.
        /// </summary>
        public SRecord(char typeCode, string rawData, int lineNumber)
        {
            this.TypeCode = typeCode;
            this.RawData = rawData;
            this.LineNumber = lineNumber;
        }

        /// <summary>
        /// Renders this record as a string, for human consumption.
        /// </summary>
        public override string ToString()
        {
            if (this.Header != null)
            {
                return "Header: " + this.Header;
            }

            if (this.IsEntryPoint)
            {
                return string.Format("Entry point: {0:X2}", this.Address);
            }

            if (this.Payload != null)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("Address: {0:X8}, Payload: ", this.Address);
                foreach (byte b in this.Payload)
                {
                    builder.Append(b.ToString("X2"));
                    builder.Append(" ");
                }
                return builder.ToString();
            }

            if (this.TypeCode == 5)
            {
                return string.Format(
                    "Actual record count: {0}, expected count {1}, match = {2}",
                    this.ActualRecordCount,
                    this.ExpectedRecordCount,
                    this.ActualRecordCount == this.ExpectedRecordCount);
            }

            return string.Format("Unsupported record type {0}, Contents: {1}", this.TypeCode, this.RawData);
        }
    }
}
