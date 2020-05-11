namespace SavescumBuddy.Sqlite
{
    interface ISearchRequest
    {
        int? Offset { get; set; }
        int? Limit { get; set; }
        string Order { get; set; }
        string GroupBy { get; set; }
    }
}
