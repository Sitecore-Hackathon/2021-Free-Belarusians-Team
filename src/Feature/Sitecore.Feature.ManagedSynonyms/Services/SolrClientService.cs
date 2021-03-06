using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Sitecore.Feature.ManagedSynonyms.Models;

namespace Sitecore.Feature.ManagedSynonyms.Services
{
    public sealed class SolrClientService : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _connectionString;

        public SolrClientService()
        {
            _client = new HttpClient();
            _connectionString = ConfigurationManager.ConnectionStrings["solr.search"].ConnectionString;
        }
        
        /// <summary>
        /// Gets all synonyms from core. 
        /// </summary>
        /// <param name="core">
        /// The SOLR core that used 
        /// </param>
        /// <returns>
        /// The SOLR managed mappings response.   
        /// </returns>
        public async Task<SynonymResponse> GetSymmetricSynonymsAsync(string core)
        {
            var url = $"{_connectionString}/{core}/schema/analysis/synonyms/english";
            var result = await _client.GetAsync(url);
            return JsonConvert.DeserializeObject<SynonymResponse>(await result.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Adds synonyms to specific core. 
        /// </summary>
        /// <param name="core">
        /// The core where would be added synonyms collection. 
        /// </param>
        /// <param name="words">
        /// Synonyms that will be added in core. 
        /// </param>
        public async Task AddSymmetricSynonymsAsync(string core, string[] words)
        {
            var url = $"{_connectionString}/{core}/schema/analysis/synonyms/english";
            var content = new StringContent(JsonConvert.SerializeObject(words), Encoding.UTF8, "application/json");
            await _client.PostAsync(url, content);
        }

        /// <summary>
        /// Deletes specific synonym from specific core. 
        /// </summary>
        /// <param name="core">
        /// The synonyms core name. 
        /// </param>
        /// <param name="synonym">
        /// The synonyms that should be removed. 
        /// </param>
        public async Task DeleteSymmetricSynonymsAsync(string core, string synonym)
        {
            var url = $"{_connectionString}/{core}/schema/analysis/synonyms/english/{synonym}";
            await _client.DeleteAsync(url);
        }

        /// <summary>
        /// Reloads Cores functions 
        /// </summary>
        /// <param name="core">
        /// The name of SOLR core. 
        /// </param>
        public void ReloadCore(string core)
        {
            var url = $"{_connectionString}/admin/cores?action=RELOAD&core={core}";
            _client.GetAsync(url);
        }

        /// <summary>
        /// Disposes <see cref="SolrClientService"/>.
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}