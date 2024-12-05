using Server.Models;

namespace Server.Services;

public interface IGameService
{
    Game CreateGame(int boardSize, string playerId, string playerName, int playerCount = 2);
    bool JoinGame(string gameId, string playerId, string playerName);
    bool MakeMove(string gameId, string playerId, int x, int y);
    Game? GetGame(string gameId);
    bool DropGame(string gameId);
    Game[] GetAllGames();
    public int GetState(string gameId);
}