using System.Diagnostics;
using Events;
using Helpers;
using Serilog;

namespace Monolith;

public class Game
{
    private readonly Dictionary<Guid, GameModel?> _games = new();

    public GameStartedEvent Start()
    {
        using var activity = Monitoring.ActivitySource.StartActivity();

        Guid gameId = Guid.NewGuid();
        Log.Logger.Debug("Starting new game with id {Id}", gameId);
        _games.Add(gameId, new GameModel {GameId = gameId});
        
        var startEvent = new GameStartedEvent { GameId = gameId };
        Log.Logger.Debug("Started new game with id {Id}", gameId);
        return startEvent;
    }

    public string DeclareWinner(KeyValuePair<string, Move> p1, KeyValuePair<string, Move> p2)
    {
        using var activity = Monitoring.ActivitySource.StartActivity();
        
        Log.Logger.Debug("Both players made a move - determining winner");
        string? winner = null;

        switch (p1.Value)
        {
            case Move.Rock:
                winner = p2.Value switch
                {
                    Move.Paper => p2.Key,
                    Move.Scissor => p1.Key,
                    _ => winner
                };
                break;
            case Move.Paper:
                winner = p2.Value switch
                {
                    Move.Rock => p1.Key,
                    Move.Scissor => p2.Key,
                    _ => winner
                };
                break;
            case Move.Scissor:
                winner = p2.Value switch
                {
                    Move.Rock => p2.Key,
                    Move.Paper => p1.Key,
                    _ => winner
                };
                break;
        }

        winner ??= "Tie";
        Log.Logger.Debug("The game winner is {Winner}", winner);
        return winner;
    }

    public GameFinishedEvent? ReceivePlayerEvent(PlayerMovedEvent e)
    {
        using var activity = Monitoring.ActivitySource.StartActivity();
        
        _games.TryGetValue(e.GameId, out var game);
        if (game == null)
        {
            Log.Logger.Warning("Received event from {PlayerId} for unexisting game {GameId}", e.PlayerId, e.GameId);
            return null;
        }
        
        lock (game)
        {
            game.Moves.Add(e.PlayerId, e.Move);
            Log.Logger.Debug("Registered move from player {PlayerId} for game {GameId}", e.PlayerId, e.GameId);
            if (game.Moves.Values.Count == 2)
            {
                KeyValuePair<string?, Move> p1 = game.Moves.First()!;
                KeyValuePair<string?, Move> p2 = game.Moves.Skip(1).First()!;

                var finishedEvent = PrepareWinnerAnnouncement(game, p1, p2);
                _games.Remove(game.GameId);
                return finishedEvent;
            }
        }

        return null;
    }

    public GameFinishedEvent PrepareWinnerAnnouncement(GameModel game, KeyValuePair<string?, Move> p1, KeyValuePair<string?, Move> p2)
    {
        using var activity = Monitoring.ActivitySource.StartActivity();
        
        var finishedEvent = new GameFinishedEvent
        {
            GameId = game.GameId,
            Moves = game.Moves,
            WinnerId = DeclareWinner(p1!, p2!)
        };
        return finishedEvent;
    }
}

public class GameModel
{
    public Guid GameId { get; set; }
    public Dictionary<string, Move> Moves { get; set; } = new();
}