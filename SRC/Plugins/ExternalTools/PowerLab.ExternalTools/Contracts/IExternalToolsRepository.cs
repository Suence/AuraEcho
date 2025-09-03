using System.Collections.Generic;
using PowerLab.ExternalTools.Models;

namespace PowerLab.ExternalTools.Contracts
{
    public interface IExternalToolsRepository
    {
        List<ExternalTool> GetExternalTools();
        ExternalTool GetExternalToolById(string id);

        void DeleteExternalTool(string id);
        
        void UpdateExternalTool(ExternalTool externalTool);
        
        void AddExternalTool(ExternalTool externalTool);
    }
}
