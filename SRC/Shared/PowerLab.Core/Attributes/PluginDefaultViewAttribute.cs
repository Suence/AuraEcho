namespace PowerLab.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class PluginDefaultViewAttribute(string viewName) : Attribute
    {
        public string ViewName { get; } = viewName;
    }
}
