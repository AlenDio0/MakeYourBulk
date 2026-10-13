using System.Reflection;

namespace MakeYourBulk
{
    public static class CloneExtensions
    {
        public static T ShallowClone<T>(this T obj) where T : class
        {
            if (obj == null)
                return null;

            MethodInfo method = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)method.Invoke(obj, null);
        }
    }
}
