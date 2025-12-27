using System;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    [Serializable]
    public class StatusEffectHandler
    {
        public int TurnCounter;
        public StatusEffectName StatusEffectName;
        public float Value;

        public StatusEffectHandler(int TurnCounter, StatusEffectName StatusEffectName, float Value)
        {
            this.TurnCounter = TurnCounter;
            this.StatusEffectName = StatusEffectName;
            this.Value = Value;
        }

        public bool CheckTurnsCounterFinished()
        {
            TurnCounter--;
            return TurnCounter <= 0 ? true : false;
        }
    }
}
