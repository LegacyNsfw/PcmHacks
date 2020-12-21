using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    partial class MainForm
    {
        /// <summary>
        /// The loop that reads data from the PCM.
        /// </summary>
        private async void LoggingThread(object threadContext)
        {
            using (AwayMode lockScreenSuppressor = new AwayMode())
            {
                try
                {
                    string logFilePath = GenerateLogFilePath();

                    this.loggerProgress.Invoke(
                    (MethodInvoker)
                    delegate ()
                    {
                        this.loggerProgress.Value = 0;
                        this.loggerProgress.Visible = true;
                        this.logFilePath.Text = logFilePath;
                        this.setDirectory.Enabled = false;
                        this.startStopLogging.Focus();
                    });

                    Logger logger = new Logger(this.Vehicle, this.dpidsAndMath, this.loader.Configuration);
                    if (!await logger.StartLogging())
                    {
                        this.AddUserMessage("Unable to start logging.");
                        return;
                    }

#if Vpw4x
                    if (!await this.Vehicle.VehicleSetVPW4x(VpwSpeed.FourX))
                    {
                        this.AddUserMessage("Unable to switch to 4x.");
                        return;
                    }
#endif
                    using (StreamWriter streamWriter = new StreamWriter(logFilePath))
                    {
                        LogFileWriter writer = new LogFileWriter(streamWriter);
                        IEnumerable<string> columnNames = this.dpidsAndMath.GetColumnNames();
                        await writer.WriteHeader(columnNames);

                        lastLogTime = DateTime.Now;

                        this.loggerProgress.Invoke(
                            (MethodInvoker)
                            delegate ()
                            {
                                this.loggerProgress.MarqueeAnimationSpeed = 150;
                                this.selectButton.Enabled = false;
                            });

                        while (!this.logStopRequested)
                        {
                            this.AddDebugMessage("Requesting row...");
                            IEnumerable<string> rowValues = await logger.GetNextRow();
                            if (rowValues == null)
                            {
                                continue;
                            }
                            
                            // Write the data to disk on a background thread.
                            Task background = Task.Factory.StartNew(
                                delegate ()
                                {
                                    writer.WriteLine(rowValues);
                                });

                            // This is slowing down the logging rate significantly.
                            // Even if it is only invoked 1:5 or 1:10.
                            //
                            // Display the data using a foreground thread.
                            Task foreground = Task.Factory.StartNew(
                                delegate ()
                                {
                                    string formattedValues = FormatValuesForTextBox(rowValues);
                                    this.logValues.Text = string.Join(Environment.NewLine, formattedValues);
                                },
                                CancellationToken.None,
                                TaskCreationOptions.None,
                                uiThreadScheduler);
                        }
                    }
                }
                catch (Exception exception)
                {
                    this.AddDebugMessage(exception.ToString());
                    this.AddUserMessage("Logging interrupted. " + exception.Message);
                    this.logValues.Invoke(
                        (MethodInvoker)
                        delegate ()
                        {
                            this.logValues.Text = "Logging interrupted. " + exception.Message;
                            this.startStopLogging.Focus();
                        });
                }
                finally
                {
#if Vpw4x
                    if (!await this.Vehicle.VehicleSetVPW4x(VpwSpeed.Standard))
                    {
                        // Try twice...
                        await this.Vehicle.VehicleSetVPW4x(VpwSpeed.Standard);
                    }
#endif
                    this.logStopRequested = false;
                    this.logging = false;
                    this.startStopLogging.Invoke(
                        (MethodInvoker)
                        delegate ()
                        {
                            this.loggerProgress.MarqueeAnimationSpeed = 0;
                            this.loggerProgress.Visible = false;
                            this.startStopLogging.Enabled = true;
                            this.startStopLogging.Text = "Start &Logging";
                            this.logFilePath.Text = Configuration.Settings.LogDirectory;
                            this.setDirectory.Enabled = true;

                            this.selectButton.Enabled = true;
                            //                            this.selectProfileButton.Enabled = true;
                            this.startStopLogging.Focus();
                        });
                }
            }
        }
    }
}
