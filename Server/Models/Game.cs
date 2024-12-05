namespace Server.Models;

public class Game
{
    public string Id { get; init; } // Уникальный идентификатор игры
    public List<Player> Players { get; set; } // Список игроков
    public string[,] Board { get; set; } // Игровое поле (например, 5x5 или любое другое)
    public int BoardSize { get; set; } // Размер игрового поля (например, 5 для 5x5)
    public Player CurrentPlayerTurn { get; set; } // Символ текущего игрока
    public int GameStatus { get; set; } // 0 - не начата, 1 - начата, 2 - закончена 
    public Player? Winner { get; set; } // Победитель или null
    
    public int PlayersCount { get; init; }
    
    public Game(int boardSize)
    {
        Id = Guid.NewGuid().ToString();
        BoardSize = boardSize;
        Board = new string[boardSize, boardSize];
        Players = new List<Player>();
        CurrentPlayerTurn = null;
        GameStatus = 0;
        Winner = null;
    }
}