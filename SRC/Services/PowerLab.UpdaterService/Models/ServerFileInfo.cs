namespace PowerLab.UpdaterService.Models
{
    public class ServerFileInfo
    {
        public string Name { get; set; }

        public long Size { get; set; }

        public string RelativePath { get; set; }

        public DateTime LastModified { get; set; }

        public double Progress { get; set; }
    }
}
