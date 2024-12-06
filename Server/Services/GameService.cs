using Server.Models;

namespace Server.Services;

public class GameService : IGameService
{
    private readonly Dictionary<string, Game> _games = new();
    private readonly string[] _symbols = ["X", "O", "△", "□", "◆", "◇", "▲", "○", "●"]; // Доступные символы

    public Game CreateGame(int boardSize, string playerId, string? playerName, int playerCount = 2)
    {
        var game = new Game(boardSize, playerCount)
        {
            Id = Guid.NewGuid().ToString()
        };
        var player = new Player { Id = playerId, Name = playerName, Symbol = _symbols[0] };
        game.Players.Add(player);
        game.CurrentPlayerTurn = player;
        _games[game.Id] = game;
        return game;
    }

    public bool JoinGame(string gameId, string playerId, string playerName)
    {
        if (_games.TryGetValue(gameId, out var game) && game.PlayersCount > game.Players.Count)
        {
            var nextSymbol = _symbols[game.Players.Count];
            var player = new Player { Id = playerId, Name = playerName, Symbol = nextSymbol };
            game.Players.Add(player);

            if (game.Players.Count == game.PlayersCount)
            {
                game.GameStatus = 1; // игра начата
            }

            return true;
        }

        return false;
    }

    public Game? ExitGame(string gameId, string playerId)
    {
        if (!_games.TryGetValue(gameId, out var game)) return null;
        switch (game.GameStatus)
        {
            case 0:
            {
                var t = game.Players.FindIndex(p => p.Id == playerId);
                if (t >= 0)
                {
                    game.Players.RemoveAt(t);
                }

                break;
            }
            case 1:
                DropGame(gameId);
                break;
            default:
            {
                var t = game.Players.FindIndex(p => p.Id == playerId);
                if (t >= 0)
                {
                    game.Players.RemoveAt(t);
                }

                if (game.Players.Count == 0)
                {
                    DropGame(gameId);
                }

                break;
            }
        }

        return game;

    }

    public bool MakeMove(string gameId, string playerId, int x, int y)
    {
        if (_games.TryGetValue(gameId, out var game) && game.GameStatus == 1)
        {
            var currentPlayer = game.Players.FirstOrDefault(p => p.Id == playerId);
            if (currentPlayer == null || game.CurrentPlayerTurn != currentPlayer) return false;
            game.Board[x, y] = currentPlayer.Symbol;

            // Проверяем победителя
            if (CheckWinner(game, x, y, currentPlayer.Symbol))
            {
                game.GameStatus = 2;
                game.Winner = currentPlayer;
            }
            else
            {
                // Проверка на ничью
                bool isDraw = true;
                foreach (var cell in game.Board)
                {
                    if (string.IsNullOrEmpty(cell))
                    {
                        isDraw = false;
                        break;
                    }
                }

                if (isDraw)
                {
                    game.GameStatus = 2;
                    game.Winner = null;
                }
                else
                {
                    // Передаем ход следующему игроку
                    var currentIndex = game.Players.FindIndex(p => p == game.CurrentPlayerTurn);
                    var nextIndex = (currentIndex + 1) % game.Players.Count;
                    game.CurrentPlayerTurn = game.Players[nextIndex];
                }
            }

            return true;
        }

        return false;
    }

    public Game? GetGame(string gameId)
    {
        return _games.GetValueOrDefault(gameId);
    }

    private bool CheckWinner(Game game, int x, int y, string symbol)
    {
        int size = game.BoardSize;
        string[,] board = game.Board;

        // Проверяем строку
        bool rowWin = true;
        for (int i = 0; i < size; i++)
        {
            if (board[x, i] != symbol)
            {
                rowWin = false;
                break;
            }
        }

        // Проверяем столбец
        bool colWin = true;
        for (int i = 0; i < size; i++)
        {
            if (board[i, y] != symbol)
            {
                colWin = false;
                break;
            }
        }

        // Проверяем главную диагональ
        bool mainDiagWin = true;
        if (x == y)
        {
            for (int i = 0; i < size; i++)
            {
                if (board[i, i] != symbol)
                {
                    mainDiagWin = false;
                    break;
                }
            }
        }
        else
        {
            mainDiagWin = false;
        }

        // Проверяем побочную диагональ
        bool antiDiagWin = true;
        if (x + y == size - 1)
        {
            for (int i = 0; i < size; i++)
            {
                if (board[i, size - 1 - i] != symbol)
                {
                    antiDiagWin = false;
                    break;
                }
            }
        }
        else
        {
            antiDiagWin = false;
        }

        return rowWin || colWin || mainDiagWin || antiDiagWin;
    }

    public bool DropGame(string gameId)
    {
        if (_games.Remove(gameId))
        {
            return true;
        }

        return false;
    }

    public Game[] GetAllGames()
    {
        return _games.Values.ToArray();
    }

    public Player[] GetGamePlayers(string gameId)
    {
        return _games.TryGetValue(gameId, out var game) ? game.Players.ToArray() : [];
    }

    public int GetState(string gameId)
    {
        if (_games.TryGetValue(gameId, out var game))
        {
            return game.GameStatus;
        }

        return -1;
    }
}