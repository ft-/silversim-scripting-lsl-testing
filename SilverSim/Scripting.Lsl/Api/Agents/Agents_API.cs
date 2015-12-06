﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Agent;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Types;
using System;

namespace SilverSim.Scripting.Lsl.Api.Base
{
    [ScriptApiName("Agents")]
    [LSLImplementation]
    public class Agents_API : IScriptApi, IPlugin
    {
        public Agents_API()
        {
            /* intentionally left empty */
        }

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }

        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_FLYING = 0x0001;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_ATTACHMENTS = 0x0002;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_SCRIPTED = 0x0004;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_MOUSELOOK = 0x0008;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_SITTING = 0x0010;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_ON_OBJECT = 0x0020;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_AWAY = 0x0040;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_WALKING = 0x0080;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_IN_AIR = 0x0100;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_TYPING = 0x0200;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_CROUCHING = 0x0400;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_BUSY = 0x0800;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_ALWAYS_RUN = 0x1000;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_AUTOPILOT = 0x2000;

        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_LIST_PARCEL = 1;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_LIST_PARCEL_OWNER = 2;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int AGENT_LIST_REGION = 4;

        [APILevel(APIFlags.LSL, "llGetAgentList")]
        public AnArray GetAgentList(ScriptInstance instance, int scope, AnArray options)
        {
            throw new NotImplementedException("llGetAgentList(int, list)");
        }

        [APILevel(APIFlags.LSL, "llGetAgentInfo")]
        public int GetAgentInfo(ScriptInstance instance, LSLKey id)
        {
            throw new NotImplementedException("llGetAgentInfo(key)");
        }

        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int DATA_ONLINE = 1;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int DATA_NAME = 2;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int DATA_BORN = 3;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int DATA_RATING = 4;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int DATA_PAYINFO = 8;

        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int PAYMENT_INFO_ON_FILE = 0x1;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        public const int PAYMENT_INFO_USED = 0x2;

        [APILevel(APIFlags.LSL, "llRequestAgentData")]
        [ForcedSleep(0.1)]
        public LSLKey RequestAgentData(ScriptInstance instance, LSLKey id, int data)
        {
            throw new NotImplementedException("llRequestAgentData(key, integer)");
        }

        [APILevel(APIFlags.OSSL, "osSetSpeed")]
        public void SetSpeed(ScriptInstance instance, LSLKey id, double speedfactor)
        {
            throw new NotImplementedException("osSetSpeed(key, float)");
        }

        [APILevel(APIFlags.OSSL, "osInviteToGroup")]
        public int OsInviteToGroup(ScriptInstance instance, LSLKey id)
        {
            throw new NotImplementedException("osInviteToGroup(key)");
        }

        [APILevel(APIFlags.OSSL, "osEjectFromGroup")]
        public int OsEjectFromToGroup(ScriptInstance instance, LSLKey id)
        {
            throw new NotImplementedException("osEjectFromGroup(key)");
        }

        [APILevel(APIFlags.LSL, "llRequestDisplayName")]
        public LSLKey RequestDisplayName(ScriptInstance instance, LSLKey id)
        {
            throw new NotImplementedException("llRequestDisplayName(key)");
        }

        [APILevel(APIFlags.LSL, "llGetUsername")]
        public string GetUsername(ScriptInstance instance, LSLKey id)
        {
            /* only when child or root agent is in sim */
            lock(instance)
            {
                IAgent agent;
                if(instance.Part.ObjectGroup.Scene.Agents.TryGetValue(id.AsUUID, out agent))
                {
                    return agent.Owner.FullName.Replace(' ', '.');
                }
                return string.Empty;
            }
        }

        [APILevel(APIFlags.LSL, "llRequestUsername")]
        public LSLKey RequestUsername(ScriptInstance instance, LSLKey id)
        {
            throw new NotImplementedException("llRequestUsername(key)");
        }

        [APILevel(APIFlags.LSL, "llGetDisplayName")]
        public string GetDisplayName(ScriptInstance instance, LSLKey id)
        {
            throw new NotImplementedException("llGetDisplayName(key)");
        }

        [APILevel(APIFlags.LSL, "llKey2Name")]
        public string Key2Name(ScriptInstance instance, LSLKey id)
        {
            lock(instance)
            {
                IObject obj;
                if(instance.Part.ObjectGroup.Scene.Objects.TryGetValue(id, out obj))
                {
                    return obj.Name;
                }
            }
            return string.Empty;
        }

