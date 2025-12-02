using Prism.Regions;

namespace PowerLab.Core.Models
{
    public record NavigationHistoryEntry(string RegionName, string ViewName, NavigationParameters? Parameters);
}
