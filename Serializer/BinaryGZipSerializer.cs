using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Serializer
{
    public class BinaryGZipSerializer
    {
        private const string AutoPropertyPattern = @"<(.*)>k__BackingField";

        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.NonPublic |
                                                  System.Reflection.BindingFlags.Instance;

        public static void Serialize(MemoryStream stream, Object obj)
        {
            using (var binaryWriterStream = new MemoryStream(1024))
            {
                using (var binaryWriter = new BinaryWriter(binaryWriterStream))
                {
                    Serialize(obj, binaryWriter);
                    binaryWriter.Flush();
                    binaryWriterStream.Seek(0, SeekOrigin.Begin);
                    using (var compressionStream = new GZipStream(stream, CompressionMode.Compress, true))
                    {
                        binaryWriterStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        private static void Serialize(Object obj, BinaryWriter binaryWriter)
        {
            var objType = obj.GetType();
            var fields = objType.GetFields(BindingFlags).
                Where(fi => !Regex.IsMatch(fi.Name, AutoPropertyPattern)); // Skip AutoProperty fields
            var classFields = new Dictionary<string, object>();
            foreach (var fieldInfo in fields)
            {
                var value = fieldInfo.GetValue(obj);
                if (value == null) continue;
                classFields.Add(fieldInfo.Name, value);
            }
            var properties = objType.GetProperties(BindingFlags);
            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(obj, null);
                if (value == null) continue;
                classFields.Add(propertyInfo.Name, value);
            }
            binaryWriter.Write(objType.FullName);
            binaryWriter.Write(classFields.Count);
            foreach (var classField in classFields)
            {
                binaryWriter.Write(classField.Key);
                WriteValueByType(classField.Value, binaryWriter);
            }
        }

        private static void WriteValueByType(object value, BinaryWriter binaryWriter)
        {
            var typeCode = value.GetType().GetTypeCode();
            binaryWriter.Write((Byte) typeCode);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    binaryWriter.Write((Boolean) value);
                    break;
                case TypeCode.Char:
                    binaryWriter.Write((Char) value);
                    break;
                case TypeCode.SByte:
                    binaryWriter.Write((SByte) value);
                    break;
                case TypeCode.Byte:
                    binaryWriter.Write((Byte) value);
                    break;
                case TypeCode.Int16:
                    binaryWriter.Write((Int16) value);
                    break;
                case TypeCode.UInt16:
                    binaryWriter.Write((UInt16) value);
                    break;
                case TypeCode.Int32:
                    binaryWriter.Write((Int32) value);
                    break;
                case TypeCode.UInt32:
                    binaryWriter.Write((UInt32) value);
                    break;
                case TypeCode.Int64:
                    binaryWriter.Write((Int64) value);
                    break;
                case TypeCode.UInt64:
                    binaryWriter.Write((UInt64) value);
                    break;
                case TypeCode.Single:
                    binaryWriter.Write((Single) value);
                    break;
                case TypeCode.Double:
                    binaryWriter.Write((Double) value);
                    break;
                case TypeCode.Decimal:
                    binaryWriter.Write((Decimal) value);
                    break;
                case TypeCode.DateTime:
                    binaryWriter.Write(((DateTime) (value)).ToFileTime());
                    break;
                case TypeCode.String:
                    binaryWriter.Write((String) value);
                    break;
                case TypeCode.Array:
                    WriteArrayValues(value, binaryWriter);
                    break;
                case TypeCode.Class:
                    Serialize(value, binaryWriter);
                    break;
                default:
                    binaryWriter.Write(value.ToString());
                    break;
            }
        }

        private static void WriteArrayValues(Object arrayObject, BinaryWriter binaryWriter)
        {
            var array = (object[])arrayObject;
            binaryWriter.Write(array.Length);
            foreach (var obj in array)
            {
                WriteValueByType(obj, binaryWriter);
            }
        }

        public static object Deserialize(MemoryStream stream, Type type)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var decompressionStream = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                using (var binaryReaderStream = new MemoryStream(1024))
                {
                    decompressionStream.CopyTo(binaryReaderStream);
                    binaryReaderStream.Seek(0, SeekOrigin.Begin);
                    using (var binaryReader = new BinaryReader(binaryReaderStream))
                    {
                        return Deserialize(type, binaryReader);
                    }
                }
            }
        }

        private static object Deserialize(Type type, BinaryReader binaryReader)
        {
            var obj = Activator.CreateInstance(type);
            var name = binaryReader.ReadString();
            if (String.Compare(name, type.FullName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException(String.Format(
                    "Stream does not contains info for serialization type of {0}", type.FullName));
            }
            var fields =
                type.GetFields(BindingFlags)
                    .Where(fi => !Regex.IsMatch(fi.Name, AutoPropertyPattern)) // Skip AutoProperty fields
                    .ToDictionary(fi => String.Format("{0}", fi.Name));
            var properties =
                type.GetProperties(BindingFlags).ToDictionary(pi => String.Format("{0}", pi.Name));
            var classCountFields = binaryReader.ReadInt32();
            for (int i = 0; i < classCountFields; i++)
            {
                name = binaryReader.ReadString();
                if (fields.ContainsKey(name))
                {
                    var fieldToSet = fields[name];
                    var valueToSet = ReadValue(fieldToSet.FieldType, binaryReader);
                    fieldToSet.SetValue(obj, valueToSet);
                }
                else if (properties.ContainsKey(name))
                {
                    var propertyToSet = properties[name];
                    var valueToSet = ReadValue(propertyToSet.PropertyType, binaryReader);
                    propertyToSet.SetValue(obj, valueToSet, null);
                }
            }
            return obj;
        }

        private static object ReadValue(Type type, BinaryReader binaryReader)
        {
            object obj;
            var typeCode = binaryReader.ReadByte();
            switch ((TypeCode) typeCode)
            {
                case TypeCode.Boolean:
                    obj = binaryReader.ReadBoolean();
                    break;
                case TypeCode.Char:
                    obj = binaryReader.ReadChar();
                    break;
                case TypeCode.SByte:
                    obj = binaryReader.ReadSByte();
                    break;
                case TypeCode.Byte:
                    obj = binaryReader.ReadByte();
                    break;
                case TypeCode.Int16:
                    obj = binaryReader.ReadInt16();
                    break;
                case TypeCode.UInt16:
                    obj = binaryReader.ReadUInt16();
                    break;
                case TypeCode.Int32:
                    obj = binaryReader.ReadInt32();
                    break;
                case TypeCode.UInt32:
                    obj = binaryReader.ReadUInt32();
                    break;
                case TypeCode.Int64:
                    obj = binaryReader.ReadInt64();
                    break;
                case TypeCode.UInt64:
                    obj = binaryReader.ReadUInt64();
                    break;
                case TypeCode.Single:
                    obj = binaryReader.ReadSingle();
                    break;
                case TypeCode.Double:
                    obj = binaryReader.ReadDouble();
                    break;
                case TypeCode.Decimal:
                    obj = binaryReader.ReadDecimal();
                    break;
                case TypeCode.DateTime:
                    obj = DateTime.FromFileTime(binaryReader.ReadInt64());
                    break;
                case TypeCode.String:
                    obj = binaryReader.ReadString();
                    break;
                case TypeCode.Array:
                    obj = ReadArrayValues(type, binaryReader);
                    break;
                case TypeCode.Class:
                    obj = Deserialize(type, binaryReader);
                    break;
                default:
                    obj = binaryReader.ReadString();
                    break;
            }
            return obj;
        }

        private static object ReadArrayValues(Type type, BinaryReader binaryReader)
        {
            var count = binaryReader.ReadInt32();
            var array = Array.CreateInstance(type.GetElementType(), count);
            for (var i = 0; i < count; i++)
            {
                array.SetValue(ReadValue(type.GetElementType(), binaryReader), i);
            }
            return array;
        }
    }
}
