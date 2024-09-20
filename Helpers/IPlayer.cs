using Events;

namespace Helpers;

public interface IPlayer
{
    PlayerMovedEvent MakeMove(GameStartedEvent e);
    void ReceiveResult(GameFinishedEvent e);
    string GetPlayerId();
}