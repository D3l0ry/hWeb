using System.Collections.Generic;

namespace hWeb
{
    public class RequestHeader
    {
        private readonly Dictionary<string, string> m_RequestValue = new Dictionary<string, string>();

        public string this[string Key]
        {
            get => RequestValue[Key];
            set => RequestValue.Add(Key, value);
        }

        public Dictionary<string, string> RequestValue { get => m_RequestValue; }
    }
}