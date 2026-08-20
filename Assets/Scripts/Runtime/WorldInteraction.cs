using System;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    public interface IWorldInteractable
    {
        string ActionLabel { get; }
        Vector3 InteractionPosition { get; }
        NeedType SatisfiesNeed { get; }
        bool IsAvailable { get; }
        void BeginInteraction(SimController actor, Action completed);
    }
}