        [APILevel(APIFlags.LSL, "osKey2Name")]
        public string OsKey2Name(ScriptInstance instance, LSLKey id)
        {
            lock (instance)
            {
                IAgent obj;
                if (instance.Part.ObjectGroup.Scene.Agents.TryGetValue(id, out obj))
                {
                    return obj.Owner.FullName;
                }
            }
            return string.Empty;
        }

        [APILevel(APIFlags.LSL, "osGetGender")]
        public string OsGetGender(ScriptInstance instance, LSLKey id)
        {
            throw new NotImplementedException("osGetGender(key)");
        }

        [APILevel(APIFlags.OSSL, "osGetHealth")]
        public double GetHealth(ScriptInstance instance, LSLKey avatar)
        {
            throw new NotImplementedException("osGetHealth(key)");
        }

        [APILevel(APIFlags.LSL, "osAvatarName2Key")]
        public string OsAvatarName2Key(ScriptInstance instance, string firstName, string lastName)
        {
            throw new NotImplementedException("osAvatarName2Key(string, string)");
        }

        [APILevel(APIFlags.LSL, "llGetAgentSize")]
        public Vector3 GetAgentSize(ScriptInstance instance, LSLKey id)
        {
            lock (instance)
            {
                IAgent agent;
                if(!instance.Part.ObjectGroup.Scene.RootAgents.TryGetValue(id, out agent))
                {
                    return Vector3.Zero;
                }
                return agent.Size;
            }
        }

        [APILevel(APIFlags.LSL, "osAgentSaveAppearance")]
        public LSLKey AgentSaveAppearance(ScriptInstance instance, LSLKey agentId, string notecard)
        {
            lock (instance)
            {
                instance.CheckThreatLevel("osAgentSaveAppearance", ScriptInstance.ThreatLevelType.VeryHigh);
            }
            throw new NotImplementedException("osAgentSaveAppearance(key, string)");
        }

        [APILevel(APIFlags.LSL, "osOwnerSaveAppearance")]
        public LSLKey OwnerSaveAppearance(ScriptInstance instance, string notecard)
        {
            lock (instance)
            {
                instance.CheckThreatLevel("osOwnerSaveAppearance", ScriptInstance.ThreatLevelType.High);
            }
            throw new NotImplementedException("osOwnerSaveAppearance(string)");
        }

        [APILevel(APIFlags.LSL, "llTeleportAgentHome")]
        public void TeleportAgentHome(ScriptInstance instance, LSLKey avatar)
        {
            throw new NotImplementedException("llTeleportAgentHome(key)");
        }

        [APILevel(APIFlags.OSSL, "osTeleportAgent")]
        public void TeleportAgent(ScriptInstance instance, LSLKey agent, int regionX, int regionY, Vector3 position, Vector3 lookAt)
        {
            lock(instance)
            {
                instance.CheckThreatLevel("osTeleportAgent", ScriptInstance.ThreatLevelType.VeryHigh);
            }
            throw new NotImplementedException("osTeleportAgent(key, integer, integer, vector, vector)");
        }

        [APILevel(APIFlags.OSSL, "osTeleportAgent")]
        public void TeleportAgent(ScriptInstance instance, LSLKey agent, string regionName, Vector3 position, Vector3 lookAt)
        {
            lock (instance)
            {
                instance.CheckThreatLevel("osTeleportAgent", ScriptInstance.ThreatLevelType.VeryHigh);
            }
            throw new NotImplementedException("osTeleportAgent(key, string, vector, vector)");
        }

        [APILevel(APIFlags.OSSL, "osTeleportAgent")]
        public void TeleportAgent(ScriptInstance instance, LSLKey agent, Vector3 position, Vector3 lookAt)
        {
            lock (instance)
            {
                instance.CheckThreatLevel("osTeleportAgent", ScriptInstance.ThreatLevelType.VeryHigh);
            }
            throw new NotImplementedException("osTeleportAgent(key, vector, vector)");
        }

        [APILevel(APIFlags.OSSL, "osTeleportOwner")]
        public void TeleportOwner(ScriptInstance instance, int regionX, int regionY, Vector3 position, Vector3 lookAt)
        {
            throw new NotImplementedException("osTeleportOwner(integer, integer, vector, vector)");
        }

