using System;

namespace TTA
{
    public static class RuntimeIds
    {
        public const int InvalidId = 0;
        public const int InvalidIndex = -1;
        public const int FirstValidId = 1;
    }

    [Serializable]
    public sealed class RuntimeIdCounters
    {
        public int nextElementId = RuntimeIds.FirstValidId;
        public int nextAreaId = RuntimeIds.FirstValidId;
        public int nextSlotId = RuntimeIds.FirstValidId;
        public int nextPlayerId = RuntimeIds.FirstValidId;
    }
}
