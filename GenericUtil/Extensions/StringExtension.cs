using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericUtil.Extensions
{
    public static class StringExtension
    {
        public static string ReplaceId<T>(this string baseString, int id, T value)
        {
            string replaceId = "{" + id + "}";
            return baseString.Replace(replaceId, value.ToString());
        }
    }
}