        [APILevel(APIFlags.OSSL, "osTeleportOwner")]
        public void TeleportOwner(ScriptInstance instance, string regionName, Vector3 position, Vector3 lookAt)
        {
            throw new NotImplementedException("osTeleportOwner(string, vector, vector)");
        }

        [APILevel(APIFlags.OSSL, "osTeleportOwner")]
        public void TeleportOwner(ScriptInstance instance, Vector3 position, Vector3 lookAt)
        {
            throw new NotImplementedException("osTeleportOwner(vector, vector)");
        }

        [APILevel(APIFlags.LSL, "llAttachToAvatar")]
        public void AttachToAvatar(ScriptInstance instance, int attach_point)
        {
            throw new NotImplementedException("llAttachToAvatar(integer)");
        }

        [APILevel(APIFlags.LSL, "llAttachToAvatarTemp")]
        public void AttachToAvatarTemp(ScriptInstance instance, int attach_point)
        {
            throw new NotImplementedException("llAttachToAvatarTemp(integer)");
        }

        [APILevel(APIFlags.LSL, "llDetachFromAvatar")]
        public void DetachFromAvatar(ScriptInstance instance)
        {
            throw new NotImplementedException("llDetachFromAvatar()");
        }

        [APILevel(APIFlags.OSSL, "osForceAttachToAvatar")]
        public void ForceAttachToAvatar(ScriptInstance instance, int attach_point)
        {
            lock(instance)
            {
                instance.CheckThreatLevel("osForceAttachToAvatar", ScriptInstance.ThreatLevelType.High);
                throw new NotImplementedException("osForceAttachToAvatar(integer)");
            }
        }

        [APILevel(APIFlags.OSSL, "osForceAttachToAvatarFromInventory")]
        public void ForceAttachToAvatarFromInventory(ScriptInstance instance, string item_name, int attach_point)
        {
            lock (instance)
            {
                instance.CheckThreatLevel("osForceAttachToAvatarFromInventory", ScriptInstance.ThreatLevelType.High);
                throw new NotImplementedException("osForceAttachToAvatarFromInventory(string, integer)");
            }
        }

        [APILevel(APIFlags.OSSL, "osForceAttachToOtherAvatarFromInventory")]
        public void ForceAttachToOtherAvatarFromInventory(ScriptInstance instance, LSLKey id, string item_name, int attach_point)
        {
            lock (instance)
            {
                instance.CheckThreatLevel("osForceAttachToOtherAvatarFromInventory", ScriptInstance.ThreatLevelType.VeryHigh);
                throw new NotImplementedException("osForceAttachToOtherAvatarFromInventory(key, string, integer)");
            }
        }

        [APILevel(APIFlags.OSSL, "osCauseDamage")]
        public void CauseDamage(ScriptInstance instance, LSLKey id, double health)
        {
            throw new NotImplementedException("osCauseDamage(float)");
        }

        [APILevel(APIFlags.OSSL, "osCauseHealing")]
        public void CauseHealing(ScriptInstance instance, LSLKey id, double health)
        {
            throw new NotImplementedException("osCauseHealing(float)");
        }

        [APILevel(APIFlags.OSSL, "osForceDetachFromAvatar")]
        public void ForceDetachFromAvatar(ScriptInstance instance)
        {
            throw new NotImplementedException("osForceDetachFromAvatar()");
        }

        [APILevel(APIFlags.OSSL, "osDropAttachment")]
        public void DropAttachment(ScriptInstance instance)
        {
            throw new NotImplementedException("osDropAttachment()");
        }

        [APILevel(APIFlags.OSSL, "osDropAttachmentAt")]
        public void DropAttachmentAt(ScriptInstance instance, Vector3 pos, Quaternion rot)
        {
            throw new NotImplementedException("osDropAttachmentAt(vector, rotation)");
        }

        [APILevel(APIFlags.OSSL, "osForceDropAttachment")]
        public void ForceDropAttachment(ScriptInstance instance)
        {
            throw new NotImplementedException("osForceDropAttachment()");
        }

        [APILevel(APIFlags.OSSL, "osForceDropAttachmentAt")]
        public void ForceDropAttachmentAt(ScriptInstance instance, Vector3 pos, Quaternion rot)
        {
            throw new NotImplementedException("osForceDropAttachmentAt(vector, rotation)");
        }

