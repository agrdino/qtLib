using System.Collections.Generic;

namespace qtLib.Helper
{
    public interface ICloneable<T> where T : class
    {
        public T Clone();
    }

    public static partial class GameHelper
    {
        public static List<T> Clone<T>(this List<T> listToClone) where T : class, ICloneable<T>
        {
            List<T> result = new List<T>();
            listToClone.ForEach(target => result.Add(target.Clone()));
            return result;
        }
    }
}