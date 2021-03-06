using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Diagnostics;

namespace Sitecore.Feature.ManagedSynonyms.Services
{
    public class SymmetricSynonymsService
    {
        private readonly SolrClientService _clientService;
        private readonly ISynonymItemsService _synonymItemsService;

        public SymmetricSynonymsService(ISynonymItemsService synonymItemsService)
        {
            _clientService = new SolrClientService();
            _synonymItemsService = synonymItemsService;
        }
        
        /// <summary>
        /// Executes synchronization of both the Sitecore Synonyms And The Solr managed Synonyms
        /// </summary>
        public void Sync()
        {
            try
            {
                var synonymItems = _synonymItemsService.GetSynonyms();
                var solrCores = _synonymItemsService.GetCore().ToList();

                Parallel.ForEach(solrCores, async core =>
                {
                    var symmetricSynonyms = await _clientService.GetSymmetricSynonymsAsync(core);
                    var solrSynonyms = symmetricSynonyms.SynonymMappings.ManagedMap.Keys;
                    var validSynonyms = new List<string>();

                    foreach (var synonymItem in synonymItems)
                    {
                        foreach (var synonym in synonymItem)    
                        {
                            if (solrSynonyms.Contains(synonym))
                                validSynonyms.Add(synonym);
                        }
                        await _clientService.AddSymmetricSynonymsAsync(core, synonymItem.ToArray());
                    }

                    var invalidSynonyms = solrSynonyms.Except(validSynonyms);
                    foreach (var synonym in invalidSynonyms)
                    {
                        await _clientService.DeleteSymmetricSynonymsAsync(core, synonym);
                    }
                });
                Parallel.ForEach(solrCores, core => _clientService.ReloadCore(core));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, this);
            }
        }
    }
}