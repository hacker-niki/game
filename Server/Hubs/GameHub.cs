using Microsoft.AspNetCore.SignalR;
using Server.Models;
using Server.Services;

namespace Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;

        public GameHub(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Creates a new game and adds the creator to a SignalR group with the game ID.
        /// </summary>
        /// <param name="boardSize">The size of the game board.</param>
        /// <param name="playerName">The name of the player creating the game.</param>
        /// <returns>The ID of the created game.</returns>
        public async Task<string> CreateGame(int boardSize, string playerName)
        {
            var game = _gameService.CreateGame(boardSize, Context.ConnectionId, playerName);
            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
            return game.Id;
        }

        /// <summary>
        /// Allows a player to join an existing game and notifies all players in the game.
        /// </summary>
        /// <param name="gameId">The ID of the game to join.</param>
        /// <param name="playerName">The name of the player joining the game.</param>
        /// <returns>True if the player successfully joined the game, otherwise false.</returns>
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

        /// <summary>
        /// Processes a player's move and updates the game state for all players.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="x">The x-coordinate of the move.</param>
        /// <param name="y">The y-coordinate of the move.</param>
        /// <returns>True if the move was valid and successful, otherwise false.</returns>
        public async Task<bool> MakeMove(string gameId, int x, int y)
        {
            bool success = _gameService.MakeMove(gameId, Context.ConnectionId, x, y);
            if (success)
            {
                var game = _gameService.GetGame(gameId);
                if (game != null)
                {
                    // Notify all players in the group about the updated game board and current turn
                    await Clients.Group(gameId).SendAsync("UpdateBoard", game.Board, game.CurrentPlayerTurn);

                    // Check if the game has ended
                    if (game.GameStatus == 2) // Assuming 2 indicates "Game Over"
                    {
                        await Clients.Group(gameId).SendAsync("GameOver", game.Winner);
                    }
                }
            }
            return success;
        }

        /// <summary>
        /// Retrieves the state of a game by its ID.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <returns>The game object, or null if not found.</returns>
        public Game? GetGame(string gameId)
        {
            return _gameService.GetGame(gameId);
        }

        /// <summary>
        /// Removes a player from a game group when they disconnect.
        /// </summary>
        /// <returns>An asynchronous task.</returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Logic to handle player disconnection
            var games = _gameService.GetAllGames();
            foreach (var game in games)
            {
                if (game.Players.Any(p => p.Id == Context.ConnectionId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.Id);
                    // Optionally notify other players about the disconnection
                    await Clients.Group(game.Id).SendAsync("PlayerDisconnected", Context.ConnectionId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}