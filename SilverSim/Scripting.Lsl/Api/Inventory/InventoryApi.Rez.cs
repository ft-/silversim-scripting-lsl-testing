﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3 with
// the following clarification and special exception.

// Linking this library statically or dynamically with other modules is
// making a combined work based on this library. Thus, the terms and
// conditions of the GNU Affero General Public License cover the whole
// combination.

// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module. An independent module is a module which is not derived from
// or based on this library. If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so. If you do not wish to do so, delete this
// exception statement from your version.

#pragma warning disable IDE0018, IDE0019
#pragma warning disable RCS1029

using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.ServiceInterfaces.ServerParam;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Inventory;
using System.Collections.Generic;
using SavedScriptState = SilverSim.Scripting.Lsl.Script.SavedScriptState;

namespace SilverSim.Scripting.Lsl.Api.Inventory
{
    [ServerParam("LSL.EnforceRezDistanceLimit", ParameterType = typeof(bool), DefaultValue = true)]
    [ServerParam("LSL.RezDistanceMeterLimit", ParameterType = typeof(uint), DefaultValue = 10)]
    public partial class InventoryApi
    {
        private readonly RwLockedDictionary<UUID, bool> m_EnforceRezDistanceLimitParams = new RwLockedDictionary<UUID, bool>();
        private readonly RwLockedDictionary<UUID, uint> m_RezDistanceMeterLimit = new RwLockedDictionary<UUID, uint>();

        [ServerParam("LSL.EnforceRezDistanceLimit")]
        public void EnforceRezDistanceLimitUpdated(UUID regionID, string value)
        {
            bool boolval;
            if (value.Length == 0)
            {
                m_EnforceRezDistanceLimitParams.Remove(regionID);
            }
            else if (bool.TryParse(value, out boolval))
            {
                m_EnforceRezDistanceLimitParams[regionID] = boolval;
            }
            else
            {
                m_EnforceRezDistanceLimitParams[regionID] = true;
            }
        }

        [ServerParam("LSL.RezDistanceMeterLimit")]
        public void RezDistanceMeterLimitUpdated(UUID regionID, string value)
        {
            uint uintval;
            if (value.Length == 0)
            {
                m_RezDistanceMeterLimit.Remove(regionID);
            }
            else if (uint.TryParse(value, out uintval))
            {
                m_RezDistanceMeterLimit[regionID] = uintval;
            }
        }

        private bool IsRezDistanceLimitEnforced(UUID regionID)
        {
            bool value;
            if (m_EnforceRezDistanceLimitParams.TryGetValue(regionID, out value) ||
                m_EnforceRezDistanceLimitParams.TryGetValue(UUID.Zero, out value))
            {
                return value;
            }
            return true;
        }

        private uint GetRezDistanceMeterLimit(UUID regionID)
        {
            uint value;
            if (m_RezDistanceMeterLimit.TryGetValue(regionID, out value) ||
                m_RezDistanceMeterLimit.TryGetValue(UUID.Zero, out value))
            {
                return value;
            }
            return 10;
        }

        private bool TryGetLink(ObjectPart part, int link, out ObjectPart linkedpart)
        {
            ObjectGroup grp = part.ObjectGroup;
            if (LINK_THIS == link)
            {
                linkedpart = part;
                return true;
            }
            else if(LINK_ROOT == link || 0 == link)
            {
                linkedpart = grp.RootPart;
                return true;
            }
            else
            {
                return grp.TryGetValue(link, out linkedpart);
            }
        }

        private Vector3 CalculateGeometricCenter(List<ObjectGroup> groups)
        {
            Vector3 geometriccenteroffset = Vector3.Zero;
            int primcount = 0;
            foreach (ObjectGroup grp in groups)
            {
                foreach (ObjectPart part in grp.Values)
                {
                    geometriccenteroffset += part.GlobalPosition;
                    ++primcount;
                }
            }
            geometriccenteroffset /= primcount;
            geometriccenteroffset -= groups[0].GlobalPosition;
            return geometriccenteroffset;
        }

