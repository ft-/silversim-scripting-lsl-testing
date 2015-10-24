﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Common;
using System;

namespace SilverSim.Scripting.LSL.API.LogListen
{
    [ScriptApiName("LogListen")]
    [LSLImplementation]
    public class LogListen_API : MarshalByRefObject, IScriptApi, IPlugin
    {
        public LogListen_API()
        {

        }

        public void Startup(ConfigurationLoader loader)
        {

        }

        [APILevel(APIFlags.ASSL, "asLogListen")]
        public void LogListen(ScriptInstance instance, int onChannel, int enable)
        {
            throw new NotImplementedException();
        }
    }
}
