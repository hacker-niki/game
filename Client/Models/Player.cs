namespace Client.Models;

public class Player
{
    public string Id { get; set; } // Уникальный идентификатор игрока (например, ConnectionId из SignalR)
    public string Name { get; set; } // Имя игрока
    public string Symbol { get; set; } // Символ игрока (например, "X", "O", "△", "□")
}