using System.Reflection;

namespace BliveHelper.Utils.Structs
{
    public static class ObjectHelper
    {
        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            return (T)obj.GetType().InvokeMember(
                propertyName,
                BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
                null,
                obj,
                null
            );
        }
    }
}
