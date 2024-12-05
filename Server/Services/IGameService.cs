using Server.Models;

namespace Server.Services;

public interface IGameService
{
    Game CreateGame(int boardSize, string playerId, string playerName);
    bool JoinGame(string gameId, string playerId, string playerName);
    bool MakeMove(string gameId, string playerId, int x, int y);
    Game GetGame(string gameId);
}