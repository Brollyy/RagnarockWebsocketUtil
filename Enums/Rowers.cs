using Newtonsoft.Json;

namespace RagnarockWebsocket.Enums
{
    /// <summary>
    /// Represents rowers on the boat.<br/>
    /// Left/right is from the perspective of player, row numbering is from the front to back.<br/>
    /// Note that enum int values here don't indicate the actual IDs of rowers in Ragnarock - if you need those for some reason, use the GetRagnarockIds extension method instead.
    /// </summary>
    [Flags]
    public enum Rowers
    {
        FirstRowLeft = 1,
        FirstRowRight = 2,
        SecondRowLeft = 4, 
        SecondRowRight = 8,
        ThirdRowLeft = 16,
        ThirdRowRight = 32,
        FourthRowLeft = 64,
        FourthRowRight = 128
    }

    /// <summary>
    /// Utils for Rowers enum - extension methods and useful combinations of flags.
    /// </summary>
    public static class RowersUtil
    {
        public const Rowers FirstRow = Rowers.FirstRowLeft | Rowers.FirstRowRight;
        public const Rowers SecondRow = Rowers.SecondRowLeft | Rowers.SecondRowRight;
        public const Rowers ThirdRow = Rowers.ThirdRowLeft | Rowers.ThirdRowRight;
        public const Rowers FourthRow = Rowers.FourthRowLeft | Rowers.FirstRowRight;
        public const Rowers LeftSide = Rowers.FirstRowLeft | Rowers.SecondRowLeft | Rowers.ThirdRowLeft | Rowers.FourthRowLeft;
        public const Rowers RightSide = Rowers.FirstRowRight | Rowers.SecondRowRight | Rowers.ThirdRowRight | Rowers.FourthRowRight;
        public const Rowers All = LeftSide | RightSide;

        /// <summary>
        /// Returns a list of Ragnarock IDs of the rowers identified by this enum value.
        /// </summary>
        /// <param name="rowers">rowers enum value - possibly multiple ones</param>
        /// <returns>list of Ragnarock IDs</returns>
        public static List<int> GetRagnarockIds(this Rowers rowers)
        {
            return rowers.ToString()
                .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(GetRagnarockId)
                .ToList();
        }

        private static int GetRagnarockId(string rowerName)
        {
            if (Enum.TryParse(rowerName, out Rowers rower))
            {
                switch (rower)
                {
                    case Rowers.FirstRowLeft: return 4;
                    case Rowers.FirstRowRight: return 0;
                    case Rowers.SecondRowLeft: return 5;
                    case Rowers.SecondRowRight: return 1;
                    case Rowers.ThirdRowLeft: return 6;
                    case Rowers.ThirdRowRight: return 2;
                    case Rowers.FourthRowLeft: return 7;
                    case Rowers.FourthRowRight: return 3;
                }
            }
            return -1; // Invalid ask - not a name of a single rower
        }
    }
}
