namespace TitanReach_Server.Network
{
    using System;
    using System.IO;
    using System.Text;

    namespace Assets.Core.Network
    {

        /// <summary>
        /// This class handles byte buffer arrays.
        /// </summary>
        public class MessageBuffer
        {
            /// <summary>
            /// The byte buffer.
            /// </summary>
         //   public byte[] Buffer { get; private set; }
            /// <summary>
            /// Returns the buffer size in bytes.
            /// </summary>
            public long Size => Stream.Length;
            /// <summary>
            /// The reading and writing position.
            /// </summary>
            public long Position { 
                get => Stream.Position;
                set => Stream.Position = value;
            }

            /// <summary>
            /// Create a MessageBuffer with an existing byte buffer.
            /// </summary>
            /// <param name="buffer">The byte buffer to use.</param>
            public MessageBuffer(byte[] buffer, bool _write)
            {
                Stream = new MemoryStream(buffer);
                if (_write)
                    Writer = new BinaryWriter(Stream);
                else
                    Reader = new BinaryReader(Stream);
            }

            public void Dispose()
            {
                Writer?.Dispose();
                Reader?.Dispose();
                Stream?.Dispose();
               
            }

            public MemoryStream Stream;
            public BinaryWriter Writer;
            public BinaryReader Reader;

            /// <summary>
            /// Reads a byte from the current buffer position.
            /// </summary>
            /// <returns>Returns the value.</returns>
            public byte ReadByte()
            {
                return Reader.ReadByte();
            }


            /// <summary>
            /// Reads an unsigned short from the current buffer position.
            /// </summary>
            /// <returns>Returns the value.</returns>
            public ushort ReadUInt16()
            {
                return Reader.ReadUInt16();
            }



            /// <summary>
            /// Reads a signed short from the current buffer position.
            /// </summary>
            /// <returns>Returns the value.</returns>
            public short ReadInt16()
            {
                return Reader.ReadInt16();
            }

            /// <summary>
            /// Reads an unsigned int from the current buffer position.
            /// </summary>
            /// <returns>Returns the value.</returns>
            public uint ReadUInt32()
            {
                return Reader.ReadUInt32();
            }

            /// <summary>
            /// Reads a signed int from the current buffer position.
            /// </summary>
            /// <returns>Returns the value.</returns>
            public int ReadInt32()
            {
                return Reader.ReadInt32();
            }

            /// <summary>
            /// Reads a float from the current buffer position.
            /// </summary>
            /// <returns>Returns the value.</returns>
            public float ReadFloat()
            {
                return Reader.ReadSingle();
            }

            /// <summary>
            /// Reads a double from the current buffer position.
            /// </summary>
            /// <returns>Returns the value.</returns>
            public double ReadDouble()
            {
                return Reader.ReadDouble();
            }

            /// <summary>
            /// Reads a bool from the current buffer position.
            /// </summary>
            /// <returns>Returns the value.</returns>
            public bool ReadBoolean()
            {
                return Reader.ReadBoolean();
            }

            /// <summary>
            /// Reads a string from the current buffer position.
            /// </summary>
            /// <returns>Returns the value.</returns>
            public string ReadString(int amount)
            {
                var value = Encoding.UTF8.GetString(Reader.ReadBytes(amount));
                //Position += value.Length;
                return value;
            }

            /// <summary>
            /// Writes a byte to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteByte(byte value)
            {
                Writer.Write(value);
            }

            /// <summary>
            /// Writes a signed byte to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteSByte(sbyte value)
            {
                Writer.Write(value);
            }

            /// <summary>
            /// Writes an unsigned short to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteUInt16(ushort value)
            {
                Writer.Write(value);
            }

            /// <summary>
            /// Writes a signed short to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteInt16(short value)
            {
                Writer.Write(value);
            }

            /// <summary>
            /// Writes an unsigned int to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteUInt32(uint value)
            {
                Writer.Write(value);
            }

            /// <summary>
            /// Writes a signed int to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteInt32(int value)
            {
                Writer.Write(value);
            }

            /// <summary>
            /// Writes a float to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteFloat(float value)
            {
                Writer.Write(value);
            }

            /// <summary>
            /// Writes a double to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteDouble(double value)
            {
                Writer.Write(value);
            }

            /// <summary>
            /// Writes a boolean to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteBoolean(bool value)
            {
                Writer.Write(value);
            }

            /// <summary>
            /// Writes a null terminated string to the current buffer position.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteString(string value)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                //Array.Copy(bytes, 0, Buffer, Position, bytes.Length);
                Writer.Write(bytes);
              //  Position += bytes.Length;
                
            }
        }

    }

}
