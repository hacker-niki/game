using Client.Interfaces;

namespace Client.Service;

public class GameService: IGameService
{
    public event Action? DataLoaded;
    
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    
}