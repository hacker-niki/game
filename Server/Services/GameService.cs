using Server.Models;

namespace Server.Services;

public class GameService : IGameService
{
    private readonly Dictionary<string, Game> _games = new();
    private readonly string[] _symbols = { "X", "O", "△", "□", "◆", "◇", "▲", "○", "●" }; // Доступные символы

    public Game CreateGame(int boardSize, string playerId, string playerName)
    {
        var game = new Game(boardSize)
        {
            Id = Guid.NewGuid().ToString()
        };
        var player = new Player { Id = playerId, Name = playerName, Symbol = _symbols[0] };
        game.Players.Add(player);
        game.CurrentTurn = player.Symbol;
        _games[game.Id] = game;
        return game;
    }

    public bool JoinGame(string gameId, string playerId, string playerName)
    {
        if (_games.TryGetValue(gameId, out var game) && game.Players.Count < _symbols.Length)
        {
            var nextSymbol = _symbols[game.Players.Count];
            var player = new Player { Id = playerId, Name = playerName, Symbol = nextSymbol };
            game.Players.Add(player);
            return true;
        }
        return false;
    }

    public bool MakeMove(string gameId, string playerId, int x, int y)
    {
        if (_games.TryGetValue(gameId, out var game) && !game.IsGameOver)
        {
            var currentPlayer = game.Players.FirstOrDefault(p => p.Id == playerId);
            if (currentPlayer == null || game.CurrentTurn != currentPlayer.Symbol) return false;

            if (game.Board[x, y] == null)
            {
                game.Board[x, y] = currentPlayer.Symbol;

                // Проверяем победителя
                if (CheckWinner(game, x, y, currentPlayer.Symbol))
                {
                    game.IsGameOver = true;
                    game.Winner = currentPlayer.Symbol;
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
                        game.IsGameOver = true;
                        game.Winner = "Draw";
                    }
                    else
                    {
                        // Передаем ход следующему игроку
                        var currentIndex = game.Players.FindIndex(p => p.Symbol == game.CurrentTurn);
                        var nextIndex = (currentIndex + 1) % game.Players.Count;
                        game.CurrentTurn = game.Players[nextIndex].Symbol;
                    }
                }

                return true;
            }
        }
        return false;
    }

    public Game GetGame(string gameId)
    {
        return _games.TryGetValue(gameId, out var game) ? game : null;
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
}