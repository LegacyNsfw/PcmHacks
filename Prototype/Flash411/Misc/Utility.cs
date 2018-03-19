﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flash411
{
    public static class Utility
    {
        private static readonly char[] space = new char[] { ' ' };

        /// <summary>
        /// Convert an array of bytes to a hex string.
        /// </summary>
        public static string ToHex(this byte[] bytes)
        {
            return string.Join(" ", bytes.Select(x => x.ToString("X2")));
        }

        /// <summary>
        /// Convert only part of an array of bytes to a hex string.
        /// </summary>
        public static string ToHex(this byte[] bytes, int count)
        {
            return string.Join(" ", bytes.Take(count).Select(x => x.ToString("X2")));
        }

        /// <summary>
        /// Indicates whether the given string contains just hex characters 
        /// and whitespace, or garbage.
        /// </summary>
        public static bool IsHex(this string value)
        {
            for (int index = 0; index < value.Length; index++)
            {
                if (char.IsWhiteSpace(value[index]))
                {
                    continue;
                }

                if (Utility.GetHex(value[index]) == -1)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Convert the given string (which should be in hex format) to a byte array.
        /// </summary>
        public static byte[] ToBytes(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                                .Where(x => x % 2 == 0)
                                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                .ToArray();
        }

        /// <summary>
        /// Indicate whether the two arrays are identical or not.
        /// </summary>
        public static bool CompareArrays(byte[] actual, byte[] expected)
        {
            if (actual.Length != expected.Length)
            {
                return false;
            }

            for (int index = 0; index < expected.Length; index++)
            {
                if (actual[index] != expected[index])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Indicate whether the overlap between two arrays are identical or not.
        /// </summary>
        public static bool CompareArraysPart(byte[] actual, byte[] expected)
        {
            if (actual == null || expected == null) return false;

            for (int index = 0; index < expected.Length && index < actual.Length; index++)
            {
                if (actual[index] != expected[index])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This removes non-ascii from a byte array
        /// </summary>
        public static byte[] GetPrintable(byte[] input)
        {
            var cleanBuffer = input.Where((b) => (b <= 0x7E) & (b >= 0x30)).ToArray();
            return cleanBuffer;
        }

        // This method and its comments were copied from https://github.com/dotnet/wcf/blob/master/src/System.Private.ServiceModel/src/Internals/System/Runtime/TaskHelpers.cs
        //
        // Awaitable helper to await a maximum amount of time for a task to complete. If the task doesn't
        // complete in the specified amount of time, returns false. This does not modify the state of the
        // passed in class, but instead is a mechanism to allow interrupting awaiting a task if a timeout
        // period passes.
        public static async Task<bool> AwaitWithTimeout(this Task task, TimeSpan timeout)
        {
            if (task.IsCompleted)
            {
                return true;
            }

            if (timeout == TimeSpan.MaxValue || timeout == Timeout.InfiniteTimeSpan)
            {
                await task;
                return true;
            }

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
                if (completedTask == task)
                {
                    cts.Cancel();
                    return true;
                }
                else
                {
                    return (task.IsCompleted);
                }
            }
        }

        /// <summary>
        /// There's a bug here. I haven't fixed it because after writing this 
        /// I learned that I don't actually need it.
        /// </summary>
        internal static byte ComputeCrc(byte[] bytes)
        {
            int crc = 0xFF;
            for (int index = 0; index < bytes.Length; index++)
            {
                crc ^= bytes[index];
                if ((crc & 0x80) != 0)
                {
                    crc <<= 1;
                    crc ^= 0x11D;
                }
                else
                {
                    crc <<= 1;
                }
            }

            return (byte) crc;
        }

        /// <summary>
        /// Gets the 0-15 value of a hexadecimal numeral.
        /// </summary>
        private static int GetHex(char newCharacter)
        {
            if (newCharacter >= '0' && (newCharacter <= '9'))
            {
                return newCharacter - '0';
            }

            if (newCharacter >= 'a' && (newCharacter <= 'f'))
            {
                return 10 + (newCharacter - 'a');
            }

            if (newCharacter >= 'A' && (newCharacter <= 'F'))
            {
                return 10 + (newCharacter - 'A');
            }

            return -1;
        }
    }
}
