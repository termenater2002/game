namespace Unity.Muse.Common
{
    internal interface ICache<T>
    {
        void Initialize();
        void Clear();
        bool Contains(string key);
        void Add(string key, T value);
        T Read(string key);
        byte[] ReadRawData(string key);
    }
}
