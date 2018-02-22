using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class is responsible for sending and receiving data over a serial port.
    /// I would have called it 'SerialPort' but that name was already taken...
    /// </summary>
    class StandardPort : IPort
    {
        private string name;
        private SerialPort port;

        public StandardPort(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// This returns the string that appears in the drop-down list.
        /// </summary>
        public override string ToString()
        {
            return this.name;
        }

        /// <summary>
        /// Open the serial port.
        /// </summary>
        Task IPort.Open()
        {
            this.port = new SerialPort(this.name);
            this.port.Open();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Close the serial port.
        /// </summary>
        public void Dispose()
        {
            this.port.Dispose();
        }

        /// <summary>
        /// Send a sequence of bytes over the serial port.
        /// </summary>
        async Task IPort.Send(byte[] buffer)
        {
            await this.port.BaseStream.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Receive a sequence of bytes over the serial port.
        /// </summary>
        async Task<int> IPort.Receive(byte[] buffer, int offset, int count)
        {
            return await this.port.BaseStream.ReadAsync(buffer, offset, count);
        }
    }
}

