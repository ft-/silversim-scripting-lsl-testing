﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Common;
using SilverSim.Scripting.Lsl.Expression;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace SilverSim.Scripting.Lsl
{
    [CompilerUsesRunAndCollectMode]
    [SuppressMessage("Gendarme.Rules.Performance", "AvoidRepetitiveCallsToPropertiesRule")]
    [SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule", Justification = "Ever seen a compiler source code without such warnings?")]
    [SuppressMessage("Gendarme.Rules.Maintainability", "AvoidComplexMethodsRule", Justification = "Ever seen a compiler source code without such warnings?")]
    public partial class LSLCompiler : IScriptCompiler, IPlugin, IPluginSubFactory
    {
        internal struct ApiMethodInfo
        {
            public string FunctionName;
            public IScriptApi Api;
            public MethodInfo Method;

            public ApiMethodInfo(string functionName, IScriptApi api, MethodInfo method)
            {
                FunctionName = functionName;
                Api = api;
                Method = method;
            }
        }

        internal sealed class ApiInfo
        {
            public Dictionary<string, List<ApiMethodInfo>> Methods = new Dictionary<string, List<ApiMethodInfo>>();
            public Dictionary<string, FieldInfo> Constants = new Dictionary<string, FieldInfo>();
            public Dictionary<string, MethodInfo> EventDelegates = new Dictionary<string, MethodInfo>();

            public ApiInfo()
            {

            }

            public void Add(ApiInfo info)
            {
                foreach(KeyValuePair<string, List<ApiMethodInfo>> mInfo in info.Methods)
                {
                    if(!Methods.ContainsKey(mInfo.Key))
                    {
                        Methods[mInfo.Key] = new List<ApiMethodInfo>(mInfo.Value);
                    }
                    else
                    {
                        Methods[mInfo.Key].AddRange(mInfo.Value);
                    }
                }
                foreach(KeyValuePair<string, FieldInfo> kvp in info.Constants)
                {
                    Constants.Add(kvp.Key, kvp.Value);
                }
                foreach(KeyValuePair<string, MethodInfo> kvp in info.EventDelegates)
                {
                    EventDelegates.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private static readonly ILog m_Log = LogManager.GetLogger("LSL COMPILER");
        internal List<IScriptApi> m_Apis = new List<IScriptApi>();

        internal Dictionary<APIFlags, ApiInfo> m_ApiInfos = new Dictionary<APIFlags, ApiInfo>();
        internal Dictionary<string, ApiInfo> m_ApiExtensions = new Dictionary<string, ApiInfo>();

        readonly List<Action<ScriptInstance>> m_StateChangeDelegates = new List<Action<ScriptInstance>>();
        readonly List<Action<ScriptInstance>> m_ScriptResetDelegates = new List<Action<ScriptInstance>>();
        readonly List<string> m_ReservedWords = new List<string>();
        readonly List<string> m_Typecasts = new List<string>();
        readonly List<char> m_SingleOps = new List<char>();
        readonly List<char> m_MultiOps = new List<char>();
        readonly List<char> m_NumericChars = new List<char>();
        readonly List<char> m_OpChars = new List<char>();

        public enum OperatorType
        {
            Unknown,
            RightUnary,
            LeftUnary,
            Binary
        }

        public LSLCompiler()
        {
            m_ApiInfos.Add(APIFlags.ASSL, new ApiInfo());
            m_ApiInfos.Add(APIFlags.LSL, new ApiInfo());
            m_ApiInfos.Add(APIFlags.OSSL, new ApiInfo());

            m_ReservedWords.Add("integer");
            m_ReservedWords.Add("vector");
            m_ReservedWords.Add("list");
            m_ReservedWords.Add("float");
            m_ReservedWords.Add("string");
            m_ReservedWords.Add("key");
            m_ReservedWords.Add("rotation");
            m_ReservedWords.Add("if");
            m_ReservedWords.Add("while");
            m_ReservedWords.Add("jump");
            m_ReservedWords.Add("for");
            m_ReservedWords.Add("do");
            m_ReservedWords.Add("return");
            m_ReservedWords.Add("state");
            m_ReservedWords.Add("void");
            m_ReservedWords.Add("quaternion");

            m_Typecasts.Add("integer");
            m_Typecasts.Add("vector");
            m_Typecasts.Add("list");
            m_Typecasts.Add("float");
            m_Typecasts.Add("string");
            m_Typecasts.Add("key");
            m_Typecasts.Add("rotation");
            m_Typecasts.Add("quaternion");

            m_MultiOps.Add('+');
            m_MultiOps.Add('-');
            m_MultiOps.Add('*');
            m_MultiOps.Add('/');
            m_MultiOps.Add('%');
            m_MultiOps.Add('<');
            m_MultiOps.Add('>');
            m_MultiOps.Add('=');
            m_MultiOps.Add('&');
            m_MultiOps.Add('|');
            m_MultiOps.Add('^');
            m_MultiOps.Add('!');

            m_SingleOps.Add('~');
            m_SingleOps.Add('.');
            m_SingleOps.Add(',');
            m_SingleOps.Add('@');

            m_NumericChars.Add('.');
            m_NumericChars.Add('A');
            m_NumericChars.Add('B');
            m_NumericChars.Add('C');
            m_NumericChars.Add('D');
            m_NumericChars.Add('E');
            m_NumericChars.Add('F');
            m_NumericChars.Add('a');
            m_NumericChars.Add('b');
            m_NumericChars.Add('c');
            m_NumericChars.Add('d');
            m_NumericChars.Add('e');
            m_NumericChars.Add('f');
            m_NumericChars.Add('x');
            m_NumericChars.Add('+');
            m_NumericChars.Add('-');

            m_OpChars = new List<char>();
            m_OpChars.AddRange(m_MultiOps);
            m_OpChars.AddRange(m_SingleOps);
        }

        public void AddPlugins(ConfigurationLoader loader)
        {
            loader.AddPlugin("LSLHTTP", new LSLHTTP());
            loader.AddPlugin("LSLHttpClient", new LSLHTTPClient_RequestQueue());
            Type[] types = GetType().Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (typeof(IScriptApi).IsAssignableFrom(type))
                {
                    ScriptApiNameAttribute scriptApiAttr = Attribute.GetCustomAttribute(type, typeof(ScriptApiNameAttribute)) as ScriptApiNameAttribute;
                    Attribute impTagAttr = Attribute.GetCustomAttribute(type, typeof(LSLImplementationAttribute));
                    if (null != impTagAttr && null != scriptApiAttr)
                    {
                        IPlugin factory = (IPlugin)Activator.CreateInstance(type);
                        loader.AddPlugin("LSL_API_" + scriptApiAttr.Name, factory);
                    }
                }
            }
        }

        public void Startup(ConfigurationLoader loader)
        {
            List<IScriptApi> apis = loader.GetServicesByValue<IScriptApi>();
            foreach (IScriptApi api in apis)
            {
                Type apiType = api.GetType();
                Attribute attr = Attribute.GetCustomAttribute(apiType, typeof(LSLImplementationAttribute));
                if (attr != null && !m_Apis.Contains(api))
                {
                    if ((apiType.Attributes & TypeAttributes.Public) == 0)
                    {
                        m_Log.FatalFormat("LSLImplementation derived {0} is not set to public", apiType.FullName);
                    }
                    else
                    {
                        m_Apis.Add(api);
                    }
                }
            }

            #region API Collection
            foreach (IScriptApi api in apis)
            {
                #region Collect constants
                foreach (FieldInfo f in api.GetType().GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if ((f.Attributes & FieldAttributes.Static) != 0 &&
                        ((f.Attributes & FieldAttributes.InitOnly) != 0 || (f.Attributes & FieldAttributes.Literal) != 0))
                    {
                        if (IsValidType(f.FieldType))
                        {
                            APILevelAttribute[] apiLevelAttrs = System.Attribute.GetCustomAttributes(f, typeof(APILevelAttribute)) as APILevelAttribute[];
                            APIExtensionAttribute[] apiExtensionAttrs = System.Attribute.GetCustomAttributes(f, typeof(APIExtensionAttribute)) as APIExtensionAttribute[];
                            if (apiLevelAttrs.Length != 0 || apiExtensionAttrs.Length != 0)
                            {
                                foreach (APILevelAttribute attr in apiLevelAttrs)
                                {
                                    string constName = attr.Name;
                                    if (string.IsNullOrEmpty(constName))
                                    {
                                        constName = f.Name;
                                    }
                                    foreach (KeyValuePair<APIFlags, ApiInfo> kvp in m_ApiInfos)
                                    {
                                        if ((kvp.Key & attr.Flags) != 0)
                                        {
                                            kvp.Value.Constants.Add(constName, f);
                                        }
                                    }
                                }
                                foreach (APIExtensionAttribute attr in apiExtensionAttrs)
                                {
                                    string constName = attr.Name;
                                    if (string.IsNullOrEmpty(constName))
                                    {
                                        constName = f.Name;
                                    }

                                    ApiInfo apiInfo;
                                    string extensionName = attr.Extension.ToLower();
                                    if (!m_ApiExtensions.TryGetValue(extensionName, out apiInfo))
                                    {
                                        apiInfo = new ApiInfo();
                                        m_ApiExtensions.Add(extensionName, apiInfo);
                                    }
                                    apiInfo.Constants.Add(constName, f);
                                }
                            }
                        }
                        else
                        {
                            APILevelAttribute[] apiLevelAttrs = System.Attribute.GetCustomAttributes(f, typeof(APILevelAttribute)) as APILevelAttribute[];
                            APIExtensionAttribute[] apiExtensionAttrs = System.Attribute.GetCustomAttributes(f, typeof(APIExtensionAttribute)) as APIExtensionAttribute[];
                            if (apiLevelAttrs.Length != 0 || apiExtensionAttrs.Length != 0)
                            {
                                m_Log.DebugFormat("Field {0} has unsupported attribute flags {1}", f.Name, f.Attributes.ToString());
                            }
                        }
                    }
                }
                #endregion

                #region Collect event definitions
                foreach (Type t in api.GetType().GetNestedTypes(BindingFlags.Public).Where(t => t.BaseType == typeof(MulticastDelegate)))
                {
                    StateEventDelegateAttribute stateEventAttr = (StateEventDelegateAttribute)Attribute.GetCustomAttribute(t, typeof(StateEventDelegateAttribute));
                    if (stateEventAttr != null)
                    {
                        APILevelAttribute[] apiLevelAttrs = System.Attribute.GetCustomAttributes(t, typeof(APILevelAttribute)) as APILevelAttribute[];
                        APIExtensionAttribute[] apiExtensionAttrs = System.Attribute.GetCustomAttributes(t, typeof(APIExtensionAttribute)) as APIExtensionAttribute[];
                        MethodInfo mi = t.GetMethod("Invoke");
                        if (apiLevelAttrs.Length == 0 && apiExtensionAttrs.Length == 0)
                        {
                            m_Log.DebugFormat("Invalid delegate '{0}' in '{1}' has StateEventDelegate attribute. APILevel or APIExtension attribute missing.",
                                mi.Name,
                                mi.DeclaringType.FullName);
                        }

                        /* validate parameters */
                        ParameterInfo[] pi = mi.GetParameters();
                        bool eventValid = true;

                        for (int i = 0; i < pi.Length; ++i)
                        {
                            if (!IsValidType(pi[i].ParameterType))
                            {
                                eventValid = false;
                                m_Log.DebugFormat("Invalid delegate '{0}' in '{1}' has APILevel attribute. Parameter '{2}' does not have LSL compatible type '{3}'.",
                                    mi.Name,
                                    mi.DeclaringType.FullName,
                                    pi[i].Name,
                                    pi[i].ParameterType.FullName);
                            }
                        }
                        if (mi.ReturnType != typeof(void))
                        {
                            eventValid = false;
                            m_Log.DebugFormat("Invalid delegate '{0}' in '{1}' has APILevel attribute. Return value is not void. Found: '{2}'",
                                mi.Name,
                                mi.DeclaringType.FullName,
                                mi.ReturnType.FullName);
                        }

                        if (eventValid)
                        {
                            foreach (APILevelAttribute apiLevelAttr in apiLevelAttrs)
                            {
                                string funcName = apiLevelAttr.Name;
                                if(string.IsNullOrEmpty(funcName))
                                {
                                    funcName = mi.Name;
                                }

                                foreach (KeyValuePair<APIFlags, ApiInfo> kvp in m_ApiInfos)
                                {
                                    if ((kvp.Key & apiLevelAttr.Flags) != 0)
                                    {
                                        kvp.Value.EventDelegates.Add(funcName, mi);
                                    }
                                }
                            }
                            foreach(APIExtensionAttribute apiExtensionAttr in apiExtensionAttrs)
                            {
                                string funcName = apiExtensionAttr.Name;
                                if (string.IsNullOrEmpty(funcName))
                                {
                                    funcName = mi.Name;
                                }

                                ApiInfo apiInfo;
                                string extensionName = apiExtensionAttr.Extension.ToLower();
                                if (!m_ApiExtensions.TryGetValue(extensionName, out apiInfo))
                                {
                                    apiInfo = new ApiInfo();
                                    m_ApiExtensions.Add(extensionName, apiInfo);
                                }
                                apiInfo.EventDelegates.Add(funcName, mi);
                            }
                        }
                    }
                    else if (null != stateEventAttr)
                    {
                        MethodInfo mi = t.GetMethod("Invoke");
                        m_Log.DebugFormat("Invalid delegate '{0}' in '{1}' has APILevel attribute. APILevel attribute missing.",
                            mi.Name,
                            mi.DeclaringType.FullName);
                    }
                }
                #endregion

                #region Collect API functions, reset delegates and state change delegates
                foreach (MethodInfo m in api.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
                {
                    APILevelAttribute[] funcNameAttrs = Attribute.GetCustomAttributes(m, typeof(APILevelAttribute)) as APILevelAttribute[];
                    APIExtensionAttribute[] apiExtensionAttrs = Attribute.GetCustomAttributes(m, typeof(APIExtensionAttribute)) as APIExtensionAttribute[];
                    if (funcNameAttrs.Length != 0 || apiExtensionAttrs.Length != 0)
                    {
                        ParameterInfo[] pi = m.GetParameters();
                        if (pi.Length >= 1 &&
                            pi[0].ParameterType.Equals(typeof(ScriptInstance)))
                        {
                            /* validate parameters */
                            bool methodValid = true;
                            if((m.Attributes & MethodAttributes.Static) != 0)
                            {
                                methodValid = false;
                                m_Log.DebugFormat("Invalid method '{0}' in '{1}' has APILevel or APIExtension attribute. Method is declared static.",
                                    m.Name,
                                    m.DeclaringType.FullName);
                            }
                            for (int i = 1; i < pi.Length; ++i)
                            {
                                if (!IsValidType(pi[i].ParameterType))
                                {
                                    methodValid = false;
                                    m_Log.DebugFormat("Invalid method '{0}' in '{1}' has APILevel or APIExtension  attribute. Parameter '{2}' does not have LSL compatible type '{3}'.",
                                        m.Name,
                                        m.DeclaringType.FullName,
                                        pi[i].Name,
                                        pi[i].ParameterType.FullName);
                                }
                            }
                            if (!IsValidType(m.ReturnType))
                            {
                                methodValid = false;
                                m_Log.DebugFormat("Invalid method '{0}' in '{1}' has APILevel or APIExtension  attribute. Return value does not have LSL compatible type '{2}'.",
                                    m.Name,
                                    m.DeclaringType.FullName,
                                    m.ReturnType.FullName);
                            }

                            if (methodValid)
                            {
                                foreach (APILevelAttribute funcNameAttr in funcNameAttrs)
                                {
                                    string funcName = funcNameAttr.Name;
                                    if (string.IsNullOrEmpty(funcName))
                                    {
                                        funcName = m.Name;
                                    }
                                    foreach (KeyValuePair<APIFlags, ApiInfo> kvp in m_ApiInfos)
                                    {
                                        List<ApiMethodInfo> methodList;
                                        if (!kvp.Value.Methods.TryGetValue(funcName, out methodList))
                                        {
                                            methodList = new List<ApiMethodInfo>();
                                            kvp.Value.Methods.Add(funcName, methodList);
                                        }
                                        methodList.Add(new ApiMethodInfo(funcName, api, m));
                                    }
                                }
                                foreach (APIExtensionAttribute funcNameAttr in apiExtensionAttrs)
                                {
                                    string funcName = funcNameAttr.Name;
                                    if (string.IsNullOrEmpty(funcName))
                                    {
                                        funcName = m.Name;
                                    }

                                    ApiInfo apiInfo;
                                    string extensionName = funcNameAttr.Extension.ToLower();
                                    if (!m_ApiExtensions.TryGetValue(extensionName, out apiInfo))
                                    {
                                        apiInfo = new ApiInfo();
                                        m_ApiExtensions.Add(extensionName, apiInfo);
                                    }
                                    List<ApiMethodInfo> methodList;
                                    if (!apiInfo.Methods.TryGetValue(funcName, out methodList))
                                    {
                                        methodList = new List<ApiMethodInfo>();
                                        apiInfo.Methods.Add(funcName, methodList);
                                    }
                                    methodList.Add(new ApiMethodInfo(funcName, api, m));
                                }
                            }
                        }
                    }

                    Attribute attr = Attribute.GetCustomAttribute(m, typeof(ExecutedOnStateChangeAttribute));
                    if (attr != null && (m.Attributes & MethodAttributes.Static) != 0)
                    {
                        ParameterInfo[] pi = m.GetParameters();
                        if(pi.Length != 1)
                        {
                            m_Log.DebugFormat("Invalid method '{0}' in '{1}' has attribute ExecutedOnStateChange. Parameter count does not match.",
                                m.Name,
                                m.DeclaringType.FullName);
                        }
                        else if (m.ReturnType != typeof(void))
                        {
                            m_Log.DebugFormat("Invalid method '{0}' in '{1}' has attribute ExecutedOnStateChange. Return type is not void.",
                                m.Name,
                                m.DeclaringType.FullName);
                        }
                        else if(pi[0].ParameterType != typeof(ScriptInstance))
                        {
                            m_Log.DebugFormat("Invalid method '{0}' in '{1}' has attribute ExecutedOnStateChange. Parameter type is not ScriptInstance.",
                                m.Name,
                                m.DeclaringType.FullName);
                        }
                        else
                        {
                            m_StateChangeDelegates.Add((Action<ScriptInstance>)Delegate.CreateDelegate(typeof(Action<ScriptInstance>), null, m));
                        }
                    }

                    attr = Attribute.GetCustomAttribute(m, typeof(ExecutedOnScriptResetAttribute));
                    if (attr != null && (m.Attributes & MethodAttributes.Static) != 0)
                    {
                        ParameterInfo[] pi = m.GetParameters();
                        if(pi.Length != 1)
                        {
                            m_Log.DebugFormat("Invalid method '{0}' in '{1}' has attribute ExecutedOnScriptReset. Parameter count does not match.",
                                m.Name,
                                m.DeclaringType.FullName);
                        }
                        else if (m.ReturnType != typeof(void))
                        {
                            m_Log.DebugFormat("Invalid method '{0}' in '{1}' has attribute ExecutedOnScriptReset. Return type is not void.",
                                m.Name,
                                m.DeclaringType.FullName);
                        }
                        else if(pi[0].ParameterType != typeof(ScriptInstance))
                        {
                            m_Log.DebugFormat("Invalid method '{0}' in '{1}' has attribute ExecutedOnScriptReset. Parameter type is not ScriptInstance.",
                                m.Name,
                                m.DeclaringType.FullName);
                        }
                        else
                        {
                            m_ScriptResetDelegates.Add((Action<ScriptInstance>)Delegate.CreateDelegate(typeof(Action<ScriptInstance>), null, m));
                        }
                    }
                }
                #endregion
            }
            #endregion

#if OLD_RESOLVER
            List<Dictionary<string, OperatorType>> m_Operators = new List<Dictionary<string, OperatorType>>();

            Dictionary<string, OperatorType> plist;

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("@", OperatorType.LeftUnary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("++", OperatorType.RightUnary);
            plist.Add("--", OperatorType.RightUnary);
            plist.Add(".", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("++", OperatorType.LeftUnary);
            plist.Add("--", OperatorType.LeftUnary);
            plist.Add("+", OperatorType.LeftUnary);
            plist.Add("-", OperatorType.LeftUnary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("(integer)", OperatorType.LeftUnary);
            plist.Add("(float)", OperatorType.LeftUnary);
            plist.Add("(string)", OperatorType.LeftUnary);
            plist.Add("(list)", OperatorType.LeftUnary);
            plist.Add("(key)", OperatorType.LeftUnary);
            plist.Add("(vector)", OperatorType.LeftUnary);
            plist.Add("(rotation)", OperatorType.LeftUnary);
            plist.Add("(quaternion)", OperatorType.LeftUnary);
            plist.Add("!", OperatorType.LeftUnary);
            plist.Add("~", OperatorType.LeftUnary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("*", OperatorType.Binary);
            plist.Add("/", OperatorType.Binary);
            plist.Add("%", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("+", OperatorType.Binary);
            plist.Add("-", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("<<", OperatorType.Binary);
            plist.Add(">>", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("<", OperatorType.Binary);
            plist.Add("<=", OperatorType.Binary);
            plist.Add(">", OperatorType.Binary);
            plist.Add(">=", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("==", OperatorType.Binary);
            plist.Add("!=", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("&", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("^", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("|", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("&&", OperatorType.Binary);
            plist.Add("||", OperatorType.Binary);

            plist = new Dictionary<string, OperatorType>();
            m_Operators.Add(plist);
            plist.Add("=", OperatorType.Binary);
            plist.Add("+=", OperatorType.Binary);
            plist.Add("-=", OperatorType.Binary);
            plist.Add("*=", OperatorType.Binary);
            plist.Add("/=", OperatorType.Binary);
            plist.Add("%=", OperatorType.Binary);

            Dictionary<string, string> blockOps = new Dictionary<string, string>();
            blockOps.Add("(", ")");
            blockOps.Add("[", "]");

            m_Resolver = new Resolver(m_ReservedWords, operators, blockOps);
#endif
            GenerateLSLSyntaxFile();

            SilverSim.Scripting.Common.CompilerRegistry.ScriptCompilers["lsl"] = this;
            SilverSim.Scripting.Common.CompilerRegistry.ScriptCompilers["XEngine"] = this; /* we won't be supporting anything beyond LSL compatibility */
        }

        public void SyntaxCheck(UUI user, Dictionary<int, string> shbangs, UUID assetID, TextReader reader, int linenumber = 1)
        {
            Preprocess(user, shbangs, reader, linenumber);
        }

        void WriteIndent(TextWriter writer, int indent)
        {
            while(indent-- > 0)
            {
                writer.Write("    ");
            }
        }

        void WriteIndented(TextWriter writer, string s, ref int oldIndent)
        {
            if (s == "[")
            {
                writer.WriteLine("\n");
                ++oldIndent;
                WriteIndent(writer, oldIndent);
                writer.WriteLine(s);
                WriteIndent(writer, oldIndent);
            }
            else if (s == "{")
            {
                writer.WriteLine("\n");
                WriteIndent(writer, oldIndent);
                writer.WriteLine(s);
                ++oldIndent;
                WriteIndent(writer, oldIndent);
            }
            else if (s == "]")
            {
                writer.WriteLine("\n");
                WriteIndent(writer, oldIndent);
                writer.WriteLine(s);
                --oldIndent;
                WriteIndent(writer, oldIndent);
            }
            else if ( s == "}")
            {
                --oldIndent;
                writer.WriteLine("\n");
                WriteIndent(writer, oldIndent);
                writer.WriteLine(s);
                WriteIndent(writer, oldIndent);
            }
            else if(s == "\n")
            {
                writer.WriteLine("\n");
                WriteIndent(writer, oldIndent);
            }
            else if(s == ";")
            {
                writer.WriteLine(";");
                WriteIndent(writer, oldIndent);
            }
            else
            {
                writer.Write(s + " ");
            }
        }

        void WriteIndented(TextWriter writer, List<string> list, ref int oldIndent)
        {
            foreach(string s in list)
            {
                WriteIndented(writer, s, ref oldIndent);
            }
        }

        public void SyntaxCheckAndDump(Stream s, UUI user, Dictionary<int, string> shbangs, UUID assetID, TextReader reader, int linenumber = 1)
        {
            CompileState cs = Preprocess(user, shbangs, reader, linenumber);
            /* rewrite script */
            int indent = 0;
            using (TextWriter writer = new StreamWriter(s, new UTF8Encoding(false)))
            {
                #region Write Variables
                foreach (KeyValuePair<string, Type> kvp in cs.m_VariableDeclarations)
                {
                    LineInfo li;
                    WriteIndented(writer, MapType(kvp.Value), ref indent);
                    WriteIndented(writer, kvp.Key, ref indent);
                    if (cs.m_VariableInitValues.TryGetValue(kvp.Key, out li))
                    {
                        WriteIndented(writer, "=", ref indent);
                        WriteIndented(writer, li.Line, ref indent);
                    }
                    WriteIndented(writer, ";", ref indent);
                }
                WriteIndented(writer, "\n", ref indent);
                #endregion

                #region Write functions
                foreach(KeyValuePair<string, List<LineInfo>> kvp in cs.m_Functions)
                {
                    foreach(LineInfo li in kvp.Value)
                    {
                        WriteIndented(writer, li.Line, ref indent);
                        if (li.Line[li.Line.Count - 1] != "{" && li.Line[li.Line.Count - 1] != ";" && li.Line[li.Line.Count - 1] != "}")
                        {
                            ++indent;
                            WriteIndented(writer, "\n", ref indent);
                            --indent;
                        }
                    }
                }
                #endregion

                #region Write states
                foreach (KeyValuePair<string, Dictionary<string, List<LineInfo>>> kvp in cs.m_States)
                {
                    if (kvp.Key != "default")
                    {
                        WriteIndented(writer, "state", ref indent);
                    }
                    WriteIndented(writer, kvp.Key, ref indent);
                    WriteIndented(writer, "{", ref indent);

                    foreach (KeyValuePair<string, List<LineInfo>> eventfn in kvp.Value)
                    {
                        int tempindent = 0;
                        foreach (LineInfo li in eventfn.Value)
                        {
                            WriteIndented(writer, li.Line, ref indent);
                            if (li.Line[li.Line.Count - 1] != "{" && li.Line[li.Line.Count - 1] != ";" && li.Line[li.Line.Count - 1] != "}")
                            {
                                ++tempindent;
                                indent += tempindent;
                                WriteIndented(writer, "\n", ref indent);
                                indent -= tempindent;
                            }
                            else
                            {
                                tempindent = 0;
                            }
                        }
                    }
                    WriteIndented(writer, "}", ref indent);
                }
                #endregion
                writer.Flush();
            }
        }

        public IScriptAssembly Compile(AppDomain appDom, UUI user, Dictionary<int, string> shbangs, UUID assetID, TextReader reader, int lineNumber = 1)
        {
            CompileState compileState = Preprocess(user, shbangs, reader, lineNumber);
            return PostProcess(compileState, appDom, assetID, compileState.ForcedSleepDefault);
        }
    }
}
