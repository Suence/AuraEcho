using AuraEcho.ExternalTools.Models;
using System.Collections.Generic;

namespace AuraEcho.ExternalTools.Contracts;

public interface IExternalToolsRepository
{
    List<ExternalTool> GetExternalTools();
    ExternalTool GetExternalToolById(string id);

    void DeleteExternalTool(string id);
    
    void UpdateExternalTool(ExternalTool externalTool);
    
    void AddExternalTool(ExternalTool externalTool);
}
