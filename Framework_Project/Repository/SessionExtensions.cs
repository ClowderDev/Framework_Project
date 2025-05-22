

using Newtonsoft.Json;

namespace Framework_Project.Repository
{
    public static class SessionExtensions
	{
        //Lưu dữ liệu vào session dưới dạng chuỗi json với tham số key và value
		public static void SetJson(this ISession session, string key, object value)
		{
			session.SetString(key, JsonConvert.SerializeObject(value));
		}

        //Lấy dữ liệu từ session đã được giải mã
		public static T GetJson<T>(this ISession session, string key)
		{
			var sessionData = session.GetString(key);
			return sessionData == null ? default(T) : JsonConvert.DeserializeObject<T>(sessionData);
		}
	}
}
