using System;
using Zenject;

namespace Script.Player.LobbyPlayer.Player
{
    public class LobbyPlayerController: IDisposable
    {
        private readonly SignalBus _signalBus;
        
        public LobbyPlayerController(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Dispose()
        {
            
        }
    }
}