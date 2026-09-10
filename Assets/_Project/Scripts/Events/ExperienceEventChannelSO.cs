using System;
using UnityEngine;

namespace ACaldeira.Events
{
    [CreateAssetMenu(menuName = "A Caldeira/Events/Experience Channel", fileName = "Event_Experience_")]
    public sealed class ExperienceEventChannelSO : ScriptableObject
    {
        public event Action<int, int> Raised;

        public void Raise(int currentExperience, int requiredExperience)
        {
            Raised?.Invoke(currentExperience, requiredExperience);
        }
    }
}
