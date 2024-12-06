using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Server.Models;
using Server.Services;

namespace Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly ILogger<GameHub> _logger;

        public GameHub(IGameService gameService, ILogger<GameHub> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }
        public async void CreateGame(int boardSize, string playerName, int playersCount = 2)
        {
            var game = _gameService.CreateGame(boardSize, Context.ConnectionId, playerName, playersCount);
            var json = JsonConvert.SerializeObject(game);
            await Clients.Caller.SendAsync("ReceiveCreateGame", json);
        }
        public async Task JoinGame(string gameId, string playerName)
        {
            bool success = _gameService.JoinGame(gameId, Context.ConnectionId, playerName);
            // TODO: if gameservice.JoinGame returns adding new user notify all users of game
            if (success)
            {
                var game = _gameService.GetGame(gameId);
                var json = JsonConvert.SerializeObject(game);
                foreach (var player in game?.Players ?? new())
                {
                    await Clients.Client(player.Id).SendAsync("ReceiveJoinGame", json);
                }
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
            var game = _gameService.ExitGame(gameId, Context.ConnectionId);
            if (game == null)
            {
                var error = new Error { Message = "Leave game error", RedirectHome = true };
                var json = JsonConvert.SerializeObject(error);
                await Clients.Client(Context.ConnectionId).SendAsync("Error", json);
            }
            else
            {
                var json = JsonConvert.SerializeObject(game);
                foreach (var player in game?.Players ?? new())
                {
                    await Clients.Client(player.Id).SendAsync("ReceiveLeaveGame", json);
                }
            }

        }

        public async Task GetGame(string gameId)
        {
            var game = _gameService.GetGame(gameId);
            var json = JsonConvert.SerializeObject(game);
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveGame", json);
        }
        
        public async Task MakeMove(string gameId, int x, int y)
        {
            bool success = _gameService.MakeMove(gameId, Context.ConnectionId, x, y);
            if (success)
            {
                var game = _gameService.GetGame(gameId);
                if (game != null)
                {
                    var jsonBoard = JsonConvert.SerializeObject(game.Board);
                    var jsonCurPlayer = JsonConvert.SerializeObject(game.CurrentPlayerTurn);
                    // Notify all players in the group about the updated game board and current turn
                    foreach (var player in game?.Players ?? new())
                    {
                        await Clients.Client(player.Id).SendAsync("UpdateBoard", jsonBoard, jsonCurPlayer);
                    }

                    // Check if the game has ended
                    if (game.GameStatus == 2) // Assuming 2 indicates "Game Over"
                    {
                        foreach (var player in game?.Players ?? new())
                        {
                            var jsonWinner = JsonConvert.SerializeObject(game.Winner);
                            await Clients.Client(player.Id).SendAsync("GameOver", jsonWinner);
                        }
                    }
                }
            }
            else
            {
                var error = new Error { Message = "Move error", RedirectHome = false };
                var json = JsonConvert.SerializeObject(error);
                await Clients.Client(Context.ConnectionId).SendAsync("Error", error);
            }
        }
        public async Task GetAllGames()
        {
            var games = _gameService.GetAllGames();
            var json = JsonConvert.SerializeObject(games);
            await Clients.Caller.SendAsync("ReceiveAllGames", json);

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Logic to handle player disconnection
            var games = _gameService.GetAllGames();
            foreach (var game in games)
            {
                var json = JsonConvert.SerializeObject(game);
                if (game.Players.Any(p => p.Id == Context.ConnectionId))
                {
                    _gameService.DropGame(game.Id);
                    foreach (var player in game?.Players ?? new())
                    {
                        await Clients.Client(player.Id).SendAsync("DropGame", json);
                    }
                    // await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.Id);
                    // // Optionally notify other players about the disconnection
                    // await Clients.Group(game.Id).SendAsync("PlayerDisconnected", Context.ConnectionId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}