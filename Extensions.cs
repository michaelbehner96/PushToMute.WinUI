using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PushToMute.WinUI
{
    internal static class Extensions
    {
        public static T? GetJsonSerializedObject<T>(this ApplicationDataContainer data, string key)
        {
            if (!data.Values.ContainsKey(key))
                return default(T);

            if (data.Values[key] is not string)
                throw new Exception($"Failed getting application data: {key}; Entry is not a string.");

            string json = data.Values[key] as string;

            if (string.IsNullOrEmpty(json))
                return default(T);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void SetJsonSerializedObject<T>(this ApplicationDataContainer data, string key, T value)
        {
            data.Values[key] = JsonConvert.SerializeObject(value);
        }
    }
}
