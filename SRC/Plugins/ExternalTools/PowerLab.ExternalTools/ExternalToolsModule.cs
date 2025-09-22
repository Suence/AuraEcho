using Microsoft.EntityFrameworkCore;
using PowerLab.ExternalTools.Contracts;
using PowerLab.ExternalTools.Data;
using PowerLab.ExternalTools.Repositories;
using PowerLab.ExternalTools.Themes;
using PowerLab.ExternalTools.Views;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.PluginContracts.Models;
using Prism.Ioc;
using System.IO;
using System.Linq;
using System.Windows;

namespace PowerLab.ExternalTools;

public class ExternalToolsModule : IPlugin
{
    private readonly ResourceDictionary _lightTheme = new ExternalToolsLightTheme();
    private readonly ResourceDictionary _darkTheme = new ExternalToolsDarkTheme();

    public AppSettingsItem GetSettings()
    {
        return new()
        {
            Name = "EXTERNAL TOOLS",
            ViewName = nameof(ExternalToolsSettings),
        };
    }

    public ResourceDictionary GetThemeResource(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Light => _lightTheme,
            AppTheme.Dark => _darkTheme,
            _ => null
        };
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        using var dbContext = containerProvider.Resolve<ExternalToolsDbContext>();
        dbContext.Database.Migrate();
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<ExternalToolsHome>();
        containerRegistry.RegisterForNavigation<AddExternalTool>();
        containerRegistry.RegisterForNavigation<EditExternalTool>();
        containerRegistry.RegisterForNavigation<ExternalToolsSettings>();

        containerRegistry.Register<IExternalToolsRepository, ExternalToolsRepository>();
    }

    public void Setup(IContainerProvider containerProvider)
    {
        IPathProvider pathProvider = containerProvider.Resolve<IPathProvider>();
        Directory.CreateDirectory(Path.Combine(pathProvider.DataRootPath, "ExternalTools"));

        using var dbContext = containerProvider.Resolve<ExternalToolsDbContext>();
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }
    }
}