using Events;
using Helpers;
using Serilog;

namespace Monolith;

public class RandomPlayer : IPlayer
{
    private const string PlayerId = "Mr. Random";

    public PlayerMovedEvent MakeMove(GameStartedEvent e)
    {
        using var activity = Monitoring.ActivitySource.StartActivity();
        
        var random = new Random();
        var next = random.Next(3);
        var move = next switch
        {
            0 => Move.Rock,
            1 => Move.Paper,
            _ => Move.Scissor
        };
        
        Log.Logger.Debug("Player {PlayerId} has decided to perform the move {Move}", PlayerId, move);
        return new PlayerMovedEvent
        {
            GameId = e.GameId,
            PlayerId = PlayerId,
            Move = move
        };
    }

    public void ReceiveResult(GameFinishedEvent e)
    {
        using var activity = Monitoring.ActivitySource.StartActivity();
    }

    public string GetPlayerId()
    {
        return PlayerId;
    }
}

