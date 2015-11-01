﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Script;
using SilverSim.Types;
using SilverSim.Types.Script;
using SilverSim.Types.Inventory;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Scripting.Lsl.Api.Inventory
{
    [ScriptApiName("Inventory")]
    [LSLImplementation]
    public class InventoryApi : IScriptApi, IPlugin
    {
        public InventoryApi()
        {

        }

        public void Startup(ConfigurationLoader loader)
        {

        }

        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_ALL = -1;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_NONE = -1;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_TEXTURE = 0;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_SOUND = 1;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_LANDMARK = 3;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_CLOTHING = 5;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_OBJECT = 6;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_NOTECARD = 7;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_SCRIPT = 10;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_BODYPART = 13;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_ANIMATION = 20;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int INVENTORY_GESTURE = 21;


        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int MASK_BASE = 0;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int MASK_OWNER = 1;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int MASK_GROUP = 2;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int MASK_EVERYONE = 3;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int MASK_NEXT = 4;

        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PERM_TRANSFER = 8192;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PERM_MODIFY = 16384;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PERM_COPY = 32768;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PERM_MOVE = 524288;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PERM_ALL = 2147483647;

        [APILevel(APIFlags.LSL, "llGiveInventory")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal void GiveInventory(ScriptInstance instance, LSLKey destination, string inventory)
        {
            throw new NotImplementedException();
        }

        [APILevel(APIFlags.LSL, "llGiveInventoryList")]
        [ForcedSleep(3)]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal void GiveInventoryList(ScriptInstance instance, LSLKey target, string folder, AnArray inventory)
        {
            throw new NotImplementedException();
        }

        [APILevel(APIFlags.LSL, "llRemoveInventory")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal void RemoveInventory(ScriptInstance instance, string item)
        {
            ObjectPartInventoryItem resitem;
            lock (instance)
            {
                if (instance.Part.Inventory.TryGetValue(item, out resitem))
                {
                    ScriptInstance si = resitem.ScriptInstance;

                    instance.Part.Inventory.Remove(resitem.ID);
                    if (si == instance)
                    {
                        throw new ScriptAbortException();
                    }
                    else if (si != null)
                    {
                        si = resitem.RemoveScriptInstance;
                        if (si != null)
                        {
                            si.Abort();
                            si.Remove();
                        }
                    }
                }
            }
        }

        [APILevel(APIFlags.LSL, "llGetInventoryCreator")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal LSLKey GetInventoryCreator(ScriptInstance instance, string item)
        {
            lock (instance)
            {
                try
                {
                    return instance.Part.Inventory[item].Creator.ID;
                }
                catch
                {
                    return UUID.Zero;
                }
            }
        }

        [APILevel(APIFlags.LSL, "llGetInventoryKey")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal LSLKey GetInventoryKey(ScriptInstance instance, string item)
        {
            lock (instance)
            {
                try
                {
                    return instance.Part.Inventory[item].AssetID;
                }
                catch
                {
                    return UUID.Zero;
                }
            }
        }

        [APILevel(APIFlags.LSL, "llGetInventoryName")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal string GetInventoryName(ScriptInstance instance, int type, int number)
        {
            lock(instance)
            {
                try
                {
                    if (type == INVENTORY_ALL)
                    {
                        return instance.Part.Inventory[(uint)number].Name;
                    }
                    else if (type >= 0)
                    {
                        return instance.Part.Inventory[(Types.Inventory.InventoryType)type, (uint)number].Name;
                    }
                }
                catch
                {

                }
            }
            return string.Empty;
        }

        [APILevel(APIFlags.LSL, "llGetInventoryNumber")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal int GetInventoryNumber(ScriptInstance instance, int type)
        {
            lock (instance)
            {
                if (type == INVENTORY_ALL)
                {
                    return instance.Part.Inventory.Count;
                }
                return instance.Part.Inventory.CountType((Types.Inventory.InventoryType)type);
            }
        }

        [APILevel(APIFlags.LSL, "llSetInventoryPermMask")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal void SetInventoryPermMask(ScriptInstance instance, string name, int category, int mask)
        {
            throw new NotImplementedException();
        }

        [APILevel(APIFlags.LSL, "llGetInventoryPermMask")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal int GetInventoryPermMask(ScriptInstance instance, string name, int category)
        {
            lock(instance)
            {
                try
                {
                    ObjectPartInventoryItem item = instance.Part.Inventory[name];
                    InventoryPermissionsMask mask;
                    switch(category)
                    {
                        case MASK_BASE:
                            mask = item.Permissions.Base;
                            break;

                        case MASK_EVERYONE:
                            mask = item.Permissions.EveryOne;
                            break;

                        case MASK_GROUP:
                            mask = item.Permissions.Group;
                            break;

                        case MASK_NEXT:
                            mask = item.Permissions.NextOwner;
                            break;

                        case MASK_OWNER:
                            mask = item.Permissions.Current;
                            break;

                        default:
                            mask = InventoryPermissionsMask.None;
                            break;
                    }
                    return (int)(UInt32)mask;
                }
                catch
                {
                    throw new ArgumentException(string.Format("Inventory item {0} does not exist", name));
                }
            }
        }

        [APILevel(APIFlags.LSL, "llGetInventoryType")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal int GetInventoryType(ScriptInstance instance, string name)
        {
            lock (instance)
            {
                try
                {
                    return (int)instance.Part.Inventory[name].InventoryType;
                }
                catch
                {
                    return -1;
                }
            }
        }

        [APILevel(APIFlags.LSL, "llRequestInventoryData")]
        [ForcedSleep(1.0)]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal LSLKey RequestInventoryData(ScriptInstance instance, string name)
        {
            throw new NotImplementedException();
        }

        #region osGetInventoryDesc
        [APILevel(APIFlags.OSSL, "osGetInventoryDesc")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal string GetInventoryDesc(ScriptInstance instance, string item)
        {
            lock (instance)
            {
                try
                {
                    return instance.Part.Inventory[item].Description;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        #endregion

        #region Rez Inventory
        [APILevel(APIFlags.LSL, "llRezObject")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal void RezObject(ScriptInstance instance, string inventory, Vector3 pos, Vector3 vel, Quaternion rot, int param)
        {
            throw new NotImplementedException();
        }

        [APILevel(APIFlags.LSL, "llRezAtRoot")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal void RezAtRoot(ScriptInstance instance, string inventory, Vector3 pos, Vector3 vel, Quaternion rot, int param)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}