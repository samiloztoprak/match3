
public class TapCommand : ICommand
{
    private readonly int _x, _y;
    public (int x, int y) SourcePosition => (_x, _y);
    
    public TapCommand(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public void Execute()
    {
        
    }
}
