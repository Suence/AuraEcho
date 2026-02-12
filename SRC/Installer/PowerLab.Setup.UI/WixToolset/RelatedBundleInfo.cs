namespace PowerLab.Setup.UI.WixToolset;

public class RelatedBundleInfo
{
    public Version Version { get; set; } = new Version();
    public Dictionary<string, bool> FeatureStatus { get; set; } = [];
    public string InstallationFolder { get; set; }
}
