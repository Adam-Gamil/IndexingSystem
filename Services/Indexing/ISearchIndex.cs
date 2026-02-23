namespace IndexingSystem.Services.Indexing
{
    public interface ISearchIndex
    {
        void Insert(string key, int contactId);
        void Remove(string key, int contactId);
        IEnumerable<int> SearchPrefix(string prefix);
        void Clear();
    }
}
