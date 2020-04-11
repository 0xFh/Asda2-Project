using System;
using System.Collections.Generic;
using WCell.Core.Addons;
using WCell.Core.Variables;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Variables;

namespace WCell.Core
{
    /// <summary>
    /// Contains methods that are needed for both, Auth- and RealmServer commands
    /// </summary>
    public static class CommandUtil
    {
        public static IConfiguration GetConfig(IConfiguration deflt, ITriggerer trigger)
        {
            IConfiguration configuration;
            if (trigger.Text.NextModifiers() == "a")
            {
                string shortName = trigger.Text.NextWord();
                IWCellAddon addon = WCellAddonMgr.GetAddon(shortName);
                if (addon == null)
                {
                    trigger.Reply("Did not find any Addon matching: " + shortName);
                    return (IConfiguration) null;
                }

                configuration = addon.Config;
                if (configuration == null)
                {
                    trigger.Reply("Addon does not have a Configuration: " + (object) addon);
                    return (IConfiguration) null;
                }
            }
            else
                configuration = deflt;

            return configuration;
        }

        public static bool SetCfgValue(IConfiguration cfg, ITriggerer trigger)
        {
            if (trigger.Text.HasNext)
            {
                string name = trigger.Text.NextWord();
                string remainder = trigger.Text.Remainder;
                if (remainder.Length == 0)
                {
                    trigger.Reply("No arguments given.");
                    return false;
                }

                if (cfg.Contains(name))
                {
                    if (cfg.Set(name, remainder))
                    {
                        trigger.Reply("Variable \"{0}\" is now set to: " + remainder, (object) name);
                        return true;
                    }

                    trigger.Reply("Unable to set Variable \"{0}\" to value: {1}.", (object) name, (object) remainder);
                }
                else
                    trigger.Reply("Variable \"{0}\" does not exist.", (object) name);
            }

            return false;
        }

        public static bool GetCfgValue(IConfiguration cfg, ITriggerer trigger)
        {
            if (trigger.Text.HasNext)
            {
                string name = trigger.Text.NextWord();
                object val = cfg.Get(name);
                if (val != null)
                {
                    if (cfg is VariableConfiguration<WCellVariableDefinition> &&
                        ((VariableConfiguration<WCellVariableDefinition>) cfg).GetDefinition(name).IsFileOnly)
                    {
                        trigger.Reply("Cannot display variable: \"{0}\"", (object) name);
                        return false;
                    }

                    trigger.Reply("Variable {0} = {1}", (object) name, (object) Utility.GetStringRepresentation(val));
                    return true;
                }

                trigger.Reply("Variable \"{0}\" does not exist.", (object) name);
                return true;
            }

            trigger.Reply("No arguments given.");
            return false;
        }

        public static void ListCfgValues(IConfiguration cfg, ITriggerer trigger)
        {
            List<IVariableDefinition> vars = new List<IVariableDefinition>(50);
            string filter = trigger.Text.Remainder;
            if (filter.Length > 0)
            {
                cfg.Foreach((Action<IVariableDefinition>) (def =>
                {
                    if (def.IsFileOnly || def.Name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) < 0)
                        return;
                    vars.Add(def);
                }));
                if (vars.Count == 0)
                    trigger.Reply("Could not find any readable globals that contain \"{0}\".", (object) filter);
            }
            else
            {
                cfg.Foreach((Action<IVariableDefinition>) (def =>
                {
                    if (def.IsFileOnly)
                        return;
                    vars.Add(def);
                }));
                if (vars.Count == 0)
                    trigger.Reply("No readable variables found.");
            }

            if (vars.Count <= 0)
                return;
            vars.Sort();
            trigger.Reply("Found {0} globals:", (object) vars.Count);
            foreach (IVariableDefinition def in vars)
                trigger.Reply(def.Name + " = " + def.GetFormattedValue());
        }
    }
}