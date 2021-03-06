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

#pragma warning disable IDE0018
#pragma warning disable RCS1029

using log4net;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl.Api.ByteString;
using SilverSim.Types;
using System.ComponentModel;

namespace SilverSim.Scripting.Lsl.Api.Http
{
    [ScriptApiName("HTTP")]
    [LSLImplementation]
    [Description("LSL/OSSL HTTP API")]
    public partial class HttpApi : IScriptApi, IPlugin
    {
#if DEBUG
        private static readonly ILog m_Log = LogManager.GetLogger("HTTP LSL API");
#endif
        private LSLHTTP m_HTTPHandler;
        private LSLHTTPClient_RequestQueue m_LSLHTTPClient;

        [APILevel(APIFlags.LSL)]
        public const string URL_REQUEST_GRANTED = "URL_REQUEST_GRANTED";

        [APILevel(APIFlags.LSL)]
        public const string URL_REQUEST_DENIED = "URL_REQUEST_DENIED";

        [APILevel(APIFlags.LSL, "http_request")]
        [StateEventDelegate]
        public delegate void State_http_request(LSLKey request_id, string method, string body);

        [APILevel(APIFlags.LSL, "http_binary_request")]
        [StateEventDelegate]
        public delegate void State_http_binary_request(LSLKey request_id, string method, ByteArrayApi.ByteArray body);

        [APILevel(APIFlags.LSL, "http_response")]
        [StateEventDelegate]
        public delegate void State_http_response(LSLKey request_id, int status, AnArray metadata, string body);

        [APIExtension(APIExtension.ByteArray, "http_binary_response")]
        [StateEventDelegate]
        public delegate void State_http_binary_response(LSLKey request_id, int status, AnArray metadata, ByteArrayApi.ByteArray body);

        public void Startup(ConfigurationLoader loader)
        {
            m_HTTPHandler = loader.GetPluginService<LSLHTTP>("LSLHTTP");
            m_LSLHTTPClient = loader.GetPluginService<LSLHTTPClient_RequestQueue>("LSLHttpClient");
        }

        [ExecutedOnScriptReset]
        [ExecutedOnScriptRemove]
        public void RemoveURLs(ScriptInstance instance)
        {
            lock (instance)
            {
                foreach (UUID ids in ((Script)instance).m_RequestedURLs)
                {
                    m_HTTPHandler.ReleaseURL((string)ids);
                }
            }
        }
    }
}
