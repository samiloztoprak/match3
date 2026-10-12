
public class PendingCommandSlot
{
    private ICommand _pendingCommand;

    public void Submit(ICommand command)
    {
        _pendingCommand = command;
    }

    public bool TryTake(out ICommand command)
    {
        if(_pendingCommand != null)
        {
            command = _pendingCommand;
            _pendingCommand = null;
            return true;
        }
        command = null;
        return false;
    }
}
