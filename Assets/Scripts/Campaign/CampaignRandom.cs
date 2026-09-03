using System;

namespace TCCGame.Gameplay.Campaign
{
    public sealed class CampaignRandom
    {
        private uint state;

        public CampaignRandom(int campaignSeed, int sequence)
        {
            if (sequence <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sequence));
            }

            uint mixed = unchecked((uint)campaignSeed) ^
                unchecked((uint)sequence * 0x9E3779B9u);
            mixed ^= mixed >> 16;
            mixed *= 0x7FEB352Du;
            mixed ^= mixed >> 15;
            mixed *= 0x846CA68Bu;
            mixed ^= mixed >> 16;
            state = mixed == 0u ? 0xA341316Cu : mixed;
        }

        public uint NextUInt()
        {
            uint value = state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            state = value;
            return value;
        }

        public int NextInt(int minimumInclusive, int maximumExclusive)
        {
            if (maximumExclusive <= minimumInclusive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumExclusive)
                );
            }

            uint range = (uint)(maximumExclusive - minimumInclusive);
            uint threshold = unchecked(0u - range) % range;
            uint value;

            do
            {
                value = NextUInt();
            }
            while (value < threshold);

            return minimumInclusive + (int)(value % range);
        }
    }
}
