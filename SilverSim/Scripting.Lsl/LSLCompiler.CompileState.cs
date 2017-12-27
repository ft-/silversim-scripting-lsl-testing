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

#pragma warning disable IDE0018, RCS1029, IDE0019

using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Reflection.Emit;

namespace SilverSim.Scripting.Lsl
{
    public partial class LSLCompiler
    {
        sealed internal class LineInfo
        {
            public readonly List<string> Line;
            public readonly int LineNumber;

            public LineInfo(List<string> line, int lineNo)
            {
                Line = line;
                LineNumber = lineNo;
            }

            public override string ToString() => LineNumber.ToString() + ": " + string.Join(" ", Line);
        }

        internal class FunctionInfo
        {
            internal List<FuncParamInfo> FunctionParameters = new List<FuncParamInfo>();
            internal List<LineInfo> FunctionLines = new List<LineInfo>();

            /* following two are setup later */
            internal Type ReturnType;
            internal KeyValuePair<string, Type>[] Parameters;
            internal MethodBuilder Method;

            public FunctionInfo(List<FuncParamInfo> parameters, List<LineInfo> lines)
            {
                FunctionParameters = parameters;
                FunctionLines = lines;
            }
        }

        internal class BreakContinueLabel
        {
            public Label ContinueTargetLabel;
            public Label BreakTargetLabel;
            public Label DefaultLabel;
            public Label NextCaseLabel;
            public LocalBuilder SwitchValueLocal;
            public bool CaseRequired;
            public bool HaveDefaultCase;
            public bool HaveContinueTarget;
            public bool HaveBreakTarget;

            public BreakContinueLabel()
            {
            }

            public BreakContinueLabel(BreakContinueLabel bc)
            {
                ContinueTargetLabel = bc.ContinueTargetLabel;
                BreakTargetLabel = bc.BreakTargetLabel;
                HaveBreakTarget = bc.HaveBreakTarget;
                HaveContinueTarget = bc.HaveContinueTarget;
            }
        }

        sealed internal class CompileState
        {
            public ApiInfo ApiInfo = new ApiInfo();
            public readonly CultureInfo CurrentCulture;
            public bool ForcedSleepDefault;
            public bool EmitDebugSymbols;
            public Dictionary<string, Type> m_VariableDeclarations = new Dictionary<string, Type>();
            public Dictionary<string, Dictionary<string, Type>> m_StateVariableDeclarations = new Dictionary<string, Dictionary<string, Type>>();
            public Dictionary<string, FieldBuilder> m_VariableFieldInfo = new Dictionary<string, FieldBuilder>();
            public Dictionary<string, Dictionary<string, FieldBuilder>> m_StateVariableFieldInfo = new Dictionary<string, Dictionary<string, FieldBuilder>>();
            public Dictionary<string, LineInfo> m_VariableInitValues = new Dictionary<string, LineInfo>();
            public Dictionary<string, Dictionary<string, LineInfo>> m_StateVariableInitValues = new Dictionary<string, Dictionary<string, LineInfo>>();
            public List<List<string>> m_LocalVariables = new List<List<string>>();
            public Dictionary<string, List<FunctionInfo>> m_Functions = new Dictionary<string, List<FunctionInfo>>();
            public Dictionary<string, List<FunctionInfo>> m_MemberFunctions = new Dictionary<string, List<FunctionInfo>>();
            public Dictionary<string, Dictionary<string, List<LineInfo>>> m_States = new Dictionary<string, Dictionary<string, List<LineInfo>>>();
            public Dictionary<string, FieldBuilder> m_ApiFieldInfo = new Dictionary<string, FieldBuilder>();
            public List<BreakContinueLabel> m_BreakContinueLabels = new List<BreakContinueLabel>();

            public TypeBuilder ScriptTypeBuilder;
            public TypeBuilder StateTypeBuilder;
            public FieldBuilder InstanceField;
            public ILGenDumpProxy ILGen;
            public ISymbolDocumentWriter DebugDocument;

            public List<LineInfo> FunctionBody;
            public int FunctionLineIndex;

            public class LanguageExtensionsData
            {
                public bool EnableExtendedTypecasts;
                /** <summary>Enables a rather unknown function overloading support that happen to exist within OpenSim's XEngine</summary> */
                public bool EnableFunctionOverloading = true;

                public bool EnableStateVariables;

                public bool EnableSwitchBlock;

                public bool EnableBreakContinueStatement;

                public bool EnableLongIntegers;

                public bool EnableArrayThisOperator;

                public bool EnableMemberFunctions;

                public bool EnableProperties;
            }

            public bool UsesSinglePrecision;

            public readonly LanguageExtensionsData LanguageExtensions = new LanguageExtensionsData();

            public CompileState(CultureInfo currentCulture)
            {
                CurrentCulture = currentCulture;
            }

            private readonly Dictionary<Type, string> m_ReverseTypeDeclarations = new Dictionary<Type, string>();
            private readonly Dictionary<string, Type> m_ValidVarTypes = new Dictionary<string, Type>();

            public void FinalizeTypeList()
            {
                /* Built-in types get added last */
                if (LanguageExtensions.EnableLongIntegers)
                {
                    ApiInfo.Types["long"] = typeof(long);
                }
                else
                {
                    ApiInfo.Types.Remove("long");
                }

                foreach(KeyValuePair<string, Type> kvp in ApiInfo.Types)
                {
                    if(Attribute.GetCustomAttribute(kvp.Value, typeof(APIIsVariableTypeAttribute)) != null)
                    {
                        m_ValidVarTypes.Add(kvp.Key, kvp.Value);
                    }
                }

                ApiInfo.Types["integer"] = typeof(int);
                ApiInfo.Types["quaternion"] = typeof(Quaternion);
                ApiInfo.Types["rotation"] = typeof(Quaternion);
                ApiInfo.Types["key"] = typeof(LSLKey);
                ApiInfo.Types["string"] = typeof(string);
                ApiInfo.Types["float"] = typeof(double);
                ApiInfo.Types["list"] = typeof(AnArray);
                ApiInfo.Types["vector"] = typeof(Vector3);
                ApiInfo.Types["void"] = typeof(void);

                m_ValidVarTypes["integer"] = typeof(int);
                m_ValidVarTypes["quaternion"] = typeof(Quaternion);
                m_ValidVarTypes["rotation"] = typeof(Quaternion);
                m_ValidVarTypes["key"] = typeof(LSLKey);
                m_ValidVarTypes["string"] = typeof(string);
                m_ValidVarTypes["float"] = typeof(double);
                m_ValidVarTypes["list"] = typeof(AnArray);
                m_ValidVarTypes["vector"] = typeof(Vector3);

                if(LanguageExtensions.EnableLongIntegers)
                {
                    m_ValidVarTypes["long"] = typeof(long);
                }

                foreach (KeyValuePair<string, Type> kvp in ApiInfo.Types)
                {
                    m_ReverseTypeDeclarations[kvp.Value] = kvp.Key;
                }
            }

            #region Type validation and string representation
            public bool IsValidType(Type t) => m_ReverseTypeDeclarations.ContainsKey(t);

            public bool TryGetValidVarType(string typeName, out Type t) => m_ValidVarTypes.TryGetValue(typeName, out t);
            public bool ContainsValidVarType(string typeName) => m_ValidVarTypes.ContainsKey(typeName);

            public string MapType(Type t)
            {
                string res;
                if(!m_ReverseTypeDeclarations.TryGetValue(t, out res))
                {
                    res = t == typeof(bool) ? "boolean" : "???";
                }
                return res;
            }
            #endregion

            #region Function Body access
            public LineInfo GetLine(string message = "")
            {
                if(string.IsNullOrEmpty(message))
                {
                    message = this.GetLanguageString(CurrentCulture, "PrematureEndOfFunctionBody", "Premature end of function body");
                }
                int lineIndex = FunctionLineIndex++;
                List<LineInfo> functionBody = FunctionBody;
                if (lineIndex >= functionBody.Count)
                {
                    throw CompilerException(functionBody[functionBody.Count - 1], message);
                }
                return functionBody[lineIndex];
            }

            public LineInfo PeekLine(string message = "")
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = this.GetLanguageString(CurrentCulture, "PrematureEndOfFunctionBody", "Premature end of function body");
                }
                int lineIndex = FunctionLineIndex;
                List<LineInfo> functionBody = FunctionBody;
                if (lineIndex >= functionBody.Count)
                {
                    throw CompilerException(functionBody[functionBody.Count - 1], message);
                }
                return functionBody[lineIndex];
            }

            public bool HaveMoreLines => FunctionLineIndex < FunctionBody.Count;
            #endregion
        }
    }
}
