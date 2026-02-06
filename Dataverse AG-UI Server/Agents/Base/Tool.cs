namespace Dataverse_AG_UI_Server.Agents.Base;

public class Tool
{
    public string Name { get; }
    public Delegate Delegate { get; }

    public Tool(string name, Delegate @delegate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tool name cannot be null or empty", nameof(name));
        
        Name = name;
        Delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
    }
}
