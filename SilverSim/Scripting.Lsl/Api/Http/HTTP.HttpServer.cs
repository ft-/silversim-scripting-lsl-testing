﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Types;

namespace SilverSim.Scripting.Lsl.Api.Http
{
    public partial class HttpApi
    {
        [APILevel(APIFlags.LSL, "llGetFreeURLs")]
        public int GetFreeURLs(ScriptInstance instance)
        {
            lock(instance)
            {
                int freeurls = m_HTTPHandler.FreeUrls;
                if(freeurls < 0)
                {
                    freeurls = 0;
                }

                return freeurls;
            }
        }

        [APILevel(APIFlags.LSL, "llRequestURL")]
        public LSLKey RequestURL(ScriptInstance instance)
        {
            lock(instance)
            {
                UUID reqID = UUID.Random;
                try
                {
                    string urlID = m_HTTPHandler.RequestURL(instance.Part, instance.Item);
                    HttpRequestEvent ev = new HttpRequestEvent();
                    ev.RequestID = reqID;
                    ev.Method = URL_REQUEST_GRANTED;
                    ev.Body = urlID;
                    instance.PostEvent(ev);
                }
                catch
                {
                    HttpRequestEvent ev = new HttpRequestEvent();
                    ev.RequestID = reqID;
                    ev.Method = URL_REQUEST_DENIED;
                    ev.Body = string.Empty;
                    instance.PostEvent(ev);
                }
                return reqID;
            }
        }

        [APILevel(APIFlags.LSL, "llReleaseURL")]
        public void ReleaseURL(ScriptInstance instance, string url)
        {
            lock (instance)
            {
                m_HTTPHandler.ReleaseURL(url);
            }
        }

        [APILevel(APIFlags.LSL, "llRequestSecureURL")]
        public LSLKey RequestSecureURL(ScriptInstance instance)
        {
            lock (instance)
            {
                UUID reqID = UUID.Random;
                try
                {
                    string urlID = m_HTTPHandler.RequestSecureURL(instance.Part, instance.Item);
                    HttpRequestEvent ev = new HttpRequestEvent();
                    ev.RequestID = reqID;
                    ev.Method = URL_REQUEST_GRANTED;
                    ev.Body = urlID;
                    instance.PostEvent(ev);
                }
                catch
                {
                    HttpRequestEvent ev = new HttpRequestEvent();
                    ev.RequestID = reqID;
                    ev.Method = URL_REQUEST_DENIED;
                    ev.Body = string.Empty;
                    instance.PostEvent(ev);
                }
                return reqID;
            }
        }

        [APILevel(APIFlags.LSL, "llGetHTTPHeader")]
        public string GetHTTPHeader(ScriptInstance instance, LSLKey requestID, string header)
        {
            lock (instance)
            {
                return m_HTTPHandler.GetHttpHeader(requestID, header);
            }
        }

        [APILevel(APIFlags.LSL, "llHTTPResponse")]
        public void HTTPResponse(ScriptInstance instance, LSLKey requestID, int status, string body)
        {
            lock(instance)
            {
                m_HTTPHandler.HttpResponse(requestID, status, body);
            }
        }

        [APILevel(APIFlags.LSL)]
        public const int CONTENT_TYPE_TEXT = 0;
        [APILevel(APIFlags.LSL)]
        public const int CONTENT_TYPE_HTML = 1;
        [APILevel(APIFlags.LSL)]
        public const int CONTENT_TYPE_XML = 2;
        [APILevel(APIFlags.LSL)]
        public const int CONTENT_TYPE_XHTML = 3;
        [APILevel(APIFlags.LSL)]
        public const int CONTENT_TYPE_ATOM = 4;
        [APILevel(APIFlags.LSL)]
        public const int CONTENT_TYPE_JSON = 5;
        [APILevel(APIFlags.LSL)]
        public const int CONTENT_TYPE_LLSD = 6;
        [APILevel(APIFlags.LSL)]
        public const int CONTENT_TYPE_FORM = 7;
        [APILevel(APIFlags.LSL)]
        public const int CONTENT_TYPE_RSS = 8;

        [APILevel(APIFlags.LSL, "llSetContentType")]
        public void SetContentType(ScriptInstance instance, LSLKey requestID, int contenttype)
        {
            lock(instance)
            {
                switch(contenttype)
                {
                    case CONTENT_TYPE_TEXT: m_HTTPHandler.SetContentType(requestID, "text/plain"); break;
                    case CONTENT_TYPE_HTML: m_HTTPHandler.SetContentType(requestID, "text/html"); break;
                    case CONTENT_TYPE_XML: m_HTTPHandler.SetContentType(requestID, "application/xml"); break;
                    case CONTENT_TYPE_XHTML: m_HTTPHandler.SetContentType(requestID, "application/xhtml+xml"); break;
                    case CONTENT_TYPE_ATOM: m_HTTPHandler.SetContentType(requestID, "application/atom+xml"); break;
                    case CONTENT_TYPE_JSON: m_HTTPHandler.SetContentType(requestID, "application/json"); break;
                    case CONTENT_TYPE_LLSD: m_HTTPHandler.SetContentType(requestID, "application/llsd+xml"); break;
                    case CONTENT_TYPE_FORM: m_HTTPHandler.SetContentType(requestID, "application/x-www-form-urlencoded "); break;
                    case CONTENT_TYPE_RSS: m_HTTPHandler.SetContentType(requestID, "application/rss+xml "); break;
                    default: m_HTTPHandler.SetContentType(requestID, "text/plain"); break;
                }
            }
        }

        [APILevel(APIFlags.OSSL, "osSetContentType")]
        public void SetContentType(ScriptInstance instance, LSLKey id, string type)
        {
            lock(instance)
            {
                m_HTTPHandler.SetContentType(id, type);
            }
        }
    }
}
