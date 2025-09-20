using System;
using System.Collections.Generic;

namespace ERP_AI.Desktop.Services
{
    public class SearchQuery
    {
        public string SearchText { get; set; } = string.Empty;
        public List<string> EntityTypes { get; set; } = new List<string>();
        public List<SearchFilter> Filters { get; set; } = new List<SearchFilter>();
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "Asc";
    }

    public class AdvancedSearchQuery
    {
        public List<SearchCondition> Conditions { get; set; } = new List<SearchCondition>();
        public string LogicalOperator { get; set; } = "AND";
        public List<string> EntityTypes { get; set; } = new List<string>();
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "Asc";
    }

    public class SearchCondition
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public object Value { get; set; } = new object();
        public string DataType { get; set; } = string.Empty;
    }

    public class SearchResult
    {
        public List<SearchResultItem> Items { get; set; } = new List<SearchResultItem>();
        public int TotalCount { get; set; }
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }
        public TimeSpan SearchTime { get; set; }
        public List<SearchFacet> Facets { get; set; } = new List<SearchFacet>();
    }

    public class SearchResultItem
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Highlight { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    public class SearchFacet
    {
        public string Name { get; set; } = string.Empty;
        public List<FacetValue> Values { get; set; } = new List<FacetValue>();
    }

    public class FacetValue
    {
        public string Value { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool IsSelected { get; set; }
    }

    public class SearchSuggestion
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
    }

    public class SearchHistory
    {
        public Guid Id { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public SearchQuery Query { get; set; } = new SearchQuery();
        public DateTime SearchDate { get; set; }
        public int ResultCount { get; set; }
        public bool IsSuccessful { get; set; }
    }

    public class SavedSearch
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SearchQuery Query { get; set; } = new SearchQuery();
        public bool IsPublic { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUsed { get; set; }
        public int UseCount { get; set; }
    }

    public class SearchFilter
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new List<string>();
        public string Operator { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class SearchSettings
    {
        public bool EnableFuzzySearch { get; set; } = true;
        public bool EnableWildcardSearch { get; set; } = true;
        public bool EnablePhraseSearch { get; set; } = true;
        public int MaxResults { get; set; } = 1000;
        public int SuggestionLimit { get; set; } = 10;
        public bool SaveSearchHistory { get; set; } = true;
        public int HistoryLimit { get; set; } = 100;
        public bool EnableAutoComplete { get; set; } = true;
        public List<string> SearchableFields { get; set; } = new List<string>();
    }

    public class SearchIndexStatus
    {
        public bool IsIndexed { get; set; }
        public DateTime LastIndexed { get; set; }
        public int TotalDocuments { get; set; }
        public int IndexedDocuments { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan IndexingTime { get; set; }
    }

    public class SearchStatistics
    {
        public int TotalSearches { get; set; }
        public int SuccessfulSearches { get; set; }
        public int FailedSearches { get; set; }
        public TimeSpan AverageSearchTime { get; set; }
        public List<string> PopularSearches { get; set; } = new List<string>();
        public DateTime LastSearchDate { get; set; }
    }
}
