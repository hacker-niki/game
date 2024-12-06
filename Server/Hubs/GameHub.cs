using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Server.Models;
using Server.Services;

namespace Server.Hubs;

public class GameHub(IGameService gameService) : Hub
{
    public async void CreateGame(int boardSize, string playerName, int playersCount = 2)
    {
        var game = gameService.CreateGame(boardSize, Context.ConnectionId, playerName, playersCount);
        var json = JsonConvert.SerializeObject(game);

        // Добавить создателя игры в группу с идентификатором игры
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);

        await Clients.Caller.SendAsync("ReceiveCreateGame", json);
    }

    public async Task JoinGame(string gameId, string playerName)
    {
        var success = gameService.JoinGame(gameId, Context.ConnectionId, playerName);

        if (success)
        {
            var game = gameService.GetGame(gameId);
            var json = JsonConvert.SerializeObject(game);

            // Добавить игрока в группу игры
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            // Уведомить всех участников группы об изменении
            await Clients.Group(gameId).SendAsync("ReceiveJoinGame", json);
        }
        else
        {
            var error = new Error { Message = "Join game error", RedirectHome = true };
            var json = JsonConvert.SerializeObject(error);
            await Clients.Client(Context.ConnectionId).SendAsync("Error", json);
        }
    }

    public async Task LeaveGame(string gameId)
    {
        var game = gameService.ExitGame(gameId, Context.ConnectionId);
        if (game == null)
        {
            var error = new Error { Message = "Leave game error", RedirectHome = true };
            var json = JsonConvert.SerializeObject(error);
            await Clients.Client(Context.ConnectionId).SendAsync("Error", json);
        }
        else
        {
            var json = JsonConvert.SerializeObject(game);

            // Удалить игрока из группы игры
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);

            // Уведомить всех участников группы об изменении
            await Clients.Group(gameId).SendAsync("ReceiveLeaveGame", json);
        }
    }

    public async Task GetGame(string gameId)
    {
        var game = gameService.GetGame(gameId);
        var json = JsonConvert.SerializeObject(game);
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveGame", json);
    }

    public async Task MakeMove(string gameId, int x, int y)
    {
        var success = gameService.MakeMove(gameId, Context.ConnectionId, x, y);
        if (success)
        {
            var game = gameService.GetGame(gameId);
            if (game != null)
            {
                var jsonGame = JsonConvert.SerializeObject(game);
                // Уведомить всех игроков в группе об изменении
                await Clients.Group(gameId).SendAsync("UpdateBoard", jsonGame);

                if (game.GameStatus == 2) // Проверка на завершение игры
                {
                    var jsonWinner = JsonConvert.SerializeObject(game.Winner);
                    await Clients.Group(gameId).SendAsync("GameOver", jsonWinner);
                }
            }
        }
        else
        {
            var error = new Error { Message = "Move error", RedirectHome = false };
            var json = JsonConvert.SerializeObject(error);
            await Clients.Client(Context.ConnectionId).SendAsync("Error", json);
        }
    }

    public async Task GetAllGames()
    {
        var games = gameService.GetAllGames();
        var json = JsonConvert.SerializeObject(games);
        await Clients.Caller.SendAsync("ReceiveAllGames", json);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var games = gameService.GetAllGames();
        foreach (var game in games)
        {
            if (game.Players.All(p => p.Id != Context.ConnectionId)) continue;
            // Удалить игрока из группы игры
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.Id);

            var json = JsonConvert.SerializeObject(game);
            gameService.DropGame(game.Id);

            // Уведомить всех участников группы об изменении
            await Clients.Group(game.Id).SendAsync("DropGame", json);
        }
        await base.OnDisconnectedAsync(exception);
    }
}