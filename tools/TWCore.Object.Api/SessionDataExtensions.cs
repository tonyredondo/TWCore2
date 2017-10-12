using Microsoft.AspNetCore.Http;
using TWCore.Serialization;

namespace TWCore.Object.Api
{
    public static class SessionDataExtensions
    {
        public static SessionData GetSessionData(this ISession session)
        {
            return session.TryGetValue("$SESSIONDATA$", out var dataBytes)
                ? SerializerManager.DefaultBinarySerializer.Deserialize<SessionData>(dataBytes)
                : new SessionData();
        }
        public static void SetSessionData(this ISession session, SessionData data)
        {
            var dataBytes = SerializerManager.DefaultBinarySerializer.Serialize(data);
            session.Set("$SESSIONDATA$", (byte[])dataBytes);
        }
    }
}