        [APILevel(APIFlags.ASSL, "asLinkRezObject")]
        public void LinkRezObject(ScriptInstance instance, int link, string inventory, Vector3 pos, Vector3 vel, Quaternion rot, int param)
        {
            lock (instance)
            {
                List<ObjectGroup> groups;
                ObjectPart invpart;
                ObjectPart rezzingpart = instance.Part;
                SceneInterface scene = rezzingpart.ObjectGroup.Scene;
                UUID sceneid = scene.ID;
                bool removeinventory;
                if (IsRezDistanceLimitEnforced(sceneid) &&
                    (instance.Part.GlobalPosition - vel).Length > GetRezDistanceMeterLimit(sceneid))
                {
                    /* silent fail as per definition */
                    return;
                }
                if (TryGetLink(rezzingpart, link, out invpart) &&
                    TryGetObjectInventory(instance, invpart, inventory, out groups, out removeinventory))
                {
                    pos += CalculateGeometricCenter(groups);

                    if (RealRezObject(scene, instance.Item.Owner, rezzingpart, groups, pos, vel, rot, param) &&
                        removeinventory)
                    {
                        rezzingpart.Inventory.Remove(inventory);
                    }
                }
            }
        }

        [APILevel(APIFlags.LSL, "llRezObject")]
        [ForcedSleep(0.1)]
        public void RezObject(ScriptInstance instance, string inventory, Vector3 pos, Vector3 vel, Quaternion rot, int param)
        {
            lock(instance)
            {
                List<ObjectGroup> groups;
                ObjectPart rezzingpart = instance.Part;
                SceneInterface scene = rezzingpart.ObjectGroup.Scene;
                UUID sceneid = scene.ID;
                bool removeinventory;
                if (IsRezDistanceLimitEnforced(sceneid) &&
                    (instance.Part.GlobalPosition - vel).Length > GetRezDistanceMeterLimit(sceneid))
                {
                    /* silent fail as per definition */
                    return;
                }
                if(TryGetObjectInventory(instance, inventory, out groups, out removeinventory))
                {
                    pos += CalculateGeometricCenter(groups);

                    if(RealRezObject(scene, instance.Item.Owner, rezzingpart, groups, pos, vel, rot, param) &&
                        removeinventory)
                    {
                        rezzingpart.Inventory.Remove(inventory);
                    }
                }
            }
        }

        [APILevel(APIFlags.ASSL, "asLinkRezAtRoot")]
        public void LinkRezAtRoot(ScriptInstance instance, int link, string inventory, Vector3 pos, Vector3 vel, Quaternion rot, int param)
        {
            lock (instance)
            {
                List<ObjectGroup> groups;
                ObjectPart rezzingpart = instance.Part;
                ObjectPart invpart;
                SceneInterface scene = rezzingpart.ObjectGroup.Scene;
                UUID sceneid = scene.ID;
                bool removeinventory;
                if (IsRezDistanceLimitEnforced(sceneid) &&
                    (instance.Part.GlobalPosition - vel).Length > GetRezDistanceMeterLimit(sceneid))
                {
                    /* silent fail as per definition */
                    return;
                }

                if (TryGetLink(rezzingpart, link, out invpart) &&
                    TryGetObjectInventory(instance, invpart, inventory, out groups, out removeinventory) &&
                    RealRezObject(scene, instance.Item.Owner, rezzingpart, groups, pos, vel, rot, param) &&
                    removeinventory)
                {
                    rezzingpart.Inventory.Remove(inventory);
                }
            }
        }

        [APILevel(APIFlags.LSL, "llRezAtRoot")]
        [ForcedSleep(0.1)]
        public void RezAtRoot(ScriptInstance instance, string inventory, Vector3 pos, Vector3 vel, Quaternion rot, int param)
        {
            lock (instance)
            {
                List<ObjectGroup> groups;
                ObjectPart rezzingpart = instance.Part;
                SceneInterface scene = rezzingpart.ObjectGroup.Scene;
                UUID sceneid = scene.ID;
                bool removeinventory;
                if (IsRezDistanceLimitEnforced(sceneid) &&
                    (instance.Part.GlobalPosition - vel).Length > GetRezDistanceMeterLimit(sceneid))
                {
                    /* silent fail as per definition */
                    return;
                }

                if (TryGetObjectInventory(instance, inventory, out groups, out removeinventory) &&
                    RealRezObject(scene, instance.Item.Owner, rezzingpart, groups, pos, vel, rot, param) &&
                    removeinventory)
                {
                    rezzingpart.Inventory.Remove(inventory);
                }
            }
        }

        public bool RealRezObject(SceneInterface scene, UUI rezzingowner, ObjectPart rezzingpart, List<ObjectGroup> groups, Vector3 pos, Vector3 vel, Quaternion rot, int param)
        {
            Quaternion rotOff = rot / groups[0].GlobalRotation;
            Vector3 coalesced = groups[0].CoalescedRestoreOffset;
            foreach (ObjectGroup sog in groups)
            {
                sog.RezzingObjectID = rezzingpart.ID;
                sog.GlobalRotation *= rotOff;
                sog.GlobalPosition = pos;
                sog.GlobalPosition += (sog.CoalescedRestoreOffset - coalesced) * rotOff;
                if(!scene.CanRez(rezzingowner, sog.GlobalPosition))
                {
                    return false;
                }
            }

            foreach (ObjectGroup sog in groups)
            {
                sog.RezzingObjectID = rezzingpart.ID;
                foreach (ObjectPart part in sog.ValuesByKey1)
                {
                    part.RezDate = Date.Now;
                    foreach (ObjectPartInventoryItem item in part.Inventory.ValuesByKey2)
                    {
                        if(item.AssetType == AssetType.LSLText)
                        {
                            var savedScriptState = item.ScriptState as SavedScriptState;
                            if (savedScriptState != null)
                            {
                                savedScriptState.StartParameter = param;
                            }
                            else
                            {
                                savedScriptState = new SavedScriptState()
                                {
                                    StartParameter = param
                                };
                                item.ScriptState = savedScriptState;
                            }
                        }
                    }
                }
                sog.Velocity = vel;
            }

            foreach (ObjectGroup sog in groups)
            {
                sog.IsDieAtEdge = true; /* as per definition of llRezObject and llRezAtRoot */
                scene.Add(sog);
                rezzingpart.PostEvent(new ObjectRezEvent()
                {
                    ObjectID = sog.ID
                });
            }

            return true;
        }

        public bool TryGetObjectInventory(ScriptInstance instance, string name, out List<ObjectGroup> groups, out bool removeinventory) =>
            TryGetObjectInventory(instance, instance.Part, name, out groups, out removeinventory);

        public bool TryGetObjectInventory(ScriptInstance instance, ObjectPart linkpart, string name, out List<ObjectGroup> groups, out bool removeinventory)
        {
            ObjectPartInventoryItem item;
            AssetData data;
            ObjectPart rezzingpart = instance.Part;
            ObjectGroup rezzinggrp = rezzingpart.ObjectGroup;
            removeinventory = false;
            groups = null;
            if(!linkpart.Inventory.TryGetValue(name, out item))
            {
                instance.ShoutError(new LocalizedScriptMessage(this, "Item0NotFoundToRez", "Item '{0}' not found to rez", name));
                return false;
            }
            else if(item.InventoryType != InventoryType.Object || item.AssetType != AssetType.Object)
            {
                instance.ShoutError(new LocalizedScriptMessage(this, "Item0IsNotAnObject", "Item '{0}' is not an object.", name));
                return false;
            }
            else if(!rezzingpart.ObjectGroup.Scene.AssetService.TryGetValue(item.AssetID, out data))
            {
                instance.ShoutError(new LocalizedScriptMessage(this, "Item0IsMissingInDatabase", "Item '{0}' is missing in database.", name));
                return false;
            }

            removeinventory = !item.CheckPermissions(instance.Item.Owner, instance.Item.Group, InventoryPermissionsMask.Copy);
            if(removeinventory && rezzinggrp.IsAttached)
            {
                instance.ShoutError(new LocalizedScriptMessage(this, "CannotRezNoCopyObjectsFromAnAttachedObject", "Cannot rez no copy objects from an attached object."));
                return false;
            }

            try
            {
                groups = ObjectXML.FromAsset(data, instance.Item.Owner);
                return true;
            }
            catch
            {
                instance.ShoutError(new LocalizedScriptMessage(this, "Item0HasInvalidContent", "Item '{0}' has invalid content.", name));
                return false;
            }
        }
    }
}
