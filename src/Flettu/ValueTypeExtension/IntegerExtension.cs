using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flettu.ValueTypeExtension
{
    /// <summary>
    /// Extension methods for integers
    /// </summary>
    public static class IntegerExtension
    {
        /// <summary>
        /// Copies value to buffer from specified offset
        /// </summary>
        /// <param name="value">value to copy in buffer</param>
        /// <param name="buffer">Bytes buffer to copy value in</param>
        /// <param name="index">Index to copy from</param>
        public static void CopyToBuffer(this short value, byte[] buffer, int index)
        {
            if ((index + 2) > buffer.Length)
                throw new ArgumentException(string.Format("Not enough space in buffer to set value from index: {0}", index));

            buffer[index] = (byte)value;
            buffer[index + 1] = (byte)(value >> 8);
        }

        /// <summary>
        /// Copies value to buffer from specified offset
        /// </summary>
        /// <param name="value">value to copy in buffer</param>
        /// <param name="buffer">Bytes buffer to copy value in</param>
        /// <param name="index">Index to copy from</param>
        public static void CopyToBuffer(this int value, byte[] buffer, int index)
        {
            if ((index + 4) > buffer.Length)
                throw new ArgumentException(string.Format("Not enough space in buffer to set value from index: {0}", index));

            buffer[index] = (byte)value;
            buffer[index + 1] = (byte)(value >> 8);
            buffer[index + 2] = (byte)(value >> 16);
            buffer[index + 3] = (byte)(value >> 24);
        }

        /// <summary>
        /// Copies value to buffer from specified offset
        /// </summary>
        /// <param name="value">value to copy in buffer</param>
        /// <param name="buffer">Bytes buffer to copy value in</param>
        /// <param name="index">Index to copy from</param>
        public static void CopyToBuffer(this long value, byte[] buffer, int index)
        {
            if ((index + 8) > buffer.Length)
                throw new ArgumentException(string.Format("Not enough space in buffer to set value from index: {0}", index));

            buffer[index] = (byte)value;
            buffer[index + 1] = (byte)(value >> 8);
            buffer[index + 2] = (byte)(value >> 16);
            buffer[index + 3] = (byte)(value >> 24);
            buffer[index + 4] = (byte)(value >> 32);
            buffer[index + 5] = (byte)(value >> 40);
            buffer[index + 6] = (byte)(value >> 48);
            buffer[index + 7] = (byte)(value >> 56);
        }
    }
}
