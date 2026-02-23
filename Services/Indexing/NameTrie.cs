namespace IndexingSystem.Services.Indexing
{
    public class NameTrie : ISearchIndex
    {
        private readonly NameTrieNode _root;

        public NameTrie()
        {
            _root = new NameTrieNode();
        }

        public void Insert(string word, int contactId)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;

            var current = _root;
            foreach (char c in word.ToLower())
            {
                if (!current.Children.ContainsKey(c))
                {
                    current.Children[c] = new NameTrieNode();
                }
                current = current.Children[c];
            }

            current.ContactIds.Add(contactId);
        }

        public void Remove(string word, int contactId)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;

            var current = _root;
            foreach (char c in word.ToLower())
            {
                if (!current.Children.ContainsKey(c))
                    return; 

                current = current.Children[c];
            }

            current.ContactIds.Remove(contactId);
        }

        public IEnumerable<int> SearchPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return Enumerable.Empty<int>();

            var current = _root;
            foreach (char c in prefix.ToLower())
            {
                if (!current.Children.ContainsKey(c))
                {
                    return Enumerable.Empty<int>();
                }
                current = current.Children[c];
            }

            var results = new HashSet<int>();
            CollectIdsDfs(current, results);
            return results;
        }

        private void CollectIdsDfs(NameTrieNode node, HashSet<int> results)
        {
            if (node.IsLeaf)
            {
                foreach (var id in node.ContactIds)
                {
                    results.Add(id);
                }
            }

            foreach (var child in node.Children.Values)
            {
                CollectIdsDfs(child, results);
            }
        }

        public void Clear()
        {
            _root.Children.Clear(); 
        }
    }

}
