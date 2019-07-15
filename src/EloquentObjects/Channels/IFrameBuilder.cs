using JetBrains.Annotations;

namespace EloquentObjects.Channels
{
    /// <summary>
    /// Represents an object that can build a <see cref="IFrame" /> from simple values.
    /// </summary>
    internal interface IFrameBuilder
    {
        /// <summary>
        /// Writes a byte value to the pending frame.
        /// </summary>
        /// <param name="value">A byte value that will be added to the pending frame</param>
        void WriteByte(byte value);

        /// <summary>
        /// Writes a bool value to the pending frame.
        /// </summary>
        /// <param name="value">A bool value that will be added to the pending frame</param>
        void WriteBool(bool value);
        
        /// <summary>
        /// Writes an integer value to the pending frame.
        /// </summary>
        /// <param name="value">An integer value that will be added to the pending frame</param>
        void WriteInt(int value);

        /// <summary>
        /// Writes a string value to the pending frame.
        /// </summary>
        /// <param name="value">A string value that will be added to the pending frame</param>
        void WriteString([CanBeNull] string value);

        /// <summary>
        /// Writes an array of bytes to the pending frame.
        /// </summary>
        /// <param name="value">An array of bytes that will be added to the pending frame</param>
        void WriteBuffer(byte[] value);

        /// <summary>
        /// Writes an array of boolean values to the pending frame.
        /// </summary>
        /// <param name="values">An array of boolean values that will be added to the pending frame</param>
        void WriteBoolArray(bool[] values);

        /// <summary>
        /// Writes an array of strings to the pending frame.
        /// </summary>
        /// <param name="values">An array of strings that will be added to the pending frame</param>
        void WriteStringArray(string[] values);
        
        /// <summary>
        /// Creates the frame based on added values to the pending frame.
        /// </summary>
        /// <returns>A created frame.</returns>
        IFrame ToFrame();
    }
}