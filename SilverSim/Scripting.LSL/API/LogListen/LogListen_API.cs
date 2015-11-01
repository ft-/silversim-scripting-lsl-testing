﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Scripting.Lsl.Api.LogListen
{
    [ScriptApiName("LogListen")]
    [LSLImplementation]
    public class LogListenApi : IScriptApi, IPlugin
    {
        public LogListenApi()
        {

        }

        public void Startup(ConfigurationLoader loader)
        {

        }

        [APIExtension(APIExtension.Admin, "asLogListen")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        void LogListen(ScriptInstance instance, int onChannel, int enable)
        {
            throw new NotImplementedException();
        }
    }
}
