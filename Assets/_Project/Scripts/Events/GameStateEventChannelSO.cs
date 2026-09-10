using System;
using ACaldeira.Data;
using UnityEngine;

namespace ACaldeira.Events
{
    [CreateAssetMenu(menuName = "A Caldeira/Events/Game State Channel", fileName = "Event_GameState_")]
    public sealed class GameStateEventChannelSO : ScriptableObject
    {
        public event Action<GameState> Raised;

        public void Raise(GameState state)
        {
            Raised?.Invoke(state);
        }
    }
}
