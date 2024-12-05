using Microsoft.AspNetCore.SignalR;
using Server.Services;

namespace Server.Hubs;

public class GameHub : Hub
{
    private readonly IGameService _gameService;

    public GameHub(IGameService gameService)
    {
        _gameService = gameService;
    }

    public async Task<string> CreateGame(int boardSize, string playerName)
    {
        var game = _gameService.CreateGame(boardSize, Context.ConnectionId, playerName);
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
        return game.Id;
    }

    public async Task<bool> JoinGame(string gameId, string playerName)
    {
        bool success = _gameService.JoinGame(gameId, Context.ConnectionId, playerName);
        if (success)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            var game = _gameService.GetGame(gameId);
            await Clients.Group(gameId).SendAsync("PlayerJoined", game.Players);
        }
        return success;
    }

    public async Task<bool> MakeMove(string gameId, int x, int y)
    {
        bool success = _gameService.MakeMove(gameId, Context.ConnectionId, x, y);
        if (success)
        {
            var game = _gameService.GetGame(gameId);
            await Clients.Group(gameId).SendAsync("UpdateBoard", game.Board, game.CurrentPlayerTurn);

            if (game.GameStatus == 2)
            {
                await Clients.Group(gameId).SendAsync("GameOver", game.Winner);
            }
        }
        return success;
    }
}