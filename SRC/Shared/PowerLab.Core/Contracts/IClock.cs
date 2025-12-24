namespace PowerLab.Core.Contracts;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
    void Synchronize(DateTimeOffset serverUtcTime);
}
