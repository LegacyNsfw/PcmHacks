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
    /// Describes the flash memory configuration in the PCM.
    /// </summary>
    public enum FlashMemoryType
    {
        Unknown = 0,
        Intel512,
        Amd512,
        Intel1024,
        Amd1024,
    };

    public class MemoryRange
    {
        public UInt32 Address { get; private set; }
        public UInt32 Size { get; private set; }
        public UInt32 DesiredCrc { get; set; }
        public UInt32 ActualCrc { get; set; }

        public MemoryRange(UInt32 address, UInt32 size)
        {
            this.Address = address;
            this.Size = size;
        }
    }

    public partial class Vehicle
    {        
        /// <summary>
        /// For testing prototype kernels. 
        /// </summary>
        public async Task<bool> TestKernel(bool kernelRunning, bool recoveryMode, CancellationToken cancellationToken, Stream stream)
        {
            try
            {
                this.device.ClearMessageQueue();
                Response<byte[]> response;

                if (!kernelRunning)
                {        
                    response = await LoadKernelFromFile("read-kernel.bin");
                    if (response.Status != ResponseStatus.Success)
                    {
                        logger.AddUserMessage("Failed to load kernel from file.");
                        return false;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    // TODO: instead of this hard-coded address, get the base address from the PcmInfo object.
                    if (!await PCMExecute(response.Value, 0xFF8000, cancellationToken))
                    {
                        logger.AddUserMessage("Failed to upload kernel to PCM");

                        return false;
                    }

                    logger.AddUserMessage("Kernel uploaded to PCM succesfully.");

                    IList<MemoryRange> ranges = this.GetMemoryRanges(FlashMemoryType.Intel512);

                    // ?
                    await Task.Delay(250);

                    logger.AddUserMessage("Requesting CRCs from PCM...");
                    foreach (MemoryRange range in ranges)
                    {
                        Query<UInt32> crcQuery = new Query<uint>(
                            this.device,
                            () => this.messageFactory.CreateCrcQuery(range.Address, range.Size),
                            this.messageParser.ParseCrc,
                            this.logger,
                            cancellationToken);
                        Response<UInt32> crcResponse = await crcQuery.Execute();

                        if (crcResponse.Status != ResponseStatus.Success)
                        {
                            this.logger.AddUserMessage("Unable to get CRC for memory range " + range.Address.ToString("X8") + " / " + range.Size.ToString("X8"));
                            return false;
                        }

                        range.ActualCrc = crcResponse.Value;

                        this.logger.AddUserMessage(
                            string.Format(
                                "Range {0:X6}-{1:X6} - Local: {2:X8} - PCM: {3:X8} - {4}",
                                range.Address,
                                range.Address + (range.Size - 1),
                                range.DesiredCrc,
                                range.ActualCrc,
                                range.DesiredCrc == range.ActualCrc ? "Same" : "Different"));
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                this.logger.AddUserMessage("Something went wrong. " + exception.Message);
                this.logger.AddDebugMessage(exception.ToString());
                return false;
            }
            finally
            {
                await this.Cleanup();
            }
        }
    }
}
