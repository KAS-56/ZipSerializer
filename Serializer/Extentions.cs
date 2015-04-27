using System;

namespace Serializer
{
    public static class Extentions
    {
        public static TypeCode GetTypeCode(this Type type)
        {
            var typeCode = (byte) Type.GetTypeCode(type);
            if (type.IsEnum)
            {
                typeCode = 18;
            }
            if (type.FullName == "System.String")
            {
                typeCode = 17;
            }
            else if (type.IsArray)
            {
                typeCode = 2;
            }
            else if (type.IsClass ||
                     (type.IsValueType && !type.IsPrimitive && type.Namespace != null &&
                      !type.Namespace.StartsWith("System") && !type.FullName.StartsWith("System.")))
            {
                typeCode = 1;
            }
            return (TypeCode) typeCode;
        }
    }
}
