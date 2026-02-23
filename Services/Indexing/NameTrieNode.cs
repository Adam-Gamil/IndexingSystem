namespace IndexingSystem.Services.Indexing
{
    public class NameTrieNode
    {
        public Dictionary<char, NameTrieNode> Children { get; } = new Dictionary<char, NameTrieNode>();
        public HashSet<int> ContactIds { get; } = new HashSet<int>();
        public bool IsLeaf => ContactIds.Count > 0;
    }
}
