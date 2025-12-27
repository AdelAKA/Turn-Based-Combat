using System;

namespace GP7.Prodigy.Combat
{
    [Serializable]
    public class QuickActionHandler
    {
        public QuickActionName QuickActionName;
        public int ItemCount;

        public QuickActionHandler(QuickActionsCollection.QuickActionInfo quickActionInfo)
        {
            QuickActionName = quickActionInfo.identifier;
            ItemCount = quickActionInfo.startCount;
        }

        public bool CanConsume() => ItemCount > 0;

        public bool TryConsume()
        {
            if(ItemCount > 0)
            {
                ItemCount--;
                return true;
            }
            return false;
        }

    }
}
