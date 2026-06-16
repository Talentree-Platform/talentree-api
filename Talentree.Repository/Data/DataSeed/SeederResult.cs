using System.Collections.Generic;

namespace Talentree.Repository.Data.DataSeed
{
    public class SeederResult<TKey> where TKey : notnull
    {
        public int InsertedCount { get; set; }
        public Dictionary<TKey, int> IdMap { get; set; } = new();
    }
}
