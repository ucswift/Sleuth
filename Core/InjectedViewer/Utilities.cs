using System;

namespace Sleuth.InjectedViewer
{
    public static class Utilities
    {
        #region IsPrimitiveType

        public static bool IsPrimitiveType(Type type)
        {
            return
             type == typeof(Object) ||
             type == typeof(String) ||
             type == typeof(Char) ||
             type == typeof(Boolean) ||
             type == typeof(Byte) ||
             type == typeof(Int16) ||
             type == typeof(Int32) ||
             type == typeof(Int64) ||
             type == typeof(UInt16) ||
             type == typeof(UInt32) ||
             type == typeof(UInt64) ||
             type == typeof(IntPtr) ||
             type == typeof(Single) ||
             type == typeof(Double) ||
             type == typeof(Decimal);
        }

        #endregion // IsPrimitiveType
    }
}