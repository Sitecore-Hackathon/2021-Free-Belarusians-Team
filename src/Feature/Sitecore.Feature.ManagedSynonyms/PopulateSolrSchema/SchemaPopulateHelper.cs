using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema;
using Sitecore.Diagnostics;
using SolrNet;
using SolrNet.Schema;
using SolrCopyField = Sitecore.ContentSearch.SolrProvider.SolrCopyField;

namespace Sitecore.Feature.ManagedSynonyms.PopulateSolrSchema
{
    public class SchemaPopulateHelper : ISchemaPopulateHelper
    {
        private readonly SolrSchema solrSchema;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema.SchemaPopulateHelper" /> class.
        /// </summary>
        /// <param name="solrSchema"> Solr schema.</param>
        public SchemaPopulateHelper(SolrSchema solrSchema)
        {
            Assert.ArgumentNotNull((object)solrSchema, nameof(solrSchema));
            this.solrSchema = solrSchema;
        }

        /// <summary>
        /// Gets list of added and removed fields, copy fields and dynamic fields to be populated to Solr schema.
        /// </summary>
        /// <returns>List of all fields</returns>
        public virtual IEnumerable<XElement> GetAllFields() => this.GetRemoveFields()
            .Union<XElement>(this.GetAddFields()).Where<XElement>((Func<XElement, bool>)(o => o != null));

        /// <summary>
        /// Gets list of added and replaced field types to be populated to the schema.
        /// </summary>
        /// <returns>List of all fields</returns>
        public virtual IEnumerable<XElement> GetAllFieldTypes() => this.GetReplaceFields()
            .Union<XElement>(this.GetAddFieldTypes()).Where<XElement>((Func<XElement, bool>)(o => o != null));

        /// <summary>Check if type exists in solr schema.</summary>
        /// <param name="type">The type</param>
        /// <returns>true if type exists in solr schema, otherwise; false.</returns>
        protected virtual bool TypeExists(string type) => this.solrSchema.FindSolrFieldTypeByName(type) != null;

        /// <summary>Creates the field.</summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="required">
        /// if set to <c>true</c> [required].
        /// </param>
        /// <param name="indexed">
        /// if set to <c>true</c> [indexed].
        /// </param>
        /// <param name="stored">
        /// if set to <c>true</c> [stored].
        /// </param>
        /// <param name="multiValued">
        /// if set to <c>true</c> [multi valued].
        /// </param>
        /// <param name="omitNorms">
        /// if set to <c>true</c> [omit norms].
        /// </param>
        /// <param name="termVectors">
        /// if set to <c>true</c> [term vectors].
        /// </param>
        /// <param name="termPositions">
        /// if set to <c>true</c> [term positions].
        /// </param>
        /// <param name="termOffsets">
        /// if set to <c>true</c> [term offsets].
        /// </param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="isDynamic">
        /// if set to <c>true</c> [is dynamic].
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Xml.Linq.XElement" />.
        /// </returns>
        protected XElement CreateField(
            string name,
            string type,
            bool required,
            bool indexed,
            bool stored,
            bool multiValued,
            bool omitNorms,
            bool termVectors,
            bool termPositions,
            bool termOffsets,
            string defaultValue = null,
            bool isDynamic = false)
        {
            if (!this.TypeExists(type))
                return (XElement)null;
            XElement xelement = new XElement((XName)(isDynamic ? "add-dynamic-field" : "add-field"));
            xelement.Add((object)new XElement((XName)nameof(name), (object)name));
            xelement.Add((object)new XElement((XName)nameof(type), (object)type));
            xelement.Add((object)new XElement((XName)nameof(indexed), (object)indexed.ToString().ToLowerInvariant()));
            xelement.Add((object)new XElement((XName)nameof(stored), (object)stored.ToString().ToLowerInvariant()));
            if (required)
                xelement.Add((object)new XElement((XName)nameof(required), (object)true));
            if (multiValued)
                xelement.Add((object)new XElement((XName)nameof(multiValued), (object)true));
            if (omitNorms)
                xelement.Add((object)new XElement((XName)nameof(omitNorms), (object)true));
            if (termVectors)
                xelement.Add((object)new XElement((XName)nameof(termVectors), (object)true));
            if (termPositions)
                xelement.Add((object)new XElement((XName)nameof(termPositions), (object)true));
            if (termOffsets)
                xelement.Add((object)new XElement((XName)nameof(termOffsets), (object)true));
            if (!string.IsNullOrEmpty(defaultValue))
                xelement.Add((object)new XElement((XName)"default", (object)defaultValue));
            return xelement;
        }

        protected XElement CreateFieldType(
            string name,
            string @class,
            IDictionary<string, string> properties)
        {
            XElement xelement = new XElement((XName)(this.TypeExists(name) ? "replace-field-type" : "add-field-type"));
            xelement.Add((object)new XElement((XName)nameof(name), (object)name));
            xelement.Add((object)new XElement((XName)nameof(@class), (object)@class));
            foreach (KeyValuePair<string, string> property in (IEnumerable<KeyValuePair<string, string>>)properties)
                xelement.Add((object)new XElement((XName)property.Key, (object)property.Value));
            return xelement;
        }

        /// <summary>
        /// Populate a single element that represent a solr field or solr dynamic to be remove from solr schema
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isDynamicField">Is dynamic field.</param>
        /// <returns>List of fields.</returns>
        private static XElement GetRemoveField(string name, bool isDynamicField = false)
        {
            Assert.ArgumentNotNull((object)name, nameof(name));
            XElement xelement = new XElement((XName)(isDynamicField ? "delete-dynamic-field" : "delete-field"));
            xelement.Add((object)new XElement((XName)nameof(name), (object)name));
            return xelement;
        }

        /// <summary>
        /// Populate a single element that represent a solr copy field to be remove from solr schema
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>The list.</returns>
        private static XElement GetRemoveCopyField(string source, string destination)
        {
            Assert.ArgumentNotNull((object)source, nameof(source));
            Assert.ArgumentNotNull((object)destination, nameof(destination));
            XElement xelement = new XElement((XName)"delete-copy-field");
            xelement.Add((object)new XElement((XName)nameof(source), (object)source));
            xelement.Add((object)new XElement((XName)"dest", (object)destination));
            return xelement;
        }

        /// <summary>
        /// Enumerates the list of elements that represent all solr fields to be deleted from solr schema
        /// </summary>
        /// <returns>List of remove fields.</returns>
        private IEnumerable<XElement> GetRemoveFields()
        {
            foreach (SolrNet.Schema.SolrCopyField solrCopyField in this.solrSchema.SolrCopyFields)
                yield return SchemaPopulateHelper.GetRemoveCopyField(
                    solrCopyField.Source,
                    solrCopyField.Destination);
            foreach (SolrDynamicField solrDynamicField in this.solrSchema.SolrDynamicFields)
                yield return SchemaPopulateHelper.GetRemoveField(solrDynamicField.Name, true);
            foreach (SolrField solrField in this.solrSchema.SolrFields)
                yield return SchemaPopulateHelper.GetRemoveField(solrField.Name);
        }

        private IEnumerable<XElement> GetAddFieldTypes()
        {
            SchemaPopulateHelper schemaPopulateHelper = this;
            yield return schemaPopulateHelper.CreateFieldType(
                "random",
                "solr.RandomSortField",
                (IDictionary<string, string>)new Dictionary<string, string>()
                {
                    {
                        "indexed",
                        "true"
                    }
                });
            yield return schemaPopulateHelper.CreateFieldType(
                "ignored",
                "solr.StrField",
                (IDictionary<string, string>)new Dictionary<string, string>()
                {
                    {
                        "indexed",
                        "false"
                    },
                    {
                        "stored",
                        "false"
                    },
                    {
                        "docValues",
                        "false"
                    },
                    {
                        "multiValued",
                        "true"
                    }
                });
        }

