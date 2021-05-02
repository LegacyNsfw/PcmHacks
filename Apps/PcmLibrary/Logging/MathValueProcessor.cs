using DynamicExpresso;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PcmHacking
{
    /// <summary>
    /// Combines a math column with the columns that it depends on.
    /// </summary>
    public class MathColumnAndDependencies
    {
        public LogColumn MathColumn { get; private set; }
        public LogColumn XColumn { get; private set; }
        public LogColumn YColumn { get; private set; }
        
        public MathColumnAndDependencies(
            LogColumn mathColumn,
            LogColumn xColumn,
            LogColumn yColumn)
        {
            this.MathColumn = mathColumn;
            this.XColumn = xColumn;
            this.YColumn = yColumn;
        }
    }

    /// <summary>
    /// Computes the values for math columns, based on data read from the PCM.
    /// </summary>
    public class MathValueProcessor
    {
        private readonly DpidConfiguration dpidConfiguration;
        private IEnumerable<MathColumnAndDependencies> mathColumns;

        /// <summary>
        /// Constructor
        /// </summary>
        public MathValueProcessor(DpidConfiguration dpidConfiguration, IEnumerable<MathColumnAndDependencies> mathColumns)
        {
            this.dpidConfiguration = dpidConfiguration;
            this.mathColumns = mathColumns;
        }

        /// <summary>
        /// Returns the names of the math columns.
        /// </summary>
        public IEnumerable<string> GetHeaderNames()
        {
            return this.mathColumns.Select(x => x.MathColumn.Parameter.Name);
        }

        /// <summary>
        /// Gets the math columns - the logger will concatenate these with the PCM columns.
        /// </summary>
        public IEnumerable<LogColumn> GetMathColumns()
        {
            return this.mathColumns.Select(x => x.MathColumn);
        }

        /// <summary>
        /// Get the values of the math columns as strings, suitable for display or writing to a log file.
        /// </summary>
        public IEnumerable<string> GetMathValues(PcmParameterValues dpidValues)
        {
            List<string> result = new List<string>();
            foreach(MathColumnAndDependencies value in this.mathColumns)
            {
                double xConverted = 0;
                double yConverted = 0;
                string error = null;

                try
                {
                    double xParameterValue = dpidValues[value.XColumn].ValueAsDouble;
                    Interpreter xConverter = new Interpreter();
                    xConverter.SetVariable("x", xParameterValue);
                    xConverted = xConverter.Eval<double>(value.XColumn.Conversion.Expression);
                }
                catch (Exception exception)
                {
                    error = "X: " + exception.Message;
                    xConverted = 0;
                }

                try
                {
                    double yParameterValue = dpidValues[value.YColumn].ValueAsDouble;
                    Interpreter yConverter = new Interpreter();
                    yConverter.SetVariable("x", yParameterValue);
                    yConverted = yConverter.Eval<double>(value.YColumn.Conversion.Expression);
                }
                catch (Exception exception)
                {
                    error = "Y: " + exception.Message;
                    yConverted = 0;
                }

                if (error != null)
                {
                    result.Add(error);
                }
                else
                {
                    try
                    {
                        Interpreter finalConverter = new Interpreter();
                        finalConverter.SetVariable("x", xConverted);
                        finalConverter.SetVariable("y", yConverted);
                        double converted = finalConverter.Eval<double>(value.MathColumn.Conversion.Expression);
                        result.Add(converted.ToString(value.MathColumn.Conversion.Format));
                    }
                    catch (Exception exception)
                    {
                        result.Add("Error: " + exception.Message);
                    }
                }
            }

            return result;
        }
    }
}

