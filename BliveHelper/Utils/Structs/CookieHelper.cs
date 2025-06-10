using System.Collections;
using System.Net;

namespace BliveHelper.Utils.Structs
{
    public static class CookieHelper
    {
        public static CookieCollection GetAllCookies(this CookieContainer container)
        {
            var domainTable = container.GetPropertyValue<Hashtable>("m_domainTable");

            var result = new CookieCollection();
            lock (domainTable.SyncRoot)
            {
                var lists = domainTable.GetEnumerator();
                while (lists.MoveNext())
                {
                    var list = lists.Value.GetPropertyValue<SortedList>("m_list");
                    lock (list.SyncRoot)
                    {
                        var collections = list.GetEnumerator();
                        while (collections.MoveNext())
                        {
                            result.Add((CookieCollection)collections.Value);
                        }
                    }
                }
            }

            return result;
        }
    }
}
