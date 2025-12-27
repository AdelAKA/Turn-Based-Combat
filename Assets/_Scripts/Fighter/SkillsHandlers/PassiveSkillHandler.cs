using System;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    [Serializable]
    public class PassiveSkillHandler
    {
        public bool isInvoked;
        public PassiveSkillName PassiveSkillName;
        public TriggerCondition condition;

        public PassiveSkillHandler(Fighter fighterReference, PassiveSkillsCollection.PassiveSkillInfo skillInfo)
        {
            PassiveSkillName = skillInfo.identifier;
            condition = skillInfo.condition.CreateNew();
            condition.SubscribeToEvent(fighterReference);
        }

        public bool CheckCondition()
        {
            if (isInvoked) return false;
            bool isFulfilled = condition.CheckCondition();
            //Debug.Log($"condition is {isFulfilled}");
            return isFulfilled;
        }
    }
}