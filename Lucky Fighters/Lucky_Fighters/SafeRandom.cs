using System;

namespace Lucky_Fighters
{
    public static class SafeRandom
    {
        private static Random _inst = new Random(Guid.NewGuid().GetHashCode());

        public static int Next(int min, int max)
        {
            lock (_inst)
            {
                _inst = new Random(Guid.NewGuid().GetHashCode());
                return _inst.Next(min, max);
            }
        }
    }
}