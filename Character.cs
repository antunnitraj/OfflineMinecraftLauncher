using System.Text;
using System.Text.RegularExpressions;

namespace OfflineMinecraftLauncher
{
    internal class Character
    {
        // From 1.19.3 / 22w4x
        private static readonly string[] CharacterResourceNames1193 =
        [
            "Alex_slim", "Ari_slim", "Efe_slim", "Kai_slim", "Makena_slim", "Noor_slim", "Steve_slim", "Sunny_slim", "Zuri_slim",
            "Alex_classic", "Ari_classic", "Efe_classic", "Kai_classic", "Makena_classic", "Noor_classic", "Steve_classic", "Sunny_classic", "Zuri_classic"
        ];

        // From 1.8pre1 / 14wxx
        private static readonly string[] CharacterResourceNames18pre1 =
        [
            "Steve_classic", "Alex_slim"
        ];

        // Older
        private static readonly string[] CharacterResourceNamesOlder =
        [
            "Steve_classic"
        ];

        /*
         * Returns the character set for the given Minecraft version.
         * If the version is 1.19.3 or newer, it returns the full set of characters.
         * If the version is 1.8pre1 or newer but older than 1.19.3, it returns Steve and Alex.
         * Otherwise, it returns Steve only.
         */
        internal static string[] GetCharacterSetForVersion(string minecraftVersion)
        {
            // Splitting the version for pre-release versions and snapshots
            // We will consider a pre-release as a full release for character selection
            minecraftVersion = minecraftVersion.Split("-")[0].Split(" ")[0];
            if (string.IsNullOrWhiteSpace(minecraftVersion))
                return CharacterResourceNamesOlder;

            // Remove suffixes like -pre1, -rc1, Pre-Release 2, etc.
            var versionPart = minecraftVersion.Split('-')[0].Split(' ')[0];

            if (string.IsNullOrWhiteSpace(versionPart))
                return CharacterResourceNamesOlder;

            if (Version.TryParse(versionPart, out var version))
            {
                if (version >= new Version(1, 19, 3))
                    return CharacterResourceNames1193;
                else if (version >= new Version(1, 8))
                    return CharacterResourceNames18pre1;
            }
            else
            {
                // Try to match snapshot format, e.g. 22w45a
                var match = Regex.Match(minecraftVersion, @"^(?<yy>\d{2})w(?<ww>\d{2})[a-z]$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int year = int.Parse(match.Groups["yy"].Value) + 2000;
                    int week = int.Parse(match.Groups["ww"].Value);

                    if (year > 2022 || (year == 2022 && week >= 45))
                        return CharacterResourceNames1193;
                    else if ((year == 2022 && week < 45) || (year < 2022 && year >= 2014))
                        return CharacterResourceNames18pre1;
                }
            }

            // If the version is not recognized, return the oldest character names
            return CharacterResourceNamesOlder;
        }

        /*
         * Returns the character resource name based on the username and Minecraft version.
         */
        internal static string GetCharacterResourceNameFromUsernameAndGameVersion(string username, string minecraftVersion)
        {
            // Current username UUID
            var playerUuid = GenerateUuidFromUsername(username);
            return GetCharacterResourceNameFromUuidAndGameVersion(playerUuid, minecraftVersion);
        }

        /*
         * Returns the character resource name based on the username uuid and Minecraft version.
         */
        internal static string GetCharacterResourceNameFromUuidAndGameVersion(string playerUuid, string minecraftVersion)
        {
            // List of allowed characters for the game version
            var characterSet = GetCharacterSetForVersion(minecraftVersion);

            // If the array only contains one entry, return that entry directly
            if (characterSet.Length == 1)
                return characterSet[0];

            // If empty minecraftVersion
            if (string.IsNullOrEmpty(minecraftVersion))
                return characterSet[0];

            // If empty playerUuid
            if (string.IsNullOrEmpty(playerUuid))
                return characterSet[0];

            // UUID3 string to bytes
            var hex = playerUuid.Replace("-", "");
            var bytes = new byte[16];
            for (int i = 0; i < 16; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

            // hi and lo to big-endian
            ulong hi = ((ulong)bytes[0] << 56) | ((ulong)bytes[1] << 48) | ((ulong)bytes[2] << 40) | ((ulong)bytes[3] << 32)
                     | ((ulong)bytes[4] << 24) | ((ulong)bytes[5] << 16) | ((ulong)bytes[6] << 8) | bytes[7];
            ulong lo = ((ulong)bytes[8] << 56) | ((ulong)bytes[9] << 48) | ((ulong)bytes[10] << 40) | ((ulong)bytes[11] << 32)
                     | ((ulong)bytes[12] << 24) | ((ulong)bytes[13] << 16) | ((ulong)bytes[14] << 8) | bytes[15];

            long xor = (long)(hi ^ lo);
            int idx = (int)(xor ^ ((xor >> 32) & 0xffffffff)) % arrayLength;
            if (idx < 0) idx += characterSet.Length;

            return characterSet[idx];
        }

        /*
         * Generates a UUID for an offline player based on the username.
         * The UUID is generated using the fixed namespace "OfflinePlayer:" and the username.
         */
        internal static string GenerateUuidFromUsername(string username)
        {
            // Fallback name
            if (string.IsNullOrEmpty(username))
                username = "Player";

            // Minecraft offline player UUIDs are generated from the player name and
            // the fixed namespace "OfflinePlayer:".
            // The player name will define the UUID and thus the player Skin.
            const string ns = "OfflinePlayer:";
            return GenerateUuid3(ns, username);
        }

        /*
         * Generates a UUID3 based on the namespace and name.
         */
        internal static string GenerateUuid3(string ns, string name)
        {
            // Use ASCII encoding for both namespace and player name, no padding
            var nsBytes = Encoding.ASCII.GetBytes(ns);
            var nameBytes = Encoding.ASCII.GetBytes(name);
            var data = nsBytes.Concat(nameBytes).ToArray();

            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(data);

            hash[6] = (byte)((hash[6] & 0x0F) | 0x30); // Version 3
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // Variant RFC 4122

            // Format as UUID string in big-endian order
            return $"{hash[0]:x2}{hash[1]:x2}{hash[2]:x2}{hash[3]:x2}-" +
                   $"{hash[4]:x2}{hash[5]:x2}-" +
                   $"{hash[6]:x2}{hash[7]:x2}-" +
                   $"{hash[8]:x2}{hash[9]:x2}-" +
                   $"{hash[10]:x2}{hash[11]:x2}{hash[12]:x2}{hash[13]:x2}{hash[14]:x2}{hash[15]:x2}";
        }
    }
}
