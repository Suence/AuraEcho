using AuraEcho.Core.Contracts;
using AuraEcho.Core.Data;
using AuraEcho.Core.Extensions;
using AuraEcho.Core.Models;

namespace AuraEcho.Core.Repositories;

public class LocalPluginRepository : ILocalPluginRepository
{
    private readonly AuraEchoDbContext _dbContext;
    public LocalPluginRepository(AuraEchoDbContext dbContext)
    { 
        _dbContext = dbContext;
    }

    public void AddPluginRegistry(PluginRegistryModel pluginRegistryModel)
    {
        _dbContext.PluginRegistries.Add(pluginRegistryModel.ToPluginRegistryEntity());
        _dbContext.SaveChanges();
    }

    public List<PluginRegistryModel> GetPluginRegistries()
    {
        return _dbContext.PluginRegistries.Select(PluginRegistryExtensions.ToPluginRegistryEntity).ToList();
    }

    public void RemovePluginRegistry(string pluginRegistryId)
    {
        var entity = _dbContext.PluginRegistries.FirstOrDefault(p => p.Id == pluginRegistryId);
        if (entity != null)
        {
            _dbContext.PluginRegistries.Remove(entity);
            _dbContext.SaveChanges();
        }
    }

    public void UpdatePluginRegistry(PluginRegistryModel pluginRegistryModel)
    {
        var targetEntity = _dbContext.PluginRegistries.FirstOrDefault(pr => pr.Id == pluginRegistryModel.Id);
        if (targetEntity is null)
            throw new Exception($"EntityId not found: {pluginRegistryModel.Id}");

        targetEntity.PluginFolder = pluginRegistryModel.PluginFolder;
        targetEntity.PlanStatus = pluginRegistryModel.PlanStatus;

        _dbContext.PluginRegistries.Update(targetEntity);
        _dbContext.SaveChanges();
    }
}
