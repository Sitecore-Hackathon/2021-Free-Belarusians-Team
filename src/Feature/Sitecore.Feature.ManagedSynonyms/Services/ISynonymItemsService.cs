using System.Collections.Generic;

namespace Sitecore.Feature.ManagedSynonyms.Services
{
    public interface ISynonymItemsService
    {
        IEnumerable<string> GetCores();
        IEnumerable<IEnumerable<string>> GetSynonyms();
    }
}