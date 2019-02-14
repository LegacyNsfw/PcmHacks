using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// From the application's perspective, this class is the API to the vehicle.
    /// </summary>
    /// <remarks>
    /// Methods in this class are high-level operations like "get the VIN," or "read the contents of the EEPROM."
    /// </remarks>
    public partial class Vehicle : IDisposable
    {
        /// <summary>
        /// Query the PCM's VIN.
        /// </summary>
        public async Task<Response<string>> QueryVin()
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            this.device.ClearMessageQueue();

            if (!await this.device.SendMessage(this.protocol.CreateVinRequest1()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 1 failed.");
            }

            Message response1 = await this.device.ReceiveMessage();
            if (response1 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 1.");
            }

            if (!await this.device.SendMessage(this.protocol.CreateVinRequest2()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 2 failed.");
            }

            Message response2 = await this.device.ReceiveMessage();
            if (response2 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 2.");
            }

            if (!await this.device.SendMessage(this.protocol.CreateVinRequest3()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 3 failed.");
            }

            Message response3 = await this.device.ReceiveMessage();
            if (response3 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 3.");
            }

            return this.protocol.ParseVinResponses(response1.GetBytes(), response2.GetBytes(), response3.GetBytes());
        }

        /// <summary>
        /// Query the PCM's Serial Number.
        /// </summary>
        public async Task<Response<string>> QuerySerial()
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            this.device.ClearMessageQueue();

            if (!await this.device.SendMessage(this.protocol.CreateSerialRequest1()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 1 failed.");
            }

            Message response1 = await this.device.ReceiveMessage();
            if (response1 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 1.");
            }

            if (!await this.device.SendMessage(this.protocol.CreateSerialRequest2()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 2 failed.");
            }

            Message response2 = await this.device.ReceiveMessage();
            if (response2 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 2.");
            }

            if (!await this.device.SendMessage(this.protocol.CreateSerialRequest3()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 3 failed.");
            }

            Message response3 = await this.device.ReceiveMessage();
            if (response3 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 3.");
            }

            return this.protocol.ParseSerialResponses(response1, response2, response3);
        }

        /// <summary>
        /// Query the PCM's Broad Cast Code.
        /// </summary>
        public async Task<Response<string>> QueryBCC()
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            var query = this.CreateQuery(
                this.protocol.CreateBCCRequest,
                this.protocol.ParseBCCresponse, 
                CancellationToken.None);

            return await query.Execute();
        }

        /// <summary>
        /// Query the PCM's Manufacturer Enable Counter (MEC)
        /// </summary>
        public async Task<Response<string>> QueryMEC()
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            var query = this.CreateQuery(
                this.protocol.CreateMECRequest,
                this.protocol.ParseMECresponse,
                CancellationToken.None);

            return await query.Execute();
        }

        /// <summary>
        /// Update the PCM's VIN
        /// </summary>
        /// <remarks>
        /// Requires that the PCM is already unlocked
        /// </remarks>
        public async Task<Response<bool>> UpdateVin(string vin)
        {
            this.device.ClearMessageQueue();

            if (vin.Length != 17) // should never happen, but....
            {
                this.logger.AddUserMessage("VIN " + vin + " is not 17 characters long!");
                return Response.Create(ResponseStatus.Error, false);
            }

            this.logger.AddUserMessage("Changing VIN to " + vin);

            byte[] bvin = Encoding.ASCII.GetBytes(vin);
            byte[] vin1 = new byte[6] { 0x00, bvin[0], bvin[1], bvin[2], bvin[3], bvin[4] };
            byte[] vin2 = new byte[6] { bvin[5], bvin[6], bvin[7], bvin[8], bvin[9], bvin[10] };
            byte[] vin3 = new byte[6] { bvin[11], bvin[12], bvin[13], bvin[14], bvin[15], bvin[16] };

            this.logger.AddUserMessage("Block 1");
            Response<bool> block1 = await WriteBlock(BlockId.Vin1, vin1);
            if (block1.Status != ResponseStatus.Success) return Response.Create(ResponseStatus.Error, false);
            this.logger.AddUserMessage("Block 2");
            Response<bool> block2 = await WriteBlock(BlockId.Vin2, vin2);
            if (block2.Status != ResponseStatus.Success) return Response.Create(ResponseStatus.Error, false);
            this.logger.AddUserMessage("Block 3");
            Response<bool> block3 = await WriteBlock(BlockId.Vin3, vin3);
            if (block3.Status != ResponseStatus.Success) return Response.Create(ResponseStatus.Error, false);

            return Response.Create(ResponseStatus.Success, true);
        }

        /// <summary>
        /// Query the PCM's operating system ID.
        /// </summary>
        /// <returns></returns>
        public async Task<Response<UInt32>> QueryOperatingSystemId(CancellationToken cancellationToken)
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);
            return await this.QueryUnsignedValue(this.protocol.CreateOperatingSystemIdReadRequest, cancellationToken);
        }

        /// <summary>
        /// Query the PCM's Hardware ID.
        /// </summary>
        /// <remarks>
        /// Note that this is a software variable and my not match the hardware at all of the software runs.
        /// </remarks>
        public async Task<Response<UInt32>> QueryHardwareId()
        {
            return await this.QueryUnsignedValue(this.protocol.CreateHardwareIdReadRequest, CancellationToken.None);
        }

        /// <summary>
        /// Query the PCM's calibration ID.
        /// </summary>
        public async Task<Response<UInt32>> QueryCalibrationId()
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            var query = this.CreateQuery(
                this.protocol.CreateCalibrationIdReadRequest,
                this.protocol.ParseUInt32FromBlockReadResponse,
                CancellationToken.None);
            return await query.Execute();
        }

        /// <summary>
        /// Helper function for queries that return unsigned 32-bit integers.
        /// </summary>
        private async Task<Response<UInt32>> QueryUnsignedValue(Func<Message> generator, CancellationToken cancellationToken)
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            var query = this.CreateQuery(generator, this.protocol.ParseUInt32FromBlockReadResponse, cancellationToken);
            return await query.Execute();
        }
    }
}
