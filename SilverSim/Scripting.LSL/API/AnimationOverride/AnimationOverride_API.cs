﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Agent;
using SilverSim.Scene.Types.Script;
using SilverSim.Types;
using SilverSim.Types.Script;
using System;
using SilverSim.Scene.Types.Object;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Scripting.LSL.Api.AnimationOverride
{
    [ScriptApiName("AnimationOverride")]
    [LSLImplementation]
    public partial class AnimationOverrideApi : IScriptApi, IPlugin
    {
        public AnimationOverrideApi()
        {

        }

        public void Startup(ConfigurationLoader loader)
        {

        }

        [APILevel(APIFlags.LSL, "llSetAnimationOverride")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        void SetAnimationOverride(ScriptInstance instance, string anim_state, string anim)
        {
            lock (instance)
            {
                IAgent agent;
                ObjectPartInventoryItem.PermsGranterInfo grantinfo = instance.Item.PermsGranter;

                if ((grantinfo.PermsMask & ScriptPermissions.OverrideAnimations) == 0 ||
                    grantinfo.PermsGranter == UUI.Unknown)
                {
                    return;
                }
                try
                {
                    agent = instance.Part.ObjectGroup.Scene.Agents[grantinfo.PermsGranter.ID];
                }
                catch
                {
                    instance.ShoutError("llSetAnimationOverride: permission granter not in region");
                    return;
                }

                try
                {
                    agent.SetAnimationOverride(anim_state, anim);
                }
                catch (Exception e)
                {
                    instance.ShoutError(e.Message);
                }
            }
        }

        [APILevel(APIFlags.LSL, "llGetAnimationOverride")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        string GetAnimationOverride(ScriptInstance instance, string anim_state)
        {
            lock (instance)
            {
                IAgent agent;
                ObjectPartInventoryItem.PermsGranterInfo grantinfo = instance.Item.PermsGranter;
                if (((grantinfo.PermsMask & ScriptPermissions.OverrideAnimations) == 0 &&
                    (grantinfo.PermsMask & ScriptPermissions.TriggerAnimation) == 0) ||
                    grantinfo.PermsGranter == UUI.Unknown)
                {
                    return string.Empty;
                }
                try
                {
                    agent = instance.Part.ObjectGroup.Scene.Agents[grantinfo.PermsGranter.ID];
                }
                catch
                {
                    instance.ShoutError("llSetAnimationOverride: permission granter not in region");
                    return string.Empty;
                }

                agent.ResetAnimationOverride(anim_state);
                return anim_state;
            }
        }

        [APILevel(APIFlags.LSL, "llResetAnimationOverride")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        void ResetAnimationOverride(ScriptInstance instance, string anim_state)
        {
            lock (instance)
            {
                IAgent agent;
                ObjectPartInventoryItem.PermsGranterInfo grantinfo = instance.Item.PermsGranter;
                if ((grantinfo.PermsMask & ScriptPermissions.OverrideAnimations) == 0 ||
                    grantinfo.PermsGranter == UUI.Unknown)
                {
                    return;
                }
                try
                {
                    agent = instance.Part.ObjectGroup.Scene.Agents[grantinfo.PermsGranter.ID];
                }
                catch
                {
                    instance.ShoutError("llSetAnimationOverride: permission granter not in region");
                    return;
                }

                agent.ResetAnimationOverride(anim_state);
            }
        }
    }
}
