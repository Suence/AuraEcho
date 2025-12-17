using PowerLab.Core.Data.Entities;
using PowerLab.Core.Models;

namespace PowerLab.Core.Extensions;

public static class PluginRegistryExtensions
{
    public static PluginRegistryEntity ToPluginRegistryEntity(this PluginRegistryModel @this)
        => new()
        {
            PluginFolder = @this.PluginFolder,
            Id = @this.Id,
            Manifest = @this.Manifest,
            PlanStatus = @this.PlanStatus
        };
    public static PluginRegistryModel ToPluginRegistryEntity(this PluginRegistryEntity @this)
        => new()
        {
            PluginFolder = @this.PluginFolder,
            Id = @this.Id,
            Manifest = @this.Manifest,
            PlanStatus = @this.PlanStatus
        };
}
