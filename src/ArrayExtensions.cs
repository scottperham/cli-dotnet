using System;

namespace cli_dotnet
{
    public static class ArrayExtensions
    {
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
