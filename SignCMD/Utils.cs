using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace SignCommands
{
    #region Utils
    public static class ScUtils
    {
        public static bool CanCreate(TSPlayer player, ScSign sign)
        {
            if (player.Group.HasPermission(sign.requiredPermission))
                return true;
            var fails = sign.commands.Count(cmd => !cmd.Value.CanRun(player));

            return fails != sign.commands.Values.Count;
        }

        public static bool CanBreak(TSPlayer player, ScSign sign)
        {
            if (player.Group.HasPermission(sign.requiredPermission))
                return true;
            if (!player.Group.HasPermission("essentials.signs.break"))
                return false;
            return true;
        }
    }
    #endregion

    #region Extension
    public static class Extension
    {
        public static void AddItem(this Dictionary<Point, ScSign> dictionary, Point point, ScSign sign)
        {
            if (!dictionary.ContainsKey(point))
            {
                dictionary.Add(point, sign);
                return;
            }

            dictionary[point] = sign;
        }
        
        public static ScSign Check(this Dictionary<Point, ScSign> dictionary, int x, int y, string text, TSPlayer tPly)
        {
            var point = new Point(x, y);
            if (!dictionary.ContainsKey(point))
            {
                var sign = new ScSign(text, tPly, point);
                dictionary.Add(point, sign);
                return sign;
            }
            return dictionary[point];
        }

        public static string Suffix(this int number)
        {
            return number == 0 || number > 1 ? "s" : "";
        }
    }
        #endregion

    #region Player
    public class ScPlayer
    {
        public int Index { get; set; }
        public TSPlayer TsPlayer { get { return TShock.Players[Index]; } }
        public ScSign confirmSign;
        public bool DestroyMode { get; set; }
        public int AlertCooldownCooldown { get; set; }
        public int AlertPermissionCooldown { get; set; }
        public int AlertDestroyCooldown { get; set; }

        public ScPlayer(int index)
        {
            Index = index;
            DestroyMode = false;
            AlertDestroyCooldown = 0;
            AlertPermissionCooldown = 0;
            AlertCooldownCooldown = 0;
        }
    }
    #endregion

    #region Cooldown
    public class SignCommand : Command
    {
        private int _cooldown;
        public SignCommand(int coolDown, List<string> permissions, CommandDelegate cmd, params string[] names)
            : base(permissions, cmd, names)
        {
            _cooldown = coolDown;
        }
    }

    public class Cooldown
    {
        public int time;
        public ScSign sign;
        public string name;
        public string group;

        public Cooldown(int time, ScSign sign, string name, string group = null)
        {
            this.time = time;
            this.sign = sign;
            this.name = name;
            this.group = group;
        }
    }
}
    #endregion