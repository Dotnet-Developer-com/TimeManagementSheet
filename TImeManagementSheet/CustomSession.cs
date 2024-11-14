using Newtonsoft.Json;
namespace ERPClient
{
    public static class CustomSession
    {
        public static T GetSessionData<T>(this ISession session, string key)
        {
            var data = session.GetString(key);
            if (data == null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(data);
        }
        public static void SetSessionData(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

    }
}
