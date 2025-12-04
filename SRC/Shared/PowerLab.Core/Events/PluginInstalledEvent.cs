using PowerLab.Core.Models;
using Prism.Events;

namespace PowerLab.Core.Events;

public class PluginInstalledEvent : PubSubEvent<PluginRegistry>
{
}
