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

using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Asset.Format;
using System.ComponentModel;
using System.Text;

namespace SilverSim.Scripting.Lsl.Api.Notecards
{
    public partial class NotecardApi
    {
        #region osMakeNotecard
        [APILevel(APIFlags.OSSL, "osMakeNotecard")]
        public void MakeNotecard(
            ScriptInstance instance,
            [Description("Name of notecard to be created")]
            string notecardName,
            [Description("Contents for the notecard. string is also allowed here.")]
            AnArray contents)
        {
            var nc = new StringBuilder();
            bool first = true;
            foreach(IValue val in contents)
            {
                if(!first)
                {
                    nc.Append("\n");
                }
                first = false;
                nc.Append(val.ToString());
            }
            MakeNotecard(instance, notecardName, nc.ToString());
        }

        [APILevel(APIFlags.OSSL, "osMakeNotecard")]
        public void MakeNotecard(
            ScriptInstance instance,
            [Description("Name of notecard to be created")]
            string notecardName,
            [Description("Contents for the notecard.")]
            string contents)
        {
            lock (instance)
            {
                ObjectPart thisPart = instance.Part;
                ObjectGroup thisGroup = thisPart.ObjectGroup;
                AssetData asset = new Notecard
                {
                    Text = contents
                };
                asset.ID = UUID.Random;
                asset.Name = notecardName;
                thisGroup.Scene.AssetService.Store(asset);
                var item = new ObjectPartInventoryItem(asset)
                {
                    ParentFolderID = thisPart.ID,
                    Creator = thisGroup.Owner,
                    Owner = thisGroup.Owner,
                    LastOwner = thisGroup.Owner
                };
                for (uint i = 0; i < 1000; ++i)
                {
                    item.Name = (i == 0) ?
                        notecardName :
                        string.Format("{0} {1}", notecardName, i);

                    try
                    {
                        thisPart.Inventory.Add(item.ID, item.Name, item);
                        return;
                    }
                    catch
                    {
                        /* continue */
                    }
                }
            }
            throw new LocalizedScriptErrorException(this, "CouldNotStoreNotecardWithName0", "Could not store notecard with name {0}", notecardName);
        }
        #endregion

        #region osGetNotecard
        [APILevel(APIFlags.OSSL, "osGetNotecard")]
        [Description("read the entire contents of a notecard directly.\nIt does not use the dataserver event.")]
        public string GetNotecard(
            ScriptInstance instance,
            [Description("name of notecard in inventory")]
            string name)
        {
            lock (instance)
            {
                UUID assetID = instance.GetNotecardAssetID(name);
                Notecard nc = instance.Part.ObjectGroup.Scene.GetService<NotecardCache>()[assetID];
                return nc.Text + "\n";
            }
        }

        [APILevel(APIFlags.ASSL, "asGetNotecard")]
        [Description("read the entire contents of a notecard directly.\nIt does not use the dataserver event.")]
        public string GetNotecard2(
            ScriptInstance instance,
            [Description("name of notecard in inventory")]
            string name)
        {
            lock (instance)
            {
                UUID assetID = instance.GetNotecardAssetID(name);
                Notecard nc = instance.Part.ObjectGroup.Scene.GetService<NotecardCache>()[assetID];
                return nc.Text;
            }
        }

        [APILevel(APIFlags.ASSL, "asGetNotecard")]
        [Description("read the entire contents of a notecard directly.\nIt does not use the dataserver event.")]
        public string GetNotecard(
            ScriptInstance instance,
            [Description("link number")]
            int link,
            [Description("name of notecard in inventory")]
            string name)
        {
            lock (instance)
            {
                UUID assetID = instance.GetNotecardAssetID(name, link);
                Notecard nc = instance.Part.ObjectGroup.Scene.GetService<NotecardCache>()[assetID];
                return nc.Text;
            }
        }
        #endregion

        #region osGetNotecardLine
        [APILevel(APIFlags.OSSL, "osGetNotecardLine")]
        [Description("read a line of a notecard directly.\nIt does not use the dataserver event.")]
        public string OsGetNotecardLine(
            ScriptInstance instance,
            [Description("name of notecard in inventory")]
            string name,
            [Description("line number (starting at 0)")]
            int line)
        {
            lock (instance)
            {
                UUID assetID = instance.GetNotecardAssetID(name);
                Notecard nc = instance.Part.ObjectGroup.Scene.GetService<NotecardCache>()[assetID];
                string[] lines = nc.Text.Split('\n');
                if (line >= lines.Length || line < 0)
                {
                    return EOF;
                }
                return lines[line];
            }
        }

        [APILevel(APIFlags.ASSL, "asGetNotecardLine")]
        [Description("read a line of a notecard directly.\nIt does not use the dataserver event.")]
        public string OsGetNotecardLine(
            ScriptInstance instance,
            [Description("link number")]
            int link,
            [Description("name of notecard in inventory")]
            string name,
            [Description("line number (starting at 0)")]
            int line)
        {
            lock (instance)
            {
                UUID assetID = instance.GetNotecardAssetID(name, link);
                Notecard nc = instance.Part.ObjectGroup.Scene.GetService<NotecardCache>()[assetID];
                string[] lines = nc.Text.Split('\n');
                if (line >= lines.Length || line < 0)
                {
                    return EOF;
                }
                return lines[line];
            }
        }
        #endregion

        #region osGetNumberOfNotecardLines
        [APILevel(APIFlags.OSSL, "osGetNumberOfNotecardLines")]
        [Description("read number of lines of a notecard directly.\nIt does not use the dataserver event.")]
        public int OsGetNumberOfNotecardLines(
            ScriptInstance instance,
            [Description("name of notecard in inventory")]
            string name)
        {
            lock (instance)
            {
                UUID assetID = instance.GetNotecardAssetID(name);
                Notecard nc = instance.Part.ObjectGroup.Scene.GetService<NotecardCache>()[assetID];
                return nc.Text.Split('\n').Length;
            }
        }

        [APILevel(APIFlags.ASSL, "asGetNumberOfNotecardLines")]
        [Description("read number of lines of a notecard directly.\nIt does not use the dataserver event.")]
        public int OsGetNumberOfNotecardLines(
            ScriptInstance instance,
            [Description("link number")]
            int link,
            [Description("name of notecard in inventory")]
            string name)
        {
            lock (instance)
            {
                UUID assetID = instance.GetNotecardAssetID(name, link);
                Notecard nc = instance.Part.ObjectGroup.Scene.GetService<NotecardCache>()[assetID];
                return nc.Text.Split('\n').Length;
            }
        }
        #endregion
    }
}
