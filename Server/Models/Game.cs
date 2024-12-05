namespace Server.Models;

public class Game
{
    public string Id { get; set; } // Уникальный идентификатор игры
    public List<Player> Players { get; set; } // Список игроков
    public string[,] Board { get; set; } // Игровое поле (например, 5x5 или любое другое)
    public int BoardSize { get; set; } // Размер игрового поля (например, 5 для 5x5)
    public string CurrentTurn { get; set; } // Символ текущего игрока
    public bool IsGameOver { get; set; } // Флаг окончания игры
    public string Winner { get; set; } // Победитель (символ победителя или "Draw")

    public Game(int boardSize)
    {
        BoardSize = boardSize;
        Board = new string[boardSize, boardSize];
        Players = new List<Player>();
        CurrentTurn = null;
        IsGameOver = false;
        Winner = null;
    }
}