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

        public async Task<SynonymResponse> GetSymmetricSynonyms(string core)
        {
            var url = $"{_connectionString}/{core}/schema/analysis/synonyms/english";
            var result = await _client.GetAsync(url);
            return JsonConvert.DeserializeObject<SynonymResponse>(await result.Content.ReadAsStringAsync());
        }

        public async Task AddSymmetricSynonyms(string core, string[] words)
        {
            var url = $"{_connectionString}/{core}/schema/analysis/synonyms/english";
            var content = new StringContent(JsonConvert.SerializeObject(words), Encoding.UTF8, "application/json");
            await _client.PostAsync(url, content);
        }

        public async Task DeleteSymmetricSynonyms(string core, string synonym)
        {
            var url = $"{_connectionString}/{core}/schema/analysis/synonyms/english/{synonym}";
            await _client.DeleteAsync(url);
        }

        public void ReloadCore(string core)
        {
            var url = $"{_connectionString}/admin/cores?action=RELOAD&core={core}";
            _client.GetAsync(url);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}