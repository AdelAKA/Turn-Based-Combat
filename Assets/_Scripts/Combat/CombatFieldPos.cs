using System;

namespace GP7.Prodigy.Combat
{
    [Serializable]
    public struct CombatFieldPos
    {
        public int rowIndex;
        public int columnIndex;
        public bool isLocal;
    }

    public static class ComnbatFieldPosExtentions
    {
        public static string ToString(this CombatFieldPos combatPos)
        {
            return $"{combatPos.rowIndex}, {combatPos.columnIndex}, {combatPos.isLocal}";
        }
    }
}
