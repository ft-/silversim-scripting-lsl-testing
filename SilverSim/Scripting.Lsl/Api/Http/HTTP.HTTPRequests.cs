﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Types;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SilverSim.Scripting.Lsl.Api.Http
{
    public partial class HttpApi
    {
        [APILevel(APIFlags.LSL)]
        public const int HTTP_METHOD = 0;
        [APILevel(APIFlags.LSL)]
        public const int HTTP_MIMETYPE = 1;
        [APILevel(APIFlags.LSL)]
        public const int HTTP_BODY_MAXLENGTH = 2;
        [APILevel(APIFlags.LSL)]
        public const int HTTP_VERIFY_CERT = 3;
        [APILevel(APIFlags.LSL)]
        public const int HTTP_VERBOSE_THROTTLE = 4;
        [APILevel(APIFlags.LSL)]
        public const int HTTP_CUSTOM_HEADER = 5;
        [APILevel(APIFlags.LSL)]
        public const int HTTP_PRAGMA_NO_CACHE = 6;

        private readonly string[] m_AllowedHttpHeaders =
        {
            "Accept", "Accept-Charset", "Accept-Encoding", "Accept-Language",
            "Accept-Ranges", "Age", "Allow", "Authorization", "Cache-Control",
            "Connection", "Content-Encoding", "Content-Language",
            "Content-Length", "Content-Location", "Content-MD5",
            "Content-Range", "Content-Type", "Date", "ETag", "Expect",
            "Expires", "From", "Host", "If-Match", "If-Modified-Since",
            "If-None-Match", "If-Range", "If-Unmodified-Since", "Last-Modified",
            "Location", "Max-Forwards", "Pragma", "Proxy-Authenticate",
            "Proxy-Authorization", "Range", "Referer", "Retry-After", "Server",
            "TE", "Trailer", "Transfer-Encoding", "Upgrade", "User-Agent",
            "Vary", "Via", "Warning", "WWW-Authenticate"
        };

        static readonly Regex m_AuthRegex = new Regex(@"^(https?:\/\/)(\w+):(\w+)@(.*)$");

        [APILevel(APIFlags.LSL, "llHTTPRequest")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidRepetitiveCallsToPropertiesRule")]
        [SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
        public LSLKey HTTPRequest(ScriptInstance instance, string url, AnArray parameters, string body)
        {
            LSLHTTPClient_RequestQueue.LSLHttpRequest req = new LSLHTTPClient_RequestQueue.LSLHttpRequest();
            lock (instance)
            {
                req.SceneID = instance.Part.ObjectGroup.Scene.ID;
                req.PrimID = instance.Part.ID;
                req.ItemID = instance.Item.ID;
            }

            if (url.Contains(' '))
            {
                lock (instance)
                {
                    HttpResponseEvent e = new HttpResponseEvent();
                    e.RequestID = UUID.Random;
                    e.Status = 499;
                    instance.Part.PostEvent(e);
                    return e.RequestID;
                }
            }

            for (int i = 0; i < parameters.Count; ++i)
            {
                switch(parameters[i].AsInt)
                {
                    case HTTP_METHOD:
                        if(i + 1 >= parameters.Count)
                        {
                            lock(instance)
                            {
                                instance.ShoutError("Missing parameter for HTTP_METHOD");
                                return UUID.Zero;
                            }
                        }

                        req.Method = parameters[++i].ToString();
                        break;

                    case HTTP_MIMETYPE:
                        if(i + 1 >= parameters.Count)
                        {
                            lock(instance)
                            {
                                instance.ShoutError("Missing parameter for HTTP_MIMEYPE");
                                return UUID.Zero;
                            }
                        }

                        req.MimeType = parameters[++i].ToString();
                        break;

                    case HTTP_BODY_MAXLENGTH:
                        if(i + 1 >= parameters.Count)
                        {
                            lock(instance)
                            {
                                instance.ShoutError("Missing parameter for HTTP_METHOD");
                                return UUID.Zero;
                            }
                        }

                        req.MaxBodyLength = parameters[++i].AsInt;
                        break;

                    case HTTP_VERIFY_CERT:
                        if(i + 1 >= parameters.Count)
                        {
                            lock(instance)
                            {
                                instance.ShoutError("Missing parameter for HTTP_VERIFY_CERT");
                                return UUID.Zero;
                            }
                        }

                        req.VerifyCert = parameters[++i].AsBoolean;
                        break;

                    case HTTP_VERBOSE_THROTTLE:
                        if(i + 1 >= parameters.Count)
                        {
                            lock(instance)
                            {
                                instance.ShoutError("Missing parameter for HTTP_VERBOSE_THROTTLE");
                                return UUID.Zero;
                            }
                        }

                        req.VerboseThrottle = parameters[++i].AsBoolean;
                        break;

                    case HTTP_CUSTOM_HEADER:
                        if(i + 2 >= parameters.Count)
                        {
                            lock(instance)
                            {
                                instance.ShoutError("Missing parameter for HTTP_CUSTOM_HEADER");
                                return UUID.Zero;
                            }
                        }

                        string name = parameters[++i].ToString();
                        string value = parameters[++i].ToString();

                        if (!m_AllowedHttpHeaders.Contains(name))
                        {
                            instance.ShoutError(string.Format("Custom header {0} not allowed", name));
                            return UUID.Zero;
                        }
                        try
                        {
                            req.Headers.Add(name, value);
                        }
                        catch
                        {
                            instance.ShoutError(string.Format("Custom header {0} already defined", name));
                            return UUID.Zero;
                        }
                        break;

                    case HTTP_PRAGMA_NO_CACHE:
                        if(i + 1 >= parameters.Count)
                        {
                            lock(instance)
                            {
                                instance.ShoutError("Missing parameter for HTTP_PRAGMA_NO_CACHE");
                                return UUID.Zero;
                            }
                        }

                        req.SendPragmaNoCache = parameters[++i].AsBoolean;
                        break;

                    default:
                        lock(instance)
                        {
                            instance.ShoutError(string.Format("Unknown parameter {0} for llHTTPRequest", parameters[i].AsInt));
                            return UUID.Zero;
                        }
                }
                
            }

            lock (instance)
            {
                req.Headers.Add("X-SecondLife-Object-Name", instance.Part.ObjectGroup.Name);
                req.Headers.Add("X-SecondLife-Object-Key", (string)instance.Part.ObjectGroup.ID);
                req.Headers.Add("X-SecondLife-Region", instance.Part.ObjectGroup.Scene.RegionData.Name);
                req.Headers.Add("X-SecondLife-Local-Position", string.Format("({0:0.000000}, {1:0.000000}, {2:0.000000})", instance.Part.ObjectGroup.GlobalPosition.X, instance.Part.ObjectGroup.GlobalPosition.Y, instance.Part.ObjectGroup.GlobalPosition.Z));
                req.Headers.Add("X-SecondLife-Local-Velocity", string.Format("({0:0.000000}, {1:0.000000}, {2:0.000000})", instance.Part.ObjectGroup.Velocity.X, instance.Part.ObjectGroup.Velocity.Y, instance.Part.ObjectGroup.Velocity.Z));
                req.Headers.Add("X-SecondLife-Local-Rotation", string.Format("({0:0.000000}, {1:0.000000}, {2:0.000000}, {3:0.000000})", instance.Part.ObjectGroup.GlobalRotation.X, instance.Part.ObjectGroup.GlobalRotation.Y, instance.Part.ObjectGroup.GlobalRotation.Z, instance.Part.ObjectGroup.GlobalRotation.W));
                req.Headers.Add("X-SecondLife-Owner-Name", instance.Part.ObjectGroup.Owner.FullName);
                req.Headers.Add("X-SecondLife-Owner-Key", (string)instance.Part.ObjectGroup.Owner.ID);

                Match authMatch = m_AuthRegex.Match(url);
                if(authMatch.Success &&
                    authMatch.Groups.Count == 5)
                {
                    string authData = string.Format("{0}:{1}", authMatch.Groups[2].ToString(), authMatch.Groups[3].ToString());
                    byte[] authDataBinary = authData.ToUTF8String();
                    req.Headers.Add("Authorization", string.Format("Basic {0}", Convert.ToBase64String(authDataBinary)));
                }

                return m_LSLHTTPClient.Enqueue(req) ?
                    req.RequestID :
                    UUID.Zero;
            }
        }
    }
}
