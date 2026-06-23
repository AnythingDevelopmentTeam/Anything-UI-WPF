using System.Collections.Generic;
using Anything_UI_WPF.Models;
using Anything_UI_WPF.Native;

namespace Anything_UI_WPF.Services;

public enum SearchType { Fuzzy, Regex, Exact }

public class SearchService
{
    public bool IsEngineAvailable => SearchEngineDll.IsAvailable;

    public bool LoadIndex(string path)
    {
        if (!SearchEngineDll.IsAvailable) return false;
        return SearchEngineDll.load_index_from_file(path) == 0;
    }

    public ulong GetIndexSize()
    {
        if (!SearchEngineDll.IsAvailable) return 0;
        return SearchEngineDll.index_size();
    }

    public List<FileSearchResult> Search(string query, SearchType type = SearchType.Fuzzy)
    {
        var results = new List<FileSearchResult>();
        if (!SearchEngineDll.IsAvailable || string.IsNullOrWhiteSpace(query))
            return results;

        var count = SearchEngineDll.search_query(query, (int)type);
        for (ulong i = 0; i < count; i++)
        {
            var path = SearchEngineDll.GetResultByIndex(i);
            if (path != null)
                results.Add(new FileSearchResult { FullPath = path });
        }
        return results;
    }
}
