using Sitecore.DependencyInjection;
using Sitecore.Feature.ManagedSynonyms.Services;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Feature.ManagedSynonyms.Commands
{
    public class SyncSynonyms : Command
    {
        public override void Execute(CommandContext context)
        {
            Context.ClientPage.Start(this, nameof(DialogProcessor));
        }
        public void DialogProcessor(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                SheerResponse.YesNoCancel("Are you sure you want to sync synonyms?", "500px", "200px", true);
                args.WaitForPostBack(true);
            }
            else
            {
                if (args.Result == "yes")
                {
                    ProgressBox.Execute("Sync Synonyms", "Sync Synonyms", Sync);
                    SheerResponse.Alert("Sync has been completed");
                }
            }
        }
        private void Sync(object[] parameters)
        {
            var service = (SymmetricSynonymsService)ServiceLocator.ServiceProvider.GetService(typeof(SymmetricSynonymsService));
            service?.Sync();
        }
    }
}