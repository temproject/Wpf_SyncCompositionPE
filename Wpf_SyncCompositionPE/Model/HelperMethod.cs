using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf_SyncCompositionPE.Model
{
    static public class HelperMethod
    {
        /// <summary>
        /// Очищает коллекцию
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        static void Clear<T>(this  Collection<T> list)
        {
            if (list != null)
            {
                list.Clear();
            }
        }

        public static bool IsListNullOrEmpty<T>(this IEnumerable<T> list)
        {
            if (list == null || list.Count() == 0)
                return true;

            return false;
        }



    }
}
