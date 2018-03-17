﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class provides a way to test most of the app without any interface hardware.
    /// </summary>
    class MockDevice : Device
    {
        public MockDevice(IPort port, ILogger logger) : base(port, logger)
        {

        }

        public override string ToString()
        {
            return "Mock Interface";
        }

        public override Task<bool> Initialize()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public override Task<bool> SendMessage(Message message)
        {
            StringBuilder builder = new StringBuilder();
            this.Logger.AddDebugMessage("Sending message " + message.GetBytes().ToHex());
            this.Port.Send(message.GetBytes());
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        public override Task<Response<Message>> SendRequest(Message message)
        {
            StringBuilder builder = new StringBuilder();
            this.Logger.AddDebugMessage("Sending request " + message.GetBytes().ToHex());
            this.Port.Send(message.GetBytes());

            byte[] response = new byte[100];
            this.Port.Receive(response, 0, 100);

            return Task.FromResult(Response.Create(ResponseStatus.Success, new Message(response)));
        }
    }
}
