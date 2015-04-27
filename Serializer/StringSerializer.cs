using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Serializer
{
    public class StringSerializer
    {
        private const string AutoPropertyPattern = @"<(.*)>k__BackingField";

        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.NonPublic |
                                                  System.Reflection.BindingFlags.Instance;

        public static string Serializer(Object obj)
        {
            var stringBuilder = new StringBuilder();
            var objType = obj.GetType();
            stringBuilder.AppendLine(objType.FullName);
            var fields = objType.GetFields(BindingFlags).
                Where(fi => !Regex.IsMatch(fi.Name, AutoPropertyPattern)); // Skip AutoProperty fields
            foreach (var fieldInfo in fields)
            {
                var value = fieldInfo.GetValue(obj);
                if (value == null) continue;
                value = GetValue(fieldInfo.FieldType, value);
                stringBuilder.AppendFormat("{0}.{1}:{2}{3}", objType.FullName, fieldInfo.Name, value,
                    Environment.NewLine);
            }
            var properties = objType.GetProperties(BindingFlags);
            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(obj, null);
                if (value == null) continue;
                value = GetValue(propertyInfo.PropertyType, value);
                stringBuilder.AppendFormat("{0}.{1}:{2}{3}", objType.FullName, propertyInfo.Name, value,
                    Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        private static object GetValue(Type type, object value)
        {
            if (type.IsArray)
            {
                value = GetArrayValues(value);
            }
            else if (type.IsValueType == false && type.FullName != "System.String")
            {
                value = Serializer(value);
            }
            return value;
        }

        private static string GetArrayValues(Object arrayObject)
        {
            var sb = new StringBuilder();
            var array = (object[]) arrayObject;
            foreach (var obj in array)
            {
                sb.AppendFormat("{0}|", GetValue(obj.GetType(), obj));
            }
            return sb.ToString().Trim('|');
        }

        public static object Deserialize(Type type, string serializedObject)
        {
            var obj = Activator.CreateInstance(type);
            using (var reader = new StringReader(serializedObject))
            {
                var line = reader.ReadLine();
                if (String.Compare(line, type.FullName, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new ArgumentException(
                        String.Format("value does not contains info for serialization type of {0}", type.FullName),
                        "serializedObject");
                }
                var fields =
                    type.GetFields(BindingFlags)
                        .Where(fi => !Regex.IsMatch(fi.Name, AutoPropertyPattern)) // Skip AutoProperty fields
                        .ToDictionary(fi => String.Format("{0}.{1}", type.FullName, fi.Name));
                var properties =
                    type.GetProperties(BindingFlags).ToDictionary(pi => String.Format("{0}.{1}", type.FullName, pi.Name));
                var classFieldNames = fields.Keys.Union(properties.Keys).ToList();
                while (!String.IsNullOrWhiteSpace(line = reader.ReadLine()))
                {
                    var name = line.Substring(0, line.IndexOf(':'));
                    var value = line.Substring(line.IndexOf(':') + 1);
                    if (fields.ContainsKey(name))
                    {
                        var fieldToSet = fields[name];
                        var valueToSet = GetObjectToSet(fieldToSet.FieldType, value, reader, classFieldNames);
                        fieldToSet.SetValue(obj, valueToSet);
                    }
                    else if (properties.ContainsKey(name))
                    {
                        var propertyToSet = properties[name];
                        var valueToSet = GetObjectToSet(propertyToSet.PropertyType, value, reader, classFieldNames);
                        propertyToSet.SetValue(obj, valueToSet, null);
                    }
                    else return obj;
                }
            }
            return obj;
        }

        private static object GetObjectToSet(Type type, string value, StringReader reader, ICollection<string> classFieldNames)
        {
            if (type.IsArray)
            {
                var arrayValues = value.Split('|');
                var array = Array.CreateInstance(type.GetElementType(), arrayValues.Length);
                var i = 0;
                foreach (var str in arrayValues)
                {
                    array.SetValue(ConvertFromString(type, str), i++);
                }
                return array;
            }
            if (type.IsValueType || type.FullName == "System.String")
            {
                return ConvertFromString(type, value);
            }
            var sb = new StringBuilder();
            sb.AppendLine(value);
            string line;
            while (!String.IsNullOrWhiteSpace(line = reader.ReadLine()))
            {
                var name = line.Substring(0, line.IndexOf(':'));
                if (classFieldNames.Contains(name))
                {
                    break;
                }
                sb.AppendLine(line);
            }
            var nestedObj = Deserialize(type, sb.ToString());
            return nestedObj;
        }

        private static object ConvertFromString(Type destinationType, string value)
        {
            var formatProvider = CultureInfo.InvariantCulture;
            if (destinationType == typeof (Boolean))
            {
                return Convert.ToBoolean(value, formatProvider);
            }
            if (destinationType == typeof (Byte))
            {
                return Convert.ToByte(value, formatProvider);
            }
            if (destinationType == typeof (Char))
            {
                return Convert.ToChar(value, formatProvider);
            }
            if (destinationType == typeof (DateTime))
            {
                return Convert.ToDateTime(value, formatProvider);
            }
            if (destinationType == typeof (Decimal))
            {
                return Convert.ToDecimal(value, formatProvider);
            }
            if (destinationType == typeof (Double))
            {
                return Convert.ToDouble(value, formatProvider);
            }
            if (destinationType == typeof (Int16))
            {
                return Convert.ToInt16(value, formatProvider);
            }
            if (destinationType == typeof (Int32))
            {
                return Convert.ToInt32(value, formatProvider);
            }
            if (destinationType == typeof (Int64))
            {
                return Convert.ToInt64(value, formatProvider);
            }
            if (destinationType == typeof (SByte))
            {
                return Convert.ToSByte(value, formatProvider);
            }
            if (destinationType == typeof (Single))
            {
                return Convert.ToSingle(value, formatProvider);
            }
            if (destinationType == typeof (UInt16))
            {
                return Convert.ToUInt16(value, formatProvider);
            }
            if (destinationType == typeof (UInt32))
            {
                return Convert.ToUInt32(value, formatProvider);
            }
            if (destinationType == typeof (UInt64))
            {
                return Convert.ToUInt64(value, formatProvider);
            }
            return value;
        }
    }
}
