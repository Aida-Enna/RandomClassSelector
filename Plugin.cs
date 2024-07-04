﻿using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Veda;

namespace RandomClassSelector
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Random Class Selector";

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; }
        [PluginService] public static ICommandManager Commands { get; set; }
        [PluginService] public static ICondition Conditions { get; set; }
        [PluginService] public static IDataManager Data { get; set; }
        [PluginService] public static IFramework Framework { get; set; }
        [PluginService] public static IGameGui GameGui { get; set; }
        [PluginService] public static ISigScanner SigScanner { get; set; }
        [PluginService] public static IKeyState KeyState { get; set; }
        [PluginService] public static IChatGui Chat { get; set; }
        [PluginService] public static IClientState ClientState { get; set; }
        [PluginService] public static IPartyList PartyList { get; set; }
        [PluginService] public static IPluginLog PluginLog { get; set; }

        public static Configuration PluginConfig { get; set; }
        private PluginCommandManager<Plugin> CommandManager;
        private PluginUI ui;

        public static bool FirstRun = true;
        public string PreviousWorkingChannel;
        public bool SuccessfullyJoined;
        private Random RNGenerator = new Random();

        public Plugin(IDalamudPluginInterface pluginInterface, IChatGui chat, IPartyList partyList, ICommandManager commands, ISigScanner sigScanner)
        {
            PluginInterface = pluginInterface;
            PartyList = partyList;
            Chat = chat;
            SigScanner = sigScanner;

            // Get or create a configuration object
            PluginConfig = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            PluginConfig.Initialize(PluginInterface);

            ui = new PluginUI();
            PluginInterface.UiBuilder.Draw += new System.Action(ui.Draw);
            PluginInterface.UiBuilder.OpenConfigUi += () =>
            {
                PluginUI ui = this.ui;
                ui.IsVisible = !ui.IsVisible;
            };

            // Load all of our commands
            CommandManager = new PluginCommandManager<Plugin>(this, commands);

            Functions.GetChatSignatures(sigScanner);
        }

        [Command("/rnd")]
        [HelpMessage("Shows RandomClassSelector configuration options")]
        public void ShowTwitchOptions(string command, string args)
        {
            ui.IsVisible = !ui.IsVisible;
        }

        [Command("/rndc")]
        [HelpMessage("Selects a random class")]
        public unsafe void RollRandomClass(string command, string args)
        {
            List<string> ClassesToLevel = GetClassesUnderLevel(PluginConfig.MaxClassLevel);
            int SelectedClass = RNGenerator.Next(0, ClassesToLevel.Count());
            string SelectedClassString = ClassesToLevel[SelectedClass];
            if (SelectedClassString.Contains("SMN/SCH"))
            {
                switch(RNGenerator.Next(0,2))
                {
                    case 0:
                        SelectedClassString = SelectedClassString.Replace("SMN/", "");
                        break;
                    case 1:
                        SelectedClassString = SelectedClassString.Replace("/SCH", "");
                        break;
                }
            }
            Chat.Print("Your randomly selected class is: " + SelectedClassString);
            if (PluginConfig.PrintAllChoices)
            {
                string AllClassesDebug = "Your choices were: "; //Make toggleable
                foreach (string ClassName in ClassesToLevel)
                {
                    AllClassesDebug += ClassName + " ";
                }
                Chat.Print(AllClassesDebug);
            }
            if (PluginConfig.ChangeGSUsingShortname)
            {
                PluginLog.Debug("Sending " + "/gs change \"" + SelectedClassString.Split(' ')[0] + "\"");
                Functions.Send("/gs change \"" + SelectedClassString.Split(' ')[0] + "\"");
            }
            if (PluginConfig.ChangeGSUsingLongname)
            {
                PluginLog.Debug("Sending " + "/gs change " + GetFullClassName(SelectedClassString.Split(' ')[0]));
                Functions.Send("/gs change \"" + GetFullClassName(SelectedClassString.Split(' ')[0]) + "\""); 
            }
        }

        public string GetFullClassName(string ShortName)
        {
            switch (ShortName)
            {
                case "GLA":
                    return "Gladiator";

                case "PGL":
                    return "Pugilist";

                case "MRD":
                    return "Marauder";

                case "LNC":
                    return "Lancer";

                case "ARC":
                    return "Archer";

                case "CNJ":
                    return "Conjurer";

                case "THM":
                    return "Thaumaturge";

                case "CRP":
                    return "Carpenter";

                case "BSM":
                    return "Blacksmith";

                case "ARM":
                    return "Armorer";

                case "GSM":
                    return "Goldsmith";

                case "LTW":
                    return "Leatherworker";

                case "WVR":
                    return "Weaver";

                case "ALC":
                    return "Alchemist";

                case "CUL":
                    return "Culinarian";

                case "MIN":
                    return "Miner";

                case "BTN":
                    return "Botanist";

                case "FSH":
                    return "Fisher";

                case "PLD":
                    return "Paladin";

                case "MNK":
                    return "Monk";

                case "WAR":
                    return "Warrior";

                case "DRG":
                    return "Dragoon";

                case "BRD":
                    return "Bard";

                case "WHM":
                    return "White Mage";

                case "BLM":
                    return "Black Mage";

                case "ACN":
                    return "Arcanist";

                case "SMN":
                    return "Summoner";

                case "SCH":
                    return "Scholar";

                case "ROG":
                    return "Rogue";

                case "NIN":
                    return "Ninja";

                case "MCH":
                    return "Machinist";

                case "DRK":
                    return "Dark Knight";

                case "AST":
                    return "Astrologian";

                case "SAM":
                    return "Samurai";

                case "RDM":
                    return "Red Mage";

                case "BLU":
                    return "Blue Mage";

                case "GNB":
                    return "Gunbreaker";

                case "DNC":
                    return "Dancer";

                case "RPR":
                    return "Reaper";

                case "SGE":
                    return "Sage";

                case "VPR":
                    return "Viper";

                case "PCT":
                    return "Pictomancer";

                default:
                    return "Unknown";
            }
        }

        public unsafe List<string> GetClassesUnderLevel(int MaxLevel)
        {
            var playerStatePtr = PlayerState.Instance();
            var Levels = playerStatePtr->ClassJobLevels;
            int ClassCount = 0;
            List<string> LevelsUnderCap = new List<string>();
            foreach (int level in Levels)
            {
                if (level >= MaxLevel) { ClassCount++; continue; } //They're level X or higher, ignore them
                if (level == 0) { ClassCount++; continue; } //Not unlocked, assumedly?
                if (ClassCount > 6 & ClassCount < 18 & !PluginConfig.SuggestCraftersGatherers) { ClassCount++; continue; } //Crafting or Gathering classes, become a toggle later
                if (ClassCount == 25 & !PluginConfig.SuggestBLU) { ClassCount++; continue; } //Blue Mage, become a toggle later

                LevelsUnderCap.Add(GetClassNameByIndex(ClassCount, level) + " (" + level + ")");

                //PluginLog.Debug("Class " + ClassCount + ": " + level.ToString());
                ClassCount++;
            }
            return LevelsUnderCap;
        }

        public string GetClassNameByIndex(int index, int level = 100)
        {
            switch (index)
            {
                case -1:
                    return "ADV";

                case 0:
                    if (level < 30)
                    {
                        return "PGL";
                    }
                    else
                    {
                        return "MNK";
                    }
                case 1:
                    if (level < 30)
                    {
                        return "GLA";
                    }
                    else
                    {
                        return "PLD";
                    }
                case 2:
                    if (level < 30)
                    {
                        return "MRD";
                    }
                    else
                    {
                        return "WAR";
                    }
                case 3:
                    if (level < 30)
                    {
                        return "ARC";
                    }
                    else
                    {
                        return "BRD";
                    }
                case 4:
                    if (level < 30)
                    {
                        return "LNC";
                    }
                    else
                    {
                        return "DRG";
                    }
                case 5:
                    if (level < 30)
                    {
                        return "THM";
                    }
                    else
                    {
                        return "BLM";
                    }
                case 6:
                    if (level < 30)
                    {
                        return "CNJ";
                    }
                    else
                    {
                        return "WHM";
                    }
                case 7:
                    return "CRP";

                case 8:
                    return "BSM";

                case 9:
                    return "ARM";

                case 10:
                    return "GSM";

                case 11:
                    return "LTW";

                case 12:
                    return "WVR";

                case 13:
                    return "ALC";

                case 14:
                    return "CUL";

                case 15:
                    return "MIN";

                case 16:
                    return "BTN";

                case 17:
                    return "FSH";

                case 18:
                    if (level < 30)
                    {
                        return "ACN";
                    }
                    else
                    {
                        return "SMN/SCH";
                    }
                case 19:
                    if (level < 30)
                    {
                        return "ROG";
                    }
                    else
                    {
                        return "NIN";
                    }
                case 20:
                    return "MCH";

                case 21:
                    return "DRK";

                case 22:
                    return "AST";

                case 23:
                    return "SAM";

                case 24:
                    return "RDM";

                case 25:
                    return "BLU";

                case 26:
                    return "GNB";

                case 27:
                    return "DNC";

                case 28:
                    return "RPR";

                case 29:
                    return "SGE";

                case 30:
                    return "VPR";

                case 31:
                    return "PCT";

                default:
                    return "Unknown??";
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            CommandManager.Dispose();

            PluginInterface.SavePluginConfig(PluginConfig);

            PluginInterface.UiBuilder.Draw -= ui.Draw;
            PluginInterface.UiBuilder.OpenConfigUi -= () =>
            {
                PluginUI ui = this.ui;
                ui.IsVisible = !ui.IsVisible;
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}