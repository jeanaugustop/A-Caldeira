using System;
using UnityEngine;

namespace ACaldeira.Events
{
    [CreateAssetMenu(menuName = "A Caldeira/Events/Void Channel", fileName = "Event_Void_")]
    public sealed class VoidEventChannelSO : ScriptableObject
    {
        public event Action Raised;

        public void Raise()
        {
            Raised?.Invoke();
        }
    }
}