        [APILevel(APIFlags.OSSL, "osGetNumberOfAttachments")]
        public AnArray GetNumberOfAttachments(ScriptInstance instance, LSLKey avatar, AnArray attachmentPoints)
        {
            throw new NotImplementedException("osGetNumberOfAttachments(key, list)");
        }

        [APILevel(APIFlags.OSSL, "osKickAvatar")]
        public void KickAvatar(ScriptInstance instance, string firstName, string lastName, string alert)
        {
            throw new NotImplementedException("osKickAvatar(string, string, string)");
        }

        [APILevel(APIFlags.OSSL, "osForceOtherSit")]
        public void ForceOtherSit(ScriptInstance instance, LSLKey avatar)
        {
            lock(instance)
            {
                instance.CheckThreatLevel("osForceOtherSit", ScriptInstance.ThreatLevelType.VeryHigh);
            }
            throw new NotImplementedException("osForceOtherSit(key)");
        }

        [APILevel(APIFlags.OSSL, "osForceOtherSit")]
        public void ForceOtherSit(ScriptInstance instance, LSLKey avatar, LSLKey target)
        {
            lock (instance)
            {
                instance.CheckThreatLevel("osForceOtherSit", ScriptInstance.ThreatLevelType.VeryHigh);
            }
            throw new NotImplementedException("osForceOtherSit(key, key)");
        }

        [APILevel(APIFlags.LSL, "llGetAgentLanguage")]
        public string GetAgentLanguage(ScriptInstance instance, LSLKey avatar)
        {
            /* Details from LSL wiki
             *
             * If the user has "Share language with objects" disabled then this function returns an empty string.
             * During a 1-5 seconds period after which an agent is logging in, this function will return an empty string as well, until the viewer sends the data to the simulator.             
             * Users may prefer to see the client interface in a language that is not their native language, and some may prefer to use objects in the native language of the creator, or dislike low-quality translations. Consider providing a manual language override when it is appropriate. 
             * New language/variant values may be added later. Scripts may need to be prepared for unexpected values.
             * If the viewer is set to "System Default" the possible return may be outside the list given above. see List of ISO 639-1 codes for reference.
             * Viewers can specify other arbitrary language strings with the 'InstallLanguage' debug setting. For example, launching the viewer with "--set InstallLanguage american" results this function returning 'american' for the avatar. VWR-12222
             *   If the viewer supplies a multiline value, the simulator will only accept the first line and ignore all others. SVC-5503
             */
            throw new NotImplementedException("llGetAgentLanguage(key)");
        }

        #region osGetAvatarList
        [APILevel(APIFlags.OSSL, "osGetAvatarList")]
        public AnArray GetAvatarList(ScriptInstance instance)
        {
            AnArray res = new AnArray();

            lock (instance)
            {
                SceneInterface thisScene = instance.Part.ObjectGroup.Scene;
                UUID ownerID = thisScene.Owner.ID;
                foreach (IAgent agent in thisScene.Agents)
                {
                    if (agent.ID == ownerID)
                    {
                        continue;
                    }
                    res.Add(new LSLKey(agent.ID));
                    res.Add(agent.GlobalPosition);
                    res.Add(agent.Name);
                }
            }
            return res;
        }
        #endregion

        #region osGetAgents
        [APILevel(APIFlags.OSSL, "osGetAgents")]
        public AnArray GetAgents(ScriptInstance instance)
        {
            AnArray res = new AnArray();

            lock (instance)
            {
                foreach (IAgent agent in instance.Part.ObjectGroup.Scene.Agents)
                {
                    res.Add(agent.Name);
                }
            }
            return res;
        }
        #endregion

        [APILevel(APIFlags.OSSL, "osGetAgentIP")]
        public string GetAgentIP(ScriptInstance instance, LSLKey key)
        {
            lock(instance)
            {
                instance.CheckThreatLevel("osGetAgentIP", ScriptInstance.ThreatLevelType.High);

                IAgent agent;
                if(instance.Part.ObjectGroup.Scene.Agents.TryGetValue(key.AsUUID, out agent))
                {
                    throw new NotImplementedException("osGetAgentIP(key)");
                }
                return string.Empty;
            }
        }
    }
}
