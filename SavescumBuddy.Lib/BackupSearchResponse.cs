using System.Collections.Generic;

namespace SavescumBuddy.Lib
{
    public class BackupSearchResponse
    {
        public List<Backup> Backups { get; set; }
        public int TotalCount { get; set; }
    }
}
