﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Script;
using SilverSim.Types;
using System;

namespace SilverSim.Scripting.Lsl.Api.Sound
{
    [ScriptApiName("Sound")]
    [LSLImplementation]
    public partial class SoundApi : IScriptApi, IPlugin
    {
        UUID GetSoundAssetID(ScriptInstance instance, string item)
        {
            UUID assetID;
            if (!UUID.TryParse(item, out assetID))
            {
                /* must be an inventory item */
                lock (instance)
                {
                    ObjectPartInventoryItem i = instance.Part.Inventory[item];
                    if (i.InventoryType != Types.Inventory.InventoryType.Sound)
                    {
                        throw new InvalidOperationException(string.Format("Inventory item {0} is not a sound", item));
                    }
                    assetID = i.AssetID;
                }
            }
            return assetID;
        }

        public SoundApi()
        {

        }

        public void Startup(ConfigurationLoader loader)
        {

        }
    }
}