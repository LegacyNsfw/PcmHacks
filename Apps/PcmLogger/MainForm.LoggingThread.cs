using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    partial class MainForm
    {
        private ConcurrentQueue<Tuple<Logger, LogFileWriter, IEnumerable<string>>> logRowQueue = new ConcurrentQueue<Tuple<Logger, LogFileWriter, IEnumerable<string>>>();

        private AutoResetEvent endWriterThread = new AutoResetEvent(false);
        private AutoResetEvent rowAvailable = new AutoResetEvent(false);

        private static DateTime lastLogTime;

        private ManualResetEvent loggerThreadEnded = new ManualResetEvent(false);
        private ManualResetEvent writerThreadEnded = new ManualResetEvent(false);
        
        enum LogState
        {
            Nothing,
            DisplayOnly,
            StartSaving,
            Saving,
            StopSaving
        }

        private LogState logState = LogState.Nothing;

        /// <summary>
        /// Create a string that will look reasonable in the UI's main text box.
        /// TODO: Use a grid instead.
        /// </summary>
        private string FormatValuesForTextBox(DpidsAndMath dpidsAndMath, IEnumerable<string> rowValues)
        {
            StringBuilder builder = new StringBuilder();
            IEnumerator<string> rowValueEnumerator = rowValues.GetEnumerator();
            foreach (ParameterGroup group in dpidsAndMath.Profile.ParameterGroups)
            {
                foreach (ProfileParameter parameter in group.Parameters)
                {
                    rowValueEnumerator.MoveNext();
                    builder.Append(rowValueEnumerator.Current);
                    builder.Append('\t');
                    builder.Append(parameter.Conversion.Units);
                    builder.Append('\t');
                    builder.AppendLine(parameter.Parameter.Name);
                }
            }

            foreach (MathValue mathValue in dpidsAndMath.MathValueProcessor.GetMathValues())
            {
                rowValueEnumerator.MoveNext();
                builder.Append(rowValueEnumerator.Current);
                builder.Append('\t');
                builder.Append(mathValue.Units);
                builder.Append('\t');
                builder.AppendLine(mathValue.Name);
            }

            DateTime now = DateTime.Now;
            builder.AppendLine((now - lastLogTime).TotalMilliseconds.ToString("0.00") + "\tms\tQuery time");
            lastLogTime = now;

            return builder.ToString();
        }

        private async Task<Logger> RecreateLogger()
        {
            DpidsAndMath dpidsAndMath = null;

            this.Invoke(
                (MethodInvoker)
                delegate ()
                {
                    // Read parmeters from the UI
                    dpidsAndMath = this.CreateDpidConfiguration();
                });

            if (dpidsAndMath == null)
            {
                this.loggerProgress.Invoke(
                    (MethodInvoker)
                    delegate ()
                    {
                        this.logValues.Text = "Unable to create DPID configuration.";
                    });

                Thread.Sleep(200);
                return null;
            }

            Logger logger = new Logger(this.Vehicle, dpidsAndMath, this.loader.Configuration);
            if (!await logger.StartLogging())
            {
                this.loggerProgress.Invoke(
                    (MethodInvoker)
                    delegate ()
                    {
                        this.logValues.Text = "Unable to start logging.";
                    });

                Thread.Sleep(200);

                // Force a retry.
                return null;
            }

            return logger;
        }

        private async Task<Tuple<LogFileWriter,StreamWriter>> StartSaving(Logger logger)
        {
            string logFilePath = GenerateLogFilePath();
            StreamWriter streamWriter = new StreamWriter(logFilePath);
            LogFileWriter logFileWriter = new LogFileWriter(streamWriter);

            IEnumerable<string> columnNames = logger.DpidsAndMath.GetColumnNames();
            await logFileWriter.WriteHeader(columnNames);

            return new Tuple<LogFileWriter, StreamWriter>(logFileWriter, streamWriter);
        }

        private void StopSaving(ref StreamWriter streamWriter)
        {
            if (streamWriter != null)
            {
                streamWriter.Dispose();
                streamWriter = null;
            }
        }

        private async Task ProcessRow(Logger logger, LogFileWriter logFileWriter)
        {
            IEnumerable<string> rowValues = await logger.GetNextRow();
            if (rowValues != null)
            {
                // Hand this data off to be written to disk and displayed in the UI.
                this.logRowQueue.Enqueue(
                    new Tuple<Logger, LogFileWriter, IEnumerable<string>>(
                        logger,
                        logFileWriter,
                        rowValues));

                this.rowAvailable.Set();
            }
        }

        /// <summary>
        /// The loop that reads data from the PCM.
        /// </summary>
        private async void LoggingThread(object threadContext)
        {
            using (AwayMode lockScreenSuppressor = new AwayMode())
            {
                try
                {
                    // Start the write/display thread.
                    ThreadPool.QueueUserWorkItem(LogFileWriterThread, null);

#if Vpw4x
                    if (!await this.Vehicle.VehicleSetVPW4x(VpwSpeed.FourX))
                    {
                        this.AddUserMessage("Unable to switch to 4x.");
                        return;
                    }
#endif                    

                    StreamWriter streamWriter = null;
                    try
                    {
                        LogProfile lastProfile = null;
                        Logger logger = null;
                        LogFileWriter logFileWriter = null;

                        while (!this.logStopRequested)
                        {
                            // Re-create the logger with an updated profile if necessary.
                            if (this.currentProfile != lastProfile)
                            {
                                this.StopSaving(ref streamWriter);

                                if ((this.currentProfile == null) || this.currentProfile.IsEmpty)
                                {
                                    this.logState = LogState.Nothing;
                                    lastProfile = this.currentProfile;
                                    logger = null;
                                }
                                else
                                {
                                    logger = await this.RecreateLogger();
                                    if (logger != null)
                                    {
                                        lastProfile = this.currentProfile;

                                        // If this was the first profile to load...
                                        if (this.logState == LogState.Nothing)
                                        {
                                            this.logState = LogState.DisplayOnly;
                                        }
                                    }

                                    switch (logState)
                                    {
                                        case LogState.Nothing:
                                        case LogState.DisplayOnly:
                                        case LogState.StopSaving:
                                            break;

                                        default:
                                            var tuple = await this.StartSaving(logger);
                                            logFileWriter = tuple.Item1;
                                            streamWriter = tuple.Item2;
                                            logState = LogState.Saving;
                                            break;
                                    }
                                }

                                this.Invoke(
                                    (MethodInvoker)
                                    delegate ()
                                    {
                                        this.startStopSaving.Enabled = logger != null;
                                    });

                            }

                            switch (logState)
                            {
                                case LogState.Nothing:
                                    this.loggerProgress.Invoke(
                                        (MethodInvoker)
                                        delegate ()
                                        {
                                            this.logValues.Text = "Please select some parameters, or open a log profile.";
                                        });

                                    Thread.Sleep(200);
                                    break;

                                case LogState.DisplayOnly:
                                    await this.ProcessRow(logger, null);
                                    break;

                                case LogState.StartSaving:
                                    var tuple = await this.StartSaving(logger);
                                    logFileWriter = tuple.Item1;
                                    streamWriter = tuple.Item2;
                                    logState = LogState.Saving;
                                    break;

                                case LogState.Saving:
                                    await this.ProcessRow(logger, logFileWriter);
                                    break;

                                case LogState.StopSaving:
                                    this.StopSaving(ref streamWriter);
                                    this.logState = LogState.DisplayOnly;
                                    break;
                            }
                        }        
                    }
                    finally
                    {
                        if (streamWriter != null)
                        {
                            streamWriter.Dispose();
                            streamWriter = null;
                        }

                        endWriterThread.Set();
                    }
                }
                catch (Exception exception)
                {
                    this.AddUserMessage("Logging halted. " + exception.Message);
                    this.AddDebugMessage(exception.ToString());
                    this.logValues.Invoke(
                        (MethodInvoker)
                        delegate ()
                        {
                            this.logValues.Text = "Logging halted. " + exception.Message;
                            this.startStopSaving.Focus();
                        });
                }
                finally
                {
                    this.loggerThreadEnded.Set();
#if Vpw4x
                    if (!await this.Vehicle.VehicleSetVPW4x(VpwSpeed.Standard))
                    {
                        // Try twice...
                        await this.Vehicle.VehicleSetVPW4x(VpwSpeed.Standard);
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Background thread to write to disk and send updates to the UI.
        /// This minimizes the amount code that executes between requests for new rows of log data.
        /// </summary>
        private void LogFileWriterThread(object threadContext)
        {
            WaitHandle[] writerHandles = new WaitHandle[] { endWriterThread, rowAvailable };

            try
            {
                while (!logStopRequested)
                {
                    int index = WaitHandle.WaitAny(writerHandles);
                    if (index == 0)
                    {
                        this.BeginInvoke((MethodInvoker)
                        delegate ()
                        {
                            this.logValues.Text = "Logging halted.";
                        });

                        return;
                    }

                    Tuple<Logger, LogFileWriter, IEnumerable<string>> row;
                    if (logRowQueue.TryDequeue(out row))
                    {
                        if (row.Item2 != null)
                        {
                            row.Item2.WriteLine(row.Item3);
                        }

                        string formattedValues = FormatValuesForTextBox(row.Item1.DpidsAndMath, row.Item3);

                        this.BeginInvoke((MethodInvoker)
                        delegate ()
                        {
                            this.logValues.Text = formattedValues;
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                this.AddUserMessage("Log writing halted. " + exception.Message);
                this.AddDebugMessage(exception.ToString());
                this.logValues.Invoke(
                    (MethodInvoker)
                    delegate ()
                    {
                        this.logValues.Text = "Log writing halted. " + exception.Message;
                        this.startStopSaving.Focus();
                    });
            }
            finally
            {
                this.writerThreadEnded.Set();
            }
        }
    }
}
