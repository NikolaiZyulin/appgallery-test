using System.Collections.Generic;

namespace Analytics
{
    public interface IAnalytic
    {
        void Init();
        void LogEvent(string action);
        void LogEvent(string action, Dictionary<string, string> parameters);
        void SetUserID(string userId);
    }
}