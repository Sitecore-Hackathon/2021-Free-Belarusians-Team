using Sitecore.ContentSearch.SolrProvider.Abstractions;
using Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema;
using SolrNet.Schema;

namespace Sitecore.Feature.ManagedSynonyms.PopulateSolrSchema
{
    public class PopulateHelperFactory: IPopulateHelperFactory
    {
        public ISchemaPopulateHelper GetPopulateHelper(SolrSchema solrSchema)
        {
            return new SchemaPopulateHelper(solrSchema);
        }
    }
}