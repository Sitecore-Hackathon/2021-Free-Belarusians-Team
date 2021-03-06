# Synonyms Managed Module

The [Managed Synonyms Package](/SitecorePackage) contains:

* .NET binary (module implementation)
* Sitecore.Feature.ManagedSynonyms.config(DI, settings, command, processor patch)

There is the custom `PopulateHelperFactory` schema proceccor applied instead of the default one - `DefaultPopulateHelperFactory` 

![Patch ](/docs/images/schema.png)

This processor extends the `Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema` to add a factory is responsable to support the synonyms.

After the installation of the package the schema (for a target index) should be populated - this is the main requirement.

Next steps of this process:

1. Create a Synonym group item under */sitecore/system/Modules/**ManagedSynonyms***


2. Trigger the action in the custom **ManagedSynonyms** Ribbon. It synchronizes the synonym group items with target solr core

    The main procces is happening in `Sitecore.Feature.ManagedSynonyms.Services.SymmetricSynonymsService`:

    ![Patch ](/docs/images/sync.png)

