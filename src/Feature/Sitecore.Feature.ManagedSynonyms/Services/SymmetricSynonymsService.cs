using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Diagnostics;
using Sitecore.Jobs;

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
        
        public void Sync()
        {
            try
            {
                var synonymItems = _synonymItemsService.GetSynonyms();
                var solrCores = _synonymItemsService.GetCores().ToList();

                Parallel.ForEach(solrCores, async core =>
                {
                    var symmetricSynonyms = await _clientService.GetSymmetricSynonyms(core);
                    var solrSynonyms = symmetricSynonyms.SynonymMappings.ManagedMap.Keys;
                    var validSynonyms = new List<string>();

                    foreach (var synonymItem in synonymItems)
                    {
                        foreach (var synonym in synonymItem)    
                        {
                            if (solrSynonyms.Contains(synonym))
                                validSynonyms.Add(synonym);
                        }
                        await _clientService.AddSymmetricSynonyms(core, synonymItem.ToArray());
                    }

                    var invalidSynonyms = solrSynonyms.Except(validSynonyms);
                    foreach (var synonym in invalidSynonyms)
                    {
                        await _clientService.DeleteSymmetricSynonyms(core, synonym);
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