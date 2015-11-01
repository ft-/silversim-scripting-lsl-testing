﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Common;
using SilverSim.Types;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Scripting.Lsl.Api.Parcel
{
    public partial class ParcelApi
    {
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_STOP = 0;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_PAUSE = 1;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_PLAY = 2;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_LOOP = 3;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_TEXTURE = 4;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_URL = 5;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_TIME = 6;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_AGENT = 7;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_UNLOAD = 8;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_AUTO_ALIGN = 9;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_TYPE = 10;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_SIZE = 11;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_DESC = 12;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int PARCEL_MEDIA_COMMAND_LOOP_SET = 13;

        [APILevel(APIFlags.LSL, "llParcelMediaCommandList")]
        [ForcedSleep(2)]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal void ParcelMediaCommandList(ScriptInstance instance, AnArray commandList)
        {
            throw new NotImplementedException();
        }

        [APILevel(APIFlags.LSL, "llParcelMediaQuery")]
        [ForcedSleep(2)]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal AnArray ParcelMediaQuery(ScriptInstance instance, AnArray query)
        {
            throw new NotImplementedException();
        }
    }
}