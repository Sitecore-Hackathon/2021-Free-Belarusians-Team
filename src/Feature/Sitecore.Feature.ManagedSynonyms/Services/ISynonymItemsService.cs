using System.Collections.Generic;

namespace Sitecore.Feature.ManagedSynonyms.Services
{
    /// <summary>
    /// The Service that makes easiest work with Synonyms that store in Sitecore. 
    /// </summary>
    public interface ISynonymItemsService
    {
        /// <summary>
        /// Gets specified core names from root settings synonym folder. 
        /// </summary>
        /// <returns>
        /// Collection of core names. 
        /// </returns>
        IEnumerable<string> GetCore();
        
        /// <summary>
        /// Returns specified in Sitecore synonyms to Solr-friendly model.  
        /// </summary>
        /// <returns>
        /// Array of synonyms with synonyms. 
        /// </returns>
        IEnumerable<IEnumerable<string>> GetSynonyms();
    }
}