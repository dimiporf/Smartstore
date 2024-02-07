using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Smartstore;
using Smartstore.Core.Data;
using Smartstore.Core.DataExchange.Export;
using Smartstore.Core.Localization;
using Smartstore.Core.Widgets;
using Smartstore.Engine.Modularity;
using Smartstore.Http;
using Smartstore.Scheduling;

public class Module : ModuleBase, IConfigurable
{
    private readonly SmartDbContext _db;
    private readonly IExportProfileService _exportProfileService;
    private readonly ITaskStore _taskStore;

    public Module(SmartDbContext db, IExportProfileService exportProfileService, ITaskStore taskStore)
    {
        _db = db;
        _exportProfileService = exportProfileService;
        _taskStore = taskStore;
    }

    public Localizer T { get; set; } = NullLocalizer.Instance;
    public RouteInfo GetConfigurationRoute()
        => new("Configure", "HelloWorldAdmin", new { area = "Admin" });


    public RouteInfo GetProductRoute()
             => new("Index", "AddProductAdmin", new { area = "Admin" });



    public override async Task InstallAsync(ModuleInstallationContext context)
    {
        // Saves the default state of a settings class to the database 
        // without overwriting existing values.
        //await TrySaveSettingsAsync<HelloWorldSettings>();

        // Imports all language resources for the current module from 
        // xml files in "Localization" directory (if any found).
        await ImportLanguageResourcesAsync();

        

        // VERY IMPORTANT! Don't forget to call.
        await base.InstallAsync(context);
    }

    public override async Task UninstallAsync()
    {
        // Delete existing export profiles.
        //var profiles = await _db.ExportProfiles
        //    .Include(x => x.Deployments)
        //    .Include(x => x.Task)
        //    .Where(x => x.ProviderSystemName == HelloWorldCsvExportProvider.SystemName || x.ProviderSystemName == HelloWorldXmlExportProvider.SystemName)
        //    .ToListAsync();

        //await profiles.EachAsync(x => _exportProfileService.DeleteExportProfileAsync(x, true));

        // Deletes all "MyGreatModuleSettings" properties settings from the database.
        //await DeleteSettingsAsync<ProductImportSettings>();

        // Delete Tasks.
        //await _taskStore.TryDeleteTaskAsync<CleanupHelloWorldTask>();

        // Deletes all language resource for the current module 
        // if "ResourceRootKey" is module.json is not empty.
        await DeleteLanguageResourcesAsync();

        // VERY IMPORTANT! Don't forget to call.
        await base.UninstallAsync();
    }
}