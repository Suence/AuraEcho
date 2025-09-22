using PowerLab.Core.Contracts;
using PowerLab.Core.Data;
using PowerLab.Core.Models;

namespace PowerLab.Core.Repositories;

public class PluginRepository : IPluginRepository
{
    private readonly PowerLabDbContext _dbContext;
    public PluginRepository(PowerLabDbContext dbContext) 
    { 
        _dbContext = dbContext;
    }

    public void AddPluginRegistry(PluginRegistry pluginRegistry)
    {
        _dbContext.PluginRegistries.Add(pluginRegistry);
        _dbContext.SaveChanges();
    }

    public List<PluginRegistry> GetPluginRegistries()
    {
        return _dbContext.PluginRegistries.ToList();
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

    public void UpdatePluginRegistry(PluginRegistry pluginRegistry)
    {
        _dbContext.PluginRegistries.Update(pluginRegistry);
        _dbContext.SaveChanges();
    }
}