        /// <summary>
        /// Enumerates the list of element that represent all the solr fields for adding them into solr schema
        /// </summary>
        /// <returns>List of add fields.</returns>
        private IEnumerable<XElement> GetAddFields()
        {
            yield return this.CreateField(
                "_content",
                "text_general",
                false,
                true,
                false,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField(
                "_database",
                "lowercase",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField("_path", "string", false, true, true, true, false, false, false, false);
            yield return this.CreateField("_uniqueid", "string", true, true, true, false, false, false, false, false);
            yield return this.CreateField(
                "_datasource",
                "lowercase",
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField("_parent", "string", false, true, true, false, false, false, false, false);
            yield return this.CreateField(
                "_name",
                "text_general",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField(
                "_displayname",
                "text_general",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField("_language", "string", false, true, true, false, false, false, false, false);
            yield return this.CreateField(
                "_creator",
                "lowercase",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField("_editor", "lowercase", false, true, true, false, false, false, false, false);
            yield return this.CreateField("_created", "pdate", false, true, true, false, false, false, false, false);
            yield return this.CreateField("_updated", "pdate", false, true, true, false, false, false, false, false);
            yield return this.CreateField("_hidden", "boolean", false, true, false, false, false, false, false, false);
            yield return this.CreateField(
                "_template",
                "lowercase",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField(
                "_templatename",
                "lowercase",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField("_templates", "string", false, true, true, true, false, false, false, false);
            yield return this.CreateField("_icon", "lowercase", false, true, true, false, false, false, false, false);
            yield return this.CreateField("_links", "lowercase", false, true, true, true, false, false, false, false);
            yield return this.CreateField("_tags", "lowercase", false, true, true, true, false, false, false, false);
            yield return this.CreateField("_group", "string", false, true, true, false, false, false, false, false);
            yield return this.CreateField(
                "_indexname",
                "lowercase",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField(
                "_latestversion",
                "boolean",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField(
                "_indextimestamp",
                "pdate",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                "NOW");
            yield return this.CreateField(
                "_fullpath",
                "lowercase",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField("_isclone", "boolean", false, true, true, false, false, false, false, false);
            yield return this.CreateField("_version", "string", false, true, true, false, false, false, false, false);
            yield return this.CreateField("_hash", "string", false, true, true, false, false, false, false, false);
            yield return this.CreateField("__semantics", "string", false, true, true, true, false, false, false, false);
            yield return this.CreateField(
                "__boost",
                "pfloat",
                false,
                true,
                true,
                false,
                true,
                false,
                false,
                false,
                "0");
            yield return this.CreateReadAccessField();
            yield return this.CreateField("lock", "boolean", false, true, false, false, false, false, false, false);
            yield return this.CreateField(
                "__bucketable",
                "boolean",
                false,
                true,
                false,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField(
                "__workflow_state",
                "string",
                false,
                true,
                false,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField(
                "__is_bucket",
                "boolean",
                false,
                true,
                false,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField(
                "is_displayed_in_search_results",
                "boolean",
                false,
                true,
                false,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField("text", "text_general", false, true, false, true, false, false, false, false);
            yield return this.CreateField(
                "text_rev",
                "text_general_rev",
                false,
                true,
                false,
                true,
                false,
                false,
                false,
                false);
            yield return this.CreateField(
                "alphaNameSort",
                "alphaOnlySort",
                false,
                true,
                false,
                false,
                false,
                false,
                false,
                false);
            yield return this.CreateField("__hidden", "boolean", false, true, false, false, false, false, false, false);
            yield return this.CreateField("_version_", "plong", false, true, true, false, false, false, false, false);
            yield return this.CreateField(
                "*_t",
                "text_general",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_en",
                "text_en",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_ar",
                "text_ar",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_bg",
                "text_bg",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_ca",
                "text_ca",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_cs",
                "text_cz",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_da",
                "text_da",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_de",
                "text_de",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_el",
                "text_el",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_es",
                "text_es",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_eu",
                "text_eu",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_fa",
                "text_fa",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_fi",
                "text_fi",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_fr",
                "text_fr",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_ga",
                "text_ga",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_gl",
                "text_gl",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_hi",
                "text_hi",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_hu",
                "text_hu",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_hy",
                "text_hy",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_id",
                "text_id",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_it",
                "text_it",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_ja",
                "text_ja",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_lv",
                "text_lv",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_nl",
                "text_nl",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_nb",
                "text_no",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_pt",
                "text_pt",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_ro",
                "text_ro",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_ru",
                "text_ru",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_sv",
                "text_sv",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_th",
                "text_th",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_t_tr",
                "text_tr",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_i",
                "pint",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_s",
                "string",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_sm",
                "string",
                false,
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_ls",
                "lowercase",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_lsm",
                "lowercase",
                false,
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_im",
                "pint",
                false,
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_txm",
                "text_general",
                false,
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_b",
                "boolean",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_dt",
                "pdate",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_p",
                "location",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_ti",
                "pint",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_tl",
                "plong",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_tf",
                "pfloat",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_td",
                "pdouble",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_tdt",
                "pdate",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_tdtm",
                "pdate",
                false,
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_pi",
                "pint",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_c",
                "currency",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_ignored",
                "ignored",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_random",
                "random",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateField(
                "*_rpt",
                "location_rpt",
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
            yield return this.CreateDynamicFieldWithFallbackFieldType("*_t_zh", "text_zh");
            yield return this.CreateDynamicFieldWithFallbackFieldType("*_t_pl", "text_pl");
        }

        private XElement CreateReadAccessField()
        {
            XElement field = this.CreateField(
                "_readaccess",
                "lowercase",
                false,
                true,
                false,
                true,
                false,
                false,
                false,
                false);
            field.Add((object)new XElement((XName)"docValues", (object)false));
            return field;
        }

        private XElement CreateDynamicFieldWithFallbackFieldType(
            string fieldName,
            string fieldType,
            string fallbackFieldType = "text_general")
        {
            Assert.ArgumentNotNull((object)fieldName, nameof(fieldName));
            Assert.ArgumentNotNull((object)fieldType, nameof(fieldType));
            string type = this.TypeExists(fieldType) ? fieldType : fallbackFieldType;
            return this.CreateField(
                fieldName,
                type,
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                isDynamic: true);
        }

        /// <summary>
        /// Create a list of element that represent all solr field to be replaced from solr schema
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Collections.IEnumerable" />.
        /// </returns>
        private IEnumerable<XElement> GetReplaceFields()
        {
            yield return this.GetReplaceTextGeneralFieldType();
        }

        private XElement GetReplaceTextGeneralFieldType()
        {
            SolrFieldType solrFieldType =
                this.solrSchema.SolrFieldTypes.Find((Predicate<SolrFieldType>)(f => f.Name == "text_general"));
            if (solrFieldType == null)
                return (XElement)null;
            XElement xelement1 = new XElement((XName)"replace-field-type");
            xelement1.Add((object)new XElement((XName)"name", (object)solrFieldType.Name));
            xelement1.Add((object)new XElement((XName)"class", (object)solrFieldType.Type));
            xelement1.Add((object)new XElement((XName)"positionIncrementGap", (object)"100"));
            xelement1.Add((object)new XElement((XName)"multiValued", (object)"false"));
            XElement xelement2 = new XElement((XName)"indexAnalyzer");
            xelement2.Add(
                (object)new XElement(
                    (XName)"tokenizer",
                    (object)new XElement((XName)"class", (object)"solr.StandardTokenizerFactory")));
            xelement2.Add(
                (object)new XElement(
                    (XName)"filters",
                    new object[3]
                    {
                        (object)new XElement((XName)"class", (object)"solr.StopFilterFactory"),
                        (object)new XElement((XName)"ignoreCase", (object)"true"),
                        (object)new XElement((XName)"words", (object)"stopwords.txt")
                    }));
            xelement2.Add(
                (object)new XElement(
                    (XName)"filters",
                    (object)new XElement((XName)"class", (object)"solr.LowerCaseFilterFactory")));
            xelement1.Add((object)xelement2);
            XElement xelement3 = new XElement((XName)"queryAnalyzer");
            xelement3.Add(
                (object)new XElement(
                    (XName)"tokenizer",
                    (object)new XElement((XName)"class", (object)"solr.StandardTokenizerFactory")));
            xelement3.Add(
                (object)new XElement(
                    (XName)"filters",
                    new object[3]
                    {
                        (object)new XElement((XName)"class", (object)"solr.StopFilterFactory"),
                        (object)new XElement((XName)"ignoreCase", (object)"true"),
                        (object)new XElement((XName)"words", (object)"stopwords.txt")
                    }));
            /*xelement3.Add(
                (object)new XElement(
                    (XName)"filters",
                    new object[4]
                    {
                        (object)new XElement((XName)"class", (object)"solr.SynonymFilterFactory"),
                        (object)new XElement((XName)"synonyms", (object)"synonyms.txt"),
                        (object)new XElement((XName)"ignoreCase", (object)"true"),
                        (object)new XElement((XName)"expand", (object)"true")
                    }));*/
            
            // Remove hardcoded synonyms from file& and filters. Added custom pipeline instead. 
            xelement3.Add(new XElement("filters",
                new XElement("class", "solr.ManagedSynonymGraphFilterFactory"),
                new XElement("managed", "english")));

            xelement3.Add(
                (object)new XElement(
                    (XName)"filters",
                    (object)new XElement((XName)"class", (object)"solr.LowerCaseFilterFactory")));
            xelement1.Add((object)xelement3);
            return xelement1;
        }
    }
}