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

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Script;
using System.ComponentModel;

namespace SilverSim.Scripting.Lsl.Api.Parcel
{
    [ScriptApiName("Parcel")]
    [LSLImplementation]
    [Description("LSL/OSSL Parcel API")]
    public partial class ParcelApi : IScriptApi, IPlugin
    {
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_FLY = 0x1;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_SCRIPTS = 0x2;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_LANDMARK = 0x8;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_TERRAFORM = 0x10;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_DAMAGE = 0x20;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_CREATE_OBJECTS = 0x40;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_USE_ACCESS_GROUP = 0x100;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_USE_ACCESS_LIST = 0x200;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_USE_BAN_LIST = 0x400;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_USE_LAND_PASS_LIST = 0x800;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_LOCAL_SOUND_ONLY = 0x8000;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_RESTRICT_PUSHOBJECT = 0x200000;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_GROUP_SCRIPTS = 0x2000000;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_CREATE_GROUP_OBJECTS = 0x4000000;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_ALL_OBJECT_ENTRY = 0x8000000;
        [APILevel(APIFlags.LSL)]
        public const int PARCEL_FLAG_ALLOW_GROUP_OBJECT_ENTRY = 0x10000000;

        [APILevel(APIFlags.LSL)]
        public const int ERR_GENERIC = -1;
        [APILevel(APIFlags.LSL)]
        public const int ERR_PARCEL_PERMISSIONS = -2;
        [APILevel(APIFlags.LSL)]
        public const int ERR_MALFORMED_PARAMS = -3;
        [APILevel(APIFlags.LSL)]
        public const int ERR_RUNTIME_PERMISSIONS = -4;
        [APILevel(APIFlags.LSL)]
        public const int ERR_THROTTLED = -5;
        
        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }
    }
}
