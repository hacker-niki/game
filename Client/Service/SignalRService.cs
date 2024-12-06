using Client.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client.Service;

public class SignalRService : ISignalRService
{
    private readonly HubConnection _hubConnection;

    public event Action<string>? ReceiveGame;
    public event Action<string>? ReceiveDropGame;
    public event Action<string>? ReceiveAllGames;
    public event Action<string, bool>? ReceiveError;

    public string ConnectionId
    {
        get => _hubConnection.ConnectionId!;
        set => throw new NotImplementedException();
    }

    public SignalRService()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://192.168.253.167:5000/gamehub")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>(
            "ReceiveAllGames",
            (games) => { ReceiveAllGames?.Invoke(games); }
        );

        _hubConnection.On<string>(
            "ReceiveCreateGame",
            (game) => { ReceiveGame?.Invoke(game); }
        );

        _hubConnection.On<string>(
            "ReceiveJoinGame",
            (game) => { ReceiveGame?.Invoke(game); }
        );

        _hubConnection.On<string>(
            "ReceiveGame",
            (game) => { ReceiveGame?.Invoke(game); }
        );
        
        _hubConnection.On<string>(
            "UpdateBoard",
            (game) => { ReceiveGame?.Invoke(game); }
        );
        
        _hubConnection.On<string>(
            "DropGame",
            (game) => { ReceiveDropGame?.Invoke(game); }
        );
        
        _hubConnection.On<string, bool>(
            "Error",
            (error, returnHome) => { ReceiveError?.Invoke(error, returnHome); }
        );

        _hubConnection.Reconnected += (connectionId) =>
        {
            Console.WriteLine($"Reconnected with connectionId: {connectionId}");
            return Task.CompletedTask;
        };

        _hubConnection.Closed += async (error) =>
        {
            Console.WriteLine($"Connection closed. Error: {error?.Message}");
            await Task.Delay(5000);
            await _hubConnection.StartAsync();
        };
    }

    public async Task GetAllGames()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
            await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("GetAllGames");
    }

    public async Task MakeMove(string gameId, int i, int j)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
            await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("MakeMove", gameId, i, j);
    }

    public async Task JoinGame(string gameId, string playerName)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
            await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("JoinGame", gameId, playerName);
    }

    public async Task GetGame(string gameId)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
            await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("GetGame", gameId);
    }

    public async Task CreateGame(string username, int boardSize, int playersCount)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
            await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("CreateGame", boardSize, username, playersCount);        
    }
}