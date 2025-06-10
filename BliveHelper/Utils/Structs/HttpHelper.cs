using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace BliveHelper.Utils.Structs
{
    public static class HttpHelper
    {
        public static async Task<T> ReadFromJsonAsync<T>(this HttpContent httpContent)
        {
            var contentString = await httpContent.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(contentString))
            {
                return JsonConvert.DeserializeObject<T>(contentString);
            }
            return default;
        }
    }
}
