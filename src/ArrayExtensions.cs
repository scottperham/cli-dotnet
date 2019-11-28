using System;

namespace cli_dotnet
{
    public static class ArrayExtensions
    {
        public static Array ExtendAndAddArray(this Array existingArray, Array newArray)
        {
            var type = existingArray.GetType().GetElementType();

            var valueArray = Array.CreateInstance(type, existingArray.Length + newArray.Length);
            Array.Copy(existingArray, 0, valueArray, 0, existingArray.Length);
            Array.Copy(newArray, 0, valueArray, existingArray.Length, newArray.Length);

            return valueArray;
        }

        public static Array ExtendAndAdd(this Array existingArray, object value)
        {
            var type = existingArray.GetType().GetElementType();

            var newArray = Array.CreateInstance(type, existingArray.Length + 1);
            Array.Copy(existingArray, newArray, existingArray.Length);

            newArray.SetValue(value, existingArray.Length);

            return newArray;
        }
    }
}
