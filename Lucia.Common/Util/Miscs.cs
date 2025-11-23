namespace Lucia.Common.Util
{
    public static class Miscs
    {
        public static int ParseIntOr(string? s, int d = 0)
        {
            if (int.TryParse(s, out var parsed))
            {
                return parsed;
            }
            return d;
        }

        public static List<int> ParseIntList(this string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<int>();

            raw = raw.Trim('[', ']');

            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => int.Parse(s.Trim()))
                      .ToList();
        }

        public static List<int> ToIntList(this IEnumerable<int?> src)
        {
            return src.Where(x => x.HasValue).Select(x => x.Value).ToList();
        }


        public static Dictionary<int, int> ParseIntDict(this string? raw)
        {
            Dictionary<int, int> dict = new();
            if (string.IsNullOrWhiteSpace(raw))
                return dict;

            raw = raw.Trim('{', '}');

            foreach (var pair in raw.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length == 2)
                    dict[int.Parse(kv[0].Trim())] = int.Parse(kv[1].Trim());
            }

            return dict;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        public static byte ToByte(this bool val)
        {
            return val ? (byte)1 : (byte)0;
        }
    }
}
