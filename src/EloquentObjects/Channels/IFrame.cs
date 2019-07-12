using System;

namespace EloquentObjects.Channels
{
    /// <summary>
    /// Represents an input frame from which simple values can be taken one by one.
    /// </summary>
    internal interface IFrame
    {
        /// <summary>
        /// Extracts the byte value from the frame.
        /// </summary>
        /// <returns>The byte value.</returns>
        /// <exception cref="InvalidOperationException">End of frame reached</exception>
        byte TakeByte();

        /// <summary>
        /// Extracts a byte value from the frame.
        /// </summary>
        /// <returns>A byte value.</returns>
        /// <exception cref="InvalidOperationException">End of frame reached</exception>
        bool TakeBool();

        /// <summary>
        /// Extracts an integer value from the frame.
        /// </summary>
        /// <returns>An integer value.</returns>
        /// <exception cref="InvalidOperationException">End of frame reached</exception>
        int TakeInt();

        /// <summary>
        /// Extracts a string value from the frame.
        /// </summary>
        /// <returns>A byte value.</returns>
        /// <exception cref="InvalidOperationException">End of frame reached</exception>
        string TakeString();

        /// <summary>
        /// Extracts an array of bytes from the frame.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        /// <exception cref="InvalidOperationException">End of frame reached</exception>
        byte[] TakeBuffer();
        
        /// <summary>
        /// Converts the whole frame to bytes array.
        /// </summary>
        /// <returns>An array of bytes that contains the whole frame.</returns>
        /// <exception cref="InvalidOperationException">End of frame reached</exception>
        byte[] ToArray();
    }
}