using Prism.Mvvm;

namespace PowerLab.ExternalTools.Models;

public class ExternalTool : BindableBase
{
    #region private members
    private string _id;
    private string _name;
    private string _command;
    private string _arguments;
    private ExternalToolType _type;
    #endregion

    public required string Id 
    {
        get => _id;
        init => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Command
    {
        get => _command;
        set => SetProperty(ref _command, value);
    }

    public string Arguments
    {
        get => _arguments;
        set => SetProperty(ref _arguments, value);
    }

    public ExternalToolType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
}
