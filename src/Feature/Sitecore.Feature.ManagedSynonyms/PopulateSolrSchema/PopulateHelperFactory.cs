using Sitecore.ContentSearch.SolrProvider.Abstractions;
using Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema;
using SolrNet.Schema;

namespace Sitecore.Feature.ManagedSynonyms.PopulateSolrSchema
{
    public class PopulateHelperFactory: IPopulateHelperFactory
    {
        /// <summary>
        /// Creates that inherited from  <see cref="ISchemaPopulateHelper"/>
        /// </summary>
        /// <param name="solrSchema">
        /// The current SOLR schema.
        /// </param>
        /// <returns>
        /// Instance inherited from <see cref="ISchemaPopulateHelper"/>.
        /// </returns>
        public ISchemaPopulateHelper GetPopulateHelper(SolrSchema solrSchema)
        {
            return new SchemaPopulateHelper(solrSchema);
        }
    }
}