using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace SignCMD
{
    public class ScSign
    {
        public int cooldown;
        private int _cooldown;
        private string _cooldownGroup;
        private readonly List<string> _groups = new List<string>();
        private readonly List<string> _users = new List<string>();
        private readonly Dictionary<string, int> _bosses = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _mobs = new Dictionary<string, int>();
        public readonly Dictionary<List<string>, SignCommand> commands = new Dictionary<List<string>, SignCommand>(); 
        public bool freeAccess;
        public bool noEdit;
        public bool noRead;
        private bool _confirm;
        private Point _point;

        public string requiredPermission = string.Empty;

        public ScSign(string text, TSPlayer registrar, Point point)
        {
            _point = point;
            cooldown = 0;
            _cooldownGroup = string.Empty;
            RegisterCommands(text, registrar);
        }

        #region RegisterCommands

        private void RegisterCommands(string text, TSPlayer ply)
        {
            var cmdList = ParseCommands(text);

            foreach (var cmdArgs in cmdList)
            {
                var args = new List<string>(cmdArgs);
                if (args.Count < 1)
                    continue;

                var cmdName = args[0];

                /*if (cmdName == "no-perm")
                {
                    freeAccess = true;
                    continue;
                }

                if (cmdName == "confirm")
                {
                    _confirm = true;
                    continue;
                }

                if (cmdName == "no-read")
                {
                    noRead = true;
                    continue;
                }

                if (cmdName == "no-edit")
                {
                    noEdit = true;
                }

                if (cmdName == "require-perm" || cmdName == "rperm")
                {
                    requiredPermission = args[1];
                    continue;
                }

                if (cmdName == "cd" || cmdName == "cooldown")
                {
                    ParseSignCd(args);
                    continue;
                }



                if (cmdName == "allowg")
                {
                    ParseGroups(args);
                    continue;
                }

                if (cmdName == "allowu")
                {
                    ParseUsers(args);
                    continue;
                }

                if (cmdName == "spawnmob" || cmdName == "sm")
                {
                    ParseSpawnMob(args, ply);
                    continue;
                }

                if (cmdName == "spawnboss" || cmdName == "sb")
                {
                    ParseSpawnBoss(args, ply);
                    continue;
                }*/

                switch (cmdName)
				{
					case "no-perm":
                    case "np":
                    case "free":
                    case "pub":
                    case "public":
					    freeAccess = true;
						continue;

					case "confirm":
                    case "cf":
						_confirm = true;
						continue;

					case "no-read":
                    case "nr":
						noRead = true;
						continue;

					case "no-edit":
                    case "ne":
						noEdit = true;
						continue;

					case "r-perm":
                    case "rp":
						requiredPermission = args[1];
						continue;

					case "cooldown":
                    case "cd":
						ParseSignCd(args);
						continue;

					case "aw-group":
                    case "ag":
						ParseGroups(args);
						continue;

					case "aw-user":
                    case "au":
						ParseUsers(args);
						continue;

					case "spawnmob":
					case "sm":
						ParseSpawnMob(args, ply);
						continue;

					case "spawnboss":
					case "sb":
						ParseSpawnBoss(args, ply);
						continue;
				}

                IEnumerable<Command> cmds = Commands.ChatCommands.Where(c => c.HasAlias(cmdName)).ToList();

                foreach (var cmd in cmds)
                {
                    if (!CheckPermissions(ply))
                        return;

                    var sCmd = new SignCommand(cooldown, cmd.Permissions, cmd.CommandDelegate, cmdName);
                    commands.Add(args, sCmd);
                }
            }
        }

        #endregion

        #region ExecuteCommands

        public void ExecuteCommands(ScPlayer sPly)
        {
            var hasPerm = CheckPermissions(sPly.TsPlayer);
            if (!hasPerm) return;
            /*var overridePerm = CheckPermissionOverride(sPly.TsPlayer);

            if (!freeAccess && !hasPerm && !overridePerm)
            {
                if (sPly.AlertPermissionCooldown == 0)
                {
                    sPly.TsPlayer.SendErrorMessage("You do not have access to the commands on this sign");
                    sPly.AlertPermissionCooldown = 5;
                }
                return;
            }*/

            if (cooldown > 0)
            {
                if (sPly.AlertCooldownCooldown == 0)
                {
                    sPly.TsPlayer.SendErrorMessage("This sign is still cooling down. Please wait {0} more second{1}",
                        cooldown, cooldown.Suffix());
                    sPly.AlertCooldownCooldown = 2;
                }
                return;
            }

            /*if (cooldown > 0)
            {
                if (sPly.AlertCooldownCooldown == 0)
                    sPly.TsPlayer.SendErrorMessage("This sign is still cooling down. Please wait {0} more second{1}",
                        cooldown, cooldown.Suffix());

                return;
            }*/

            if (_confirm && sPly.confirmSign != this)
            {
                sPly.confirmSign = this;
                sPly.TsPlayer.SendWarningMessage("[Warning] Hit the sign again to confirm.");
                cooldown = 1;
                return;
            }

            if (_groups.Count > 0 && !_groups.Contains(sPly.TsPlayer.Group.Name))
            {
                if (sPly.AlertPermissionCooldown == 0)
                {
                    sPly.TsPlayer.SendErrorMessage("Your group does not have access to this sign"); 
                    sPly.AlertPermissionCooldown = 2;
                }
                return;
            }

            if (_users.Count > 0 && !_users.Contains(sPly.TsPlayer.UserAccountName))
            {
                if (sPly.AlertPermissionCooldown == 0)
                {
                    sPly.TsPlayer.SendErrorMessage("You do not have access to this sign");
                    sPly.AlertPermissionCooldown = 2;
                }
                return;
            }

            if (_mobs.Count > 0)
            /*{
                if (!sPly.TsPlayer.Group.HasPermission(Permissions.spawnmob))
                {
                    if (sPly.AlertPermissionCooldown == 0)
                    {
                        sPly.TsPlayer.SendErrorMessage("You do not have access to the commands on this sign");
                        sPly.AlertPermissionCooldown = 5;
                    }
                    return;
                }*/

                SpawnMobs(_mobs, sPly);
            //}

            if (_bosses.Count > 0)
            /*{
                if (!sPly.TsPlayer.Group.HasPermission(Permissions.spawnboss))
                {
                    if (sPly.AlertPermissionCooldown == 0)
                    {
                        sPly.TsPlayer.SendErrorMessage("You do not have access to the commands on this sign");
                        sPly.AlertPermissionCooldown = 5;
                    }
                    return;
                }*/
                SpawnBoss(_bosses, sPly);
            //}

            foreach (var cmdPair in commands)
            {
                //var args = new List<string>(cmdPair.Key);
                var cmd = cmdPair.Value;
                var cmdText = string.Join(" ", cmdPair.Key);
                cmdText = cmdText.Replace("{player}", sPly.TsPlayer.Name);
                //Create args straight from the command text, meaning no need to iterate through args to replace {player}
				var args = cmdText.Split(' ').ToList();

                //if (args.Any(s => s.Contains("{player}")))
                //    args[args.IndexOf("{player}")] = sPly.TsPlayer.Name;

                //while (args.Any(s => s.Contains("{player}")))
				//	args[args.IndexOf("{player}")] = sPly.TsPlayer.Name;

                if (cmd.DoLog)
                    TShock.Utils.SendLogs(string.Format("{0} executed: {1}{2} [Via sign command].", 
                            sPly.TsPlayer.Name, TShock.Config.CommandSpecifier, cmdText), Color.PaleVioletRed, sPly.TsPlayer);

                args.RemoveAt(0);

                cmd.CommandDelegate.Invoke(new CommandArgs(cmdText, sPly.TsPlayer, args));
            }

            cooldown = _cooldown;
            sPly.AlertCooldownCooldown = 1;
            sPly.confirmSign = null;
        }

        #endregion
        
        #region CheckPerm

        /*private bool CheckPermissions(TSPlayer player)
        {
            return commands.Values.All(command => command.CanRun(player));
        }*/

        //private bool CheckPermissionOverride(TSPlayer player)
		
        public bool CheckPermissions(TSPlayer player)
        {
            if (player == null) return false;

			var sPly = SignCommands.ScPlayers[player.Index];
			if (sPly == null)
			{ 
				Log.ConsoleError("An error occured while executing a sign command." + 
                    "TSPlayer {0} at index {1} does not exist as an ScPlayer", player.Name, player.Index);
				player.SendErrorMessage("An error occured. Please try again");
				return false;
			}

			if (freeAccess) return true;

			if (!string.IsNullOrEmpty(requiredPermission))
				if (player.Group.HasPermission(requiredPermission))
					return true;
				else
				{
					if (sPly.AlertPermissionCooldown == 0)
					{
					    player.SendErrorMessage("You do not have the required permission to use this sign.");
						sPly.AlertPermissionCooldown = 2;
					}
					return false;
				}

			if (commands.Values.All(command => command.CanRun(player)))
				return true;
			else
			{
				if (sPly.AlertPermissionCooldown == 0)
				{
				    player.SendErrorMessage("You do not have access to the commands on this sign.");
					sPly.AlertPermissionCooldown = 2;
				}
				return false;
			}

            //if (player.Group.HasPermission(requiredPermission))
            //    return true;
            //return false;
        }
        #endregion

        #region ParseCMD

        private IEnumerable<List<string>> ParseCommands(string text)
        {
            //Remove the Sign Command definer. It's not required
            text = text.Remove(0, SignCommands.config.DefineSignCommands.Length);

            //Replace the Sign Command command starter with the TShock one so that it gets handled properly later
            text = text.Replace(SignCommands.config.CommandsStartWith, TShock.Config.CommandSpecifier); // CommandStartWith = '>' -> CommandSpecifer: '/'

            //Remove whitespace
            text = text.Trim();

            //Create a local variable for our return value
            var ret = new List<List<string>>();

            //Split the text string at any TShock command character
            var cmdStrings = text.Split(Convert.ToChar(TShock.Config.CommandSpecifier));

            //Iterate through the strings // Iterate = 반복하다
            foreach (var str in cmdStrings)
            {
                var sbList = new List<string>();
                var sb = new StringBuilder();
                var instr = false;
                for (var i = 0; i < str.Length; i++)
                {
                    var c = str[i];

                    if (c == '\\' && ++i < str.Length)
                    {
                        if (str[i] != '"' && str[i] != ' ' && str[i] != '\\')
                            sb.Append('\\');
                        sb.Append(str[i]);
                    }
                    else if (c == '"')
                    {
                        instr = !instr;
                        if (!instr)
                        {
                            sbList.Add(sb.ToString());
                            sb.Clear();
                        }
                        else if (sb.Length > 0)
                        {
                            sbList.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                    else if (IsWhiteSpace(c) && !instr)
                    {
                        if (sb.Length > 0)
                        {
                            sbList.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                    else
                        sb.Append(c);
                }
                if (sb.Length > 0)
                    sbList.Add(sb.ToString());

                ret.Add(sbList);
            }
            return ret;
        }

        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n';
        }

        #endregion

        #region ParseSpawnMob
        private void ParseSpawnMob(IEnumerable<string> args, TSPlayer player)
        {
            //>sm "blue slime":10 zombie:100
            var list = new List<string>(args);
            list.RemoveAt(0);
            foreach (var obj in list)
            {
                try
                {
                    var mob = obj.Split(':')[0];
                    var num = obj.Split(':')[1];

                    int spawnCount;
                    if (!int.TryParse(num, out spawnCount))
                        continue;

                    _mobs.Add(mob, spawnCount);
                }
                catch
                {
                    player.SendErrorMessage("Invalid naming format. Format: \"mobname:spawncount\"");
                }
            }
        }

        private void ParseSpawnBoss(IEnumerable<string> args, TSPlayer player)
        {
            var list = new List<string>(args);
            list.RemoveAt(0);
            foreach (var obj in list)
            {
                try
                {
                    var boss = obj.Split(':')[0];
                    var num = obj.Split(':')[1];

                    int spawnCount;
                    if (!int.TryParse(num, out spawnCount))
                        continue;

                    _bosses.Add(boss, spawnCount);
                }
                catch
                {
                    player.SendErrorMessage("Invalid naming format. Format: \"bossname:spawncount\"");
                }
            }
        }
        #endregion

        #region ParseSign
        private void ParseSignCd(IList<string> args)
        {
            int cd;
            if (args.Count < 3)
            {
                //args[0] is command name
                if (!int.TryParse(args[1], out cd))
                {
                    if (SignCommands.config.CooldownGroups.ContainsKey(args[1]))
                    {
                        cd = SignCommands.config.CooldownGroups[args[1]];
                        _cooldownGroup = args[1];
                    }
                }
                _cooldown = cd;
            }
            else
            {
                //args[0] is command name. args[1] is cooldown specifier. args[2] is cooldown
                if (string.Equals(args[1], "global", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!int.TryParse(args[2], out cd))
                    {
                        if (SignCommands.config.CooldownGroups.ContainsKey(args[2]))
                        {
                            cd = SignCommands.config.CooldownGroups[args[2]];
                            _cooldownGroup = args[2];
                        }
                    }
                    _cooldown = cd;
                }
            }
        }
        #endregion

        #region ParseU&G
        private void ParseGroups(IEnumerable<string> args)
        {
            var groups = new List<string>(args);
            //Remove the command name- it's not a group
            groups.RemoveAt(0);

            foreach (var group in groups)
                _groups.Add(group);
        }

        private void ParseUsers(IEnumerable<string> args)
        {
            var users = new List<string>(args);
            //Remove the command name- it's not a user
            users.RemoveAt(0);

            foreach (var user in users)
                _users.Add(user);
        }
        #endregion

        #region SpawnMob
        private void SpawnMobs(Dictionary<string, int> mobs, ScPlayer sPly)
        {
            var mobList = new List<string>();
            foreach (var pair in mobs)
            {
                var amount = Math.Min(pair.Value, Main.maxNPCs);

                var npcs = TShock.Utils.GetNPCByIdOrName(pair.Key);
                if (npcs.Count == 0)
                    continue;

                if (npcs.Count > 1)
                    continue;

                var npc = npcs[0];
                if (npc.type >= 1 && npc.type < Main.maxNPCTypes && npc.type != 113)
                {
                    TSPlayer.Server.SpawnNPC(npc.type, npc.name, amount, _point.X, _point.Y, 45,
                        18);
                    mobList.Add(npc.name + "(" + amount + ")");
                }
                else if (npc.type == 113)
                {
                    if (Main.wof >= 0 || (_point.Y/16f < (Main.maxTilesY - 205)))
                        continue;
                    NPC.SpawnWOF(new Vector2(_point.X, _point.Y));
                    mobList.Add("the Wall of Flesh (" + amount + ")");
                }
            }

            sPly.TsPlayer.SendMessage(String.Format("[Sign] You've spawned {0}", string.Join(", ", mobList)), Color.SteelBlue);
            
            /*TShock.Utils.SendLogs(
                         string.Format("{0} executed: {1}{2} [Via sign command].", sPly.TsPlayer.Name,
                             TShock.Config.CommandSpecifier,
                             "/spawnmob " + string.Join(", ", mobList)), Color.PaleVioletRed, sPly.TsPlayer);*/
        }
        #endregion

        #region SpawnBoss
        private void SpawnBoss(Dictionary<string, int> bosses, ScPlayer sPly)
        {
            int X = 50;
            int Y = 20;
            var bossList = new List<string>();
            foreach (var pair in bosses)
            {
                //TSPlayer.Server.SpawnNPC(TShock.Utils.GetNPCById(num).type, TShock.Utils.GetNPCById(num).name, count, _point.X, _point.Y, X, Y);//for reference
                var npc = new NPC();
                    switch (pair.Key.ToLower())
                    {
                        case "allboss":
                        case "all":
                        case "*":
                            if (Main.dayTime) {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn all bosses in daytime!"), Color.Crimson);
                                return;
                            }
                            int[] allboss = {4,13,35,50,125,126,127,134,222,245,262,266,370,315,325,327,344,345,346};
                            foreach (var i in allboss)
                            {
                                npc.SetDefaults(i);
                                //TSPlayer.Server.SetTime(false, 0.0);
                                TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            }
                            bossList.Add("all bosses(" + pair.Value + ")");
                            //sPly.TsPlayer.SendMessage(String.Format("[Sign] You've spawned {0}", string.Join(", ", bossList)), Color.MediumPurple);
                            break;
                        case "normalboss":
                        case "nmboss":
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn normal bosses in daytime!"), Color.Crimson);
                                return;
                            }
                            int[] nmboss = {4,35,50,222,266};
                            foreach (var i in nmboss)
                            {
                                npc.SetDefaults(i);
                                //TSPlayer.Server.SetTime(false, 0.0);
                                TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            }
                            bossList.Add("normal bosses(" + pair.Value + ")");
                            //sPly.TsPlayer.SendMessage(String.Format("[Sign] You've spawned {0}", string.Join(", ", bossList)), Color.MediumPurple);
                            break;
                        case "mechaboss":
                        case "mcboss":
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn mechanical 3 heads in daytime!"), Color.Crimson);
                                return;
                            }
                            int[] mcboss = {125,126,127,134};
                            foreach (var i in mcboss)
                            {
                                npc.SetDefaults(i);
                                //TSPlayer.Server.SetTime(false, 0.0);
                                TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            }
                            bossList.Add("mechanical 3 heads(" + pair.Value + ")");
                            //sPly.TsPlayer.SendMessage(String.Format("[Sign] You've spawned {0}", string.Join(", ", bossList)), Color.MediumPurple);
                            break;
                        case "hardboss":
                        case "hmboss":
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn hard bosses in daytime!"), Color.Crimson);
                                return;
                            }
                            int[] hmboss = {125,126,127,134,245,262};
                            foreach (var i in hmboss)
                            {
                                npc.SetDefaults(i);
                                //TSPlayer.Server.SetTime(false, 0.0);
                                TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            }
                            bossList.Add("hardmode bosses(" + pair.Value + ")");
                            //sPly.TsPlayer.SendMessage(String.Format("[Sign] You've spawned {0}", string.Join(", ", bossList)), Color.MediumPurple);
                            break;
                        case "pumpboss":
                        case "pkboss":
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn pmoon bosses in daytime!"), Color.Crimson);
                                return;
                            }
                            int[] pkboss = {315,325,328};
                            foreach (var i in pkboss)
                            {
                                npc.SetDefaults(i);
                                //TSPlayer.Server.SetTime(false, 0.0);
                                TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                                TSPlayer.Server.SpawnNPC(TShock.Utils.GetNPCById(315).type, TShock.Utils.GetNPCById(315).name, 1, _point.X, _point.Y, X, Y);//horseman
                            }
                            bossList.Add("pmoon bosses(" + pair.Value + ")");
                            //sPly.TsPlayer.SendMessage(String.Format("[Sign] You've spawned {0}", string.Join(", ", bossList)), Color.MediumPurple);
                            break;
                        case "frozboss":
                        case "fzboss":
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn fmoon bosses in daytime!"), Color.Crimson);
                                return;
                            }
                            int[] fzboss = {344,345,346};
                            foreach (var i in fzboss)
                            {
                                npc.SetDefaults(i);
                                //TSPlayer.Server.SetTime(false, 0.0);
                                TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            }
                            bossList.Add("fmoon bosses(" + pair.Value + ")");
                            //sPly.TsPlayer.SendMessage(String.Format("[Sign] You've spawned {0}", string.Join(", ", bossList)), Color.MediumPurple);
                            break;


                        case "slime":
                        case "king slime":
                            npc.SetDefaults(50);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, 45, 18);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        case "eye":
                        case "eye of cthulhu":
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn eye of cthulhu in daytime!"), Color.Crimson);
                                return;
                            }
                            npc.SetDefaults(4);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        case "brain":
                        case "brain of cthulhu":
                            npc.SetDefaults(266);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        case "eater":
                        case "eater of worlds":
                            npc.SetDefaults(13);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, 55, 22);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        case "bee":
                        case "queen bee":
                            npc.SetDefaults(222);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        case "skeletron":
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn skeletron in daytime!"), Color.Crimson);
                                return;
                            }
                            npc.SetDefaults(35);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        
                        
                        case "wof":
                        case "wall of flesh":
                            if (Main.wof >= 0)
                                return;
                            if (_point.Y / 16f < Main.maxTilesY - 205)
                                break;
                            //TSPlayer.Server.SetTime(false, 0.0);
                            NPC.SpawnWOF(new Vector2(_point.X, _point.Y));
                            bossList.Add("the Wall of Flesh(" + pair.Value + ")");
                            break;


                        case "destroyer":
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn destroyer in daytime!"), Color.Crimson);
                                return;
                            }
                            npc.SetDefaults(134);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, 60, 24);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        case "twins":
                            //TSPlayer.Server.SetTime(false, 0.0);
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn the twins in daytime!"), Color.Crimson);
                                return;
                            }
                            npc.SetDefaults(125);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            npc.SetDefaults(126);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            bossList.Add("the Twins(" + pair.Value + ")");
                            break;
                        case "prime":
                        case "skeletron prime":
                            if (Main.dayTime)
                            {
                                sPly.TsPlayer.SendMessage(String.Format("[Sign] You can't spawn skeletron prime in daytime!"), Color.Crimson);
                                return;
                            }
                            npc.SetDefaults(127);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;

                        
                        case "golem":
                            npc.SetDefaults(245);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, 45, 18);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        case "plantera":
                            npc.SetDefaults(262);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        

                        case "duke fishron":
                        case "fishron":
                            npc.SetDefaults(370);
                            //TSPlayer.Server.SetTime(false, 0.0);
                            TSPlayer.Server.SpawnNPC(npc.type, npc.name, pair.Value, _point.X, _point.Y, X, Y);
                            bossList.Add(npc.name + "(" + pair.Value + ")");
                            break;
                        
                    }
                }
                sPly.TsPlayer.SendMessage(String.Format("[Sign] You've spawned {0}", string.Join(", ", bossList)), Color.MediumPurple);
 
            /*TShock.Utils.SendLogs(
                          string.Format("{0} executed: {1}{2} [Via sign command].", sPly.TsPlayer.Name,
                              TShock.Config.CommandSpecifier,
                              "/spawnboss " + string.Join(", ", bossList)), Color.PaleVioletRed, sPly.TsPlayer);*/
        }
    }
}
        #endregion