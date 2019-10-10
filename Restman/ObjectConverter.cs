namespace Restman
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bifoql.Adapters;
    using Bifoql.Types;
    using Newtonsoft.Json.Linq;

    public static class ObjectConverter
    {
        internal static object ToAsyncObject(object o)
        {
            if (o == null) return null;

            if (o is JObject || o is JArray || o is JToken || o is JValue) return ConvertJToken((JToken)o);

            return o;
        }

        private static object ConvertJToken(JToken j)
        {
            var jobj = j as JObject;
            if (jobj != null)
            {
                var dict = new Dictionary<string, object>();
                foreach (var pair in jobj)
                {
                    dict[pair.Key] = ToAsyncObject(pair.Value);
                }

                return new Dictionary<string, object>(dict, null);
            }

            var jarr = j as JArray;
            if (jarr != null)
            {
                var arr = new List<object>();

                foreach (var el in jarr)
                {
                    arr.Add(ToAsyncObject(el));
                }

                return arr.ToArray();
            }

            var jval = j as JValue;
            if (jval != null)
            {
                var val = jval.Value;
                if (jval.Type == JTokenType.String)
                {
                    return (string)val;
                }
                else if (jval.Type == JTokenType.Float || jval.Type == JTokenType.Integer)
                {
                    return Convert.ToDouble(val);
                }
                else if (jval.Type == JTokenType.Boolean)
                {
                    return (bool)val;
                }
            }

            return ((JToken)j).Value<string>();
        }
    }
}