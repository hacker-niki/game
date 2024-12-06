using Client.Models;

namespace Client.Interfaces;

public interface ISignalRService
{
    public event Action<string>? ReceiveAllGames;
    public event Action<string, bool>? ReceiveError;
    public event Action<string>? ReceiveDropGame;
    public event Action<string>? ReceiveGame;
    public string ConnectionId { get; set; }

    public Task GetAllGames();
    
    public Task MakeMove(string gameId, int i, int j);

    public Task JoinGame(string gameId, string playerName);
    
    public Task GetGame(string gameId);

    public Task CreateGame(string username, int boardSize, int playersCount);
}