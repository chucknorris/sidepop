using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sidepop.Mail
{
    /// <summary>
    /// Keeps the last X bytes in mind during a buffer accumulation
    /// </summary>
    public class LastBytesTracker
    {
        /// <summary>
        /// Number of bytes to keep track
        /// </summary>
        private int _numberOfBytesToTrack;

        /// <summary>
        /// Current last bytes value
        /// </summary>
        private byte[] _lastBytes;

        /// <summary>
        /// Ctor
        /// </summary>
        public LastBytesTracker(int numberOfBytesToTrack)
        {
            _numberOfBytesToTrack = numberOfBytesToTrack;
            _lastBytes = new byte[0];
        }

        /// <summary>
        /// Keeps track of the specified buffer
        /// </summary>
        public void AddBytes(byte[] bytes, int length)
        {
            if (length >= _numberOfBytesToTrack)
            {
                _lastBytes = new byte[_numberOfBytesToTrack];
                Array.Copy(bytes, length - _numberOfBytesToTrack, _lastBytes, 0, _numberOfBytesToTrack);
            }
            else
            {
                var mergedArrays = _lastBytes.Concat(bytes.Take(length));

                int currentByteCount = _lastBytes.Length;

                int totalBytes = currentByteCount + length;

                if (totalBytes > _numberOfBytesToTrack)
                {
                    int bytesToSkip = totalBytes - _numberOfBytesToTrack;

                    _lastBytes = mergedArrays.Skip(bytesToSkip).Take(_numberOfBytesToTrack).ToArray();
                }
                else
                {
                    _lastBytes = mergedArrays.Take(totalBytes).ToArray();
                }
            }
        }

        /// <summary>
        /// Returns the last bytes received
        /// </summary>
        public byte[] LastBytes
        {
            get
            {
                return _lastBytes;
            }
        }
    }
}
