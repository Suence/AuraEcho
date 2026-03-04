using AuraEcho.ExternalTools.Contracts;
using AuraEcho.ExternalTools.Data;
using AuraEcho.ExternalTools.Models;
using System.Collections.Generic;
using System.Linq;

namespace AuraEcho.ExternalTools.Repositories;

public class ExternalToolsRepository : IExternalToolsRepository
{
    private readonly ExternalToolsDbContext _dbContext;

    public ExternalToolsRepository(ExternalToolsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddExternalTool(ExternalTool externalTool)
    {
        _dbContext.ExternalTools.Add(externalTool);
        _dbContext.SaveChanges();
    }

    public void DeleteExternalTool(string id)
    {
        var entity = _dbContext.ExternalTools.FirstOrDefault(et => et.Id == id);

        if (entity is null) return;

        _dbContext.ExternalTools.Remove(entity);
        _dbContext.SaveChanges();
    }

    public ExternalTool GetExternalToolById(string id)
    {
        return _dbContext.ExternalTools.FirstOrDefault(et => et.Id == id);
    }

    public List<ExternalTool> GetExternalTools()
    {
        return _dbContext.ExternalTools.ToList();
    }

    public void UpdateExternalTool(ExternalTool externalTool)
    {
        var entity = _dbContext.ExternalTools.FirstOrDefault(et => et.Id == externalTool.Id);
        if (entity is null) return;
        entity.Name = externalTool.Name;
        entity.Command = externalTool.Command;
        entity.Arguments = externalTool.Arguments;
        entity.Type = externalTool.Type;
        _dbContext.SaveChanges();
    }
}
