using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Feature.ManagedSynonyms.Services
{
    public class SynonymItemsService : ISynonymItemsService
    {
        public IEnumerable<string> GetCore()
        {
            return GetRoot()[Templates.ManagedSynonyms.ManagedSynonymsCoresFieldId]
                ?.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<IEnumerable<string>> GetSynonyms()
        {
            var items = GetRoot().Axes.GetDescendants()
                .Where(x => x.TemplateID == Templates.ManagedSynonyms.ManagedSynonymTemplateId);
            return items.Select(
                x => x[Templates.ManagedSynonyms.SynonymsFieldId]?.Split(
                    new[] { "|" },
                    StringSplitOptions.RemoveEmptyEntries));
        }
        
        private Item GetRoot()
        {
            var path = Configuration.Settings.GetSetting("ManagedSynonyms.Folder");
            var item = Data.Database.GetDatabase("master").GetItem(path);
            Assert.IsTrue(
                item.TemplateID == Templates.ManagedSynonyms.ManagedSynonymsFolderTemplateId,
                "Invalid synonyms folder");
            return item;

        }
    }
}