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

#pragma warning disable RCS1029, IDE0018

using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Common;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SilverSim.Scripting.Lsl
{
    public partial class LSLCompiler
    {
        private CompilerException ParserException(Parser p, string message)
        {
            string fname;
            int lineno;
            p.GetBeginFileInfo(out fname, out lineno);
            return new CompilerException(lineno, message);
        }

        private bool IsValidVarName(string name)
        {
            if (name[0] != '_' && !(name[0] >= 'a' && name[0] <= 'z') && !(name[0] >= 'A' && name[0] <= 'Z') && name[0] != '_' && name[0] != '$')
            {
                return false;
            }
            foreach (char c in name.Substring(1))
            {
                if (!(name[0] >= 'a' && name[0] <= 'z') && !(name[0] >= 'A' && name[0] <= 'Z') && name[0] != '_' && name[0] != '$')
                {
                    return false;
                }
            }
            return true;
        }

        private void CheckValidName(CompileState cs, Parser p, string type, string name)
        {
            string ltype = type.Replace(" ", "");
            if (name.Length == 0)
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "Name0IsNotValid", type + " name '{0}' is not valid."), name));
            }
            if (name[0] != '_' && !(name[0] >= 'a' && name[0] <= 'z') && !(name[0] >= 'A' && name[0] <= 'Z') && name[0] != '_' && name[0] != '$')
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "Name0IsNotValid", type + " name '{0}' is not valid."), name));
            }
            foreach (char c in name.Substring(1))
            {
                if (!(name[0] >= 'a' && name[0] <= 'z') && !(name[0] >= 'A' && name[0] <= 'Z') && name[0] != '_' && name[0] != '$')
                {
                    throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "Name0IsNotValid", type + " name '{0}' is not valid."), name));
                }
            }
        }

        private void CheckUsedFieldName(CompileState cs, Parser p, string type, string name)
        {
            string ltype = type.Replace(" ", "");
            CheckValidName(cs, p, type, name);
            if (cs.IsReservedWord(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAs0IsAReservedWord", type + " cannot be declared as '{0}'. '{0}' is a reserved word."), name));
            }
        }

        private void CheckUsedName(CompileState cs, Parser p, string type, string name)
        {
            string ltype = type.Replace(" ", "");
            CheckValidName(cs, p, type, name);
            if (cs.IsReservedWord(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAs0IsAReservedWord", type + " cannot be declared as '{0}'. '{0}' is a reserved word."), name));
            }
            else if (cs.ApiInfo.Types.ContainsKey(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAs0IsAType", type + " cannot be declared as '{0}'. '{0}' is a type."), name));
            }
            else if (cs.ApiInfo.Properties.ContainsKey(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAs0IsAProperty", type + " cannot be declared as '{0}'. '{0}' is a property."), name));
            }
            else if (cs.ApiInfo.Methods.ContainsKey(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAs0IsADefinedFunc", type + " cannot be declared as '{0}'. '{0}' is an already defined function name."), name));
            }
            else if (cs.ApiInfo.Constants.ContainsKey(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAs0IsADefinedConstant", type + " cannot be declared as '{0}'. '{0}' is an already defined constant."), name));
            }
            else if (cs.ApiInfo.EventDelegates.ContainsKey(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAs0IsAnEvent", type + " cannot be declared as '{0}'. '{0}' is an event."), name));
            }
            else if (!cs.LanguageExtensions.EnableFunctionOverloading && type == "Function" && cs.m_Functions.ContainsKey(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAs0IsAlreadyADefinedUserFunc", type + " cannot be declared as '{0}'. '{0}' is an already defined as user function."), name));
            }
            else if (type == "Variable" && cs.m_Functions.ContainsKey(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAs0IsAlreadyADefinedUserFunc", type + " cannot be declared as '{0}'. '{0}' is an already defined as user function."), name));
            }
            if (cs.m_LocalVariables.Count == 0)
            {
                /* prevent next check from getting an empty list */
            }
            else if (cs.m_LocalVariables[cs.m_LocalVariables.Count - 1].Contains(name))
            {
                throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, ltype + "CannotBeDeclaredAS0IsAlreadyALocalVar", type + " cannot be declared as '{0}'. '{0}' is an already defined as local variable in the same block."), name));
            }
        }

        internal struct FuncParamInfo
        {
            public Type Type;
            public string Name;
        }

        private List<FuncParamInfo> CheckFunctionParameters(CompileState cs, Parser p, List<TokenInfo> arguments)
        {
            var funcParams = new List<FuncParamInfo>();
            cs.m_LocalVariables.Add(new List<string>());
            if (arguments.Count == 1 && arguments[0] == ")")
            {
                return funcParams;
            }

            int i = 0;
            if(arguments.Count >= 2 && arguments[0] == "this" && arguments[1] != ")")
            {
                /* custom member method designator */
                i = 1;
            }
            for (; i < arguments.Count; i += 3)
            {
                var fp = new FuncParamInfo();
                if(!cs.TryGetValidVarType(arguments[i], out fp.Type))
                {
                    throw ParserException(p, string.Format(this.GetLanguageString(cs.CurrentCulture, "InvalidTypeForParameter0", "Invalid type for parameter {0}"), i / 3));
                }

                CheckUsedName(cs, p, "Parameter", arguments[i + 1]);
                cs.m_LocalVariables[0].Add(arguments[i + 1]);
                fp.Name = arguments[i + 1];
                funcParams.Add(fp);

                if (arguments[i + 2] == ",")
                {
                    /* nothing to do here */
                }
                else if (arguments[i + 2] == ")")
                {
                    if (i + 3 != arguments.Count)
                    {
                        throw ParserException(p, this.GetLanguageString(cs.CurrentCulture, "MissingClosingParenthesisAtTheEndOfFunctionDeclaration", "Missing ')' at the end of function declaration"));
                    }
                    return funcParams;
                }
            }
            throw ParserException(p, this.GetLanguageString(cs.CurrentCulture, "MissingClosingParenthesisAtTheEndOfFunctionDeclaration", "Missing ')' at the end of function declaration"));
        }

        private int FindEndOfControlFlow(CompileState cs, List<TokenInfo> line, int lineNumber)
        {
            int i;
            var parenstack = new List<string>();

            if(line[1] != "(")
            {
                throw new CompilerException(lineNumber, string.Format(this.GetLanguageString(cs.CurrentCulture, "0IsNotFollowedByOpeningParenthesis", "'{0}' is not followed by '('"), line[0]));
            }

            for (i = 1; i < line.Count; ++i)
            {
                switch(line[i])
                {
                    case "(":
                    case "[":
                        parenstack.Insert(0, line[i]);
                        break;

                    case ")":
                        if(parenstack[0] != "(")
                        {
                            throw new CompilerException(lineNumber, string.Format(this.GetLanguageString(cs.CurrentCulture, "Mismatching0For1", "Mismatching '{0}' for '{1}'"), line[i], parenstack[parenstack.Count - 1]));
                        }
                        parenstack.RemoveAt(0);
                        if(parenstack.Count == 0)
                        {
                            return i;
                        }
                        break;

                    case "]":
                        if (parenstack[0] != "[")
                        {
                            throw new CompilerException(lineNumber, string.Format(this.GetLanguageString(cs.CurrentCulture, "Mismatching0For1", "Mismatching '{0}' for '{1}'"), line[i], parenstack[parenstack.Count - 1]));
                        }
                        parenstack.RemoveAt(0);
                        break;

                    case "if":
                    case "for":
                    case "else":
                    case "while":
                    case "do":
                    case "return":
                    case "state":
                        if(m_ReservedWords.Contains(line[i]))
                        {
                            throw new CompilerException(lineNumber, string.Format(this.GetLanguageString(cs.CurrentCulture, "0NotAllowedIn1", "'{0}' not allowed in '{1}'"), line[i], line[0]));
                        }
                        break;

                    case "continue":
                    case "break":
                        if(cs.LanguageExtensions.EnableBreakContinueStatement)
                        {
                            throw new CompilerException(lineNumber, string.Format(this.GetLanguageString(cs.CurrentCulture, "0NotAllowedIn1", "'{0}' not allowed in '{1}'"), line[i], line[0]));
                        }
                        break;

                    case "switch":
                    case "case":
                        if (cs.LanguageExtensions.EnableSwitchBlock)
                        {
                            throw new CompilerException(lineNumber, string.Format(this.GetLanguageString(cs.CurrentCulture, "0NotAllowedIn1", "'{0}' not allowed in '{1}'"), line[i], line[0]));
                        }
                        break;

                    default:
                        break;
                }
            }
            throw new CompilerException(lineNumber, string.Format(this.GetLanguageString(cs.CurrentCulture, "CouldNotFindEndOf0", "Could not find end of '{0}'"), line[0]));
        }

        private void ParseBlockLine(CompileState compileState, Parser p, List<LineInfo> block, List<TokenInfo> args, int lineNumber, bool inState)
        {
            for (; ; )
            {
                if(args[0] == "else" && args[1] == "if")
                {
                    int eocf = FindEndOfControlFlow(compileState, args.GetRange(1, args.Count - 1), lineNumber) + 1;
                    /* make it a block */
                    if (args[eocf + 1] == "{")
                    {
                        block.Add(new LineInfo(args));
                        ParseBlock(compileState, p, block, inState, true);
                        return;
                    }
                    else
                    {
                        List<TokenInfo> controlflow = args.GetRange(0, eocf + 1);
                        block.Add(new LineInfo(controlflow));
                        args = args.GetRange(eocf + 1, args.Count - eocf - 1);
                    }
                }
                else if(args[0] == "switch" && compileState.LanguageExtensions.EnableSwitchBlock)
                {
                    if(args[args.Count - 1] != "{")
                    {
                        throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "OpeningBraceRequiredForSwitchControlFlowInstruction", "'{' required for 'switch' control flow instruction"));
                    }
                    else if (args[args.Count - 2] != ")")
                    {
                        throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "ClosingParenthesisRequiredForSwitchControlFlowInstruction", "')' required for 'switch' control flow instruction"));
                    }
                    else if (args[1] != "(")
                    {
                        throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "OpeningParenthesisRequiredForSwitchControlFlowInstruction", "'(' required for 'switch' control flow instruction"));
                    }
                    block.Add(new LineInfo(args));
                    ParseBlock(compileState, p, block, inState, true);
                    return;
                }
                else if(args[0] == "continue" && compileState.LanguageExtensions.EnableBreakContinueStatement)
                {
                    if(args[1] != ";")
                    {
                        throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "SemicolonHasToFollow0", "';' has to follow '{0}'"), args[0]));
                    }
                    block.Add(new LineInfo(args));
                    return;
                }
                else if (args[0] == "break" &&
                    (compileState.LanguageExtensions.EnableBreakContinueStatement ||
                     compileState.LanguageExtensions.EnableSwitchBlock))
                {
                    if (args[1] != ";")
                    {
                        throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "SemicolonHasToFollow0", "';' has to follow '{0}'"), args[0]));
                    }
                    block.Add(new LineInfo(args));
                    return;
                }
                else if ((args[0] == "case" || args[0] == "default") &&
                    compileState.LanguageExtensions.EnableSwitchBlock)
                {
                    if(args[args.Count - 1] != ":")
                    {
                        throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "ColonRequiredFor0ControlFlowInstruction", "':' required for '{0}' control flow instruction"), args[0]));
                    }
                    block.Add(new LineInfo(args));
                    return;
                }
                else if (args[0] == "else" || args[0] == "do")
                {
                    if (args[1] == "{")
                    {
                        block.Add(new LineInfo(args));
                        ParseBlock(compileState, p, block, inState, true);
                        return;
                    }
                    else
                    {
                        List<TokenInfo> controlflow = args.GetRange(0, 1);
                        block.Add(new LineInfo(controlflow));
                        args = args.GetRange(1, args.Count - 1);
                    }
                }
                else if (args[0] == "if" || args[0] == "for" || args[0] == "while")
                {
                    int eocf = FindEndOfControlFlow(compileState, args, lineNumber);
                    /* make it a block */
                    if(args[eocf + 1] == "{")
                    {
                        block.Add(new LineInfo(args));
                        ParseBlock(compileState, p, block, inState, true);
                        return;
                    }
                    else
                    {
                        List<TokenInfo> controlflow = args.GetRange(0, eocf + 1);
                        block.Add(new LineInfo(controlflow));
                        args = args.GetRange(eocf + 1, args.Count - eocf - 1);
                    }
                }
                else if (args[0] == "{")
                {
                    block.Add(new LineInfo(args));
                    ParseBlock(compileState, p, block, inState, true);
                    return;
                }
                else if (args[0] == ";")
                {
                    block.Add(new LineInfo(new List<TokenInfo> { new TokenInfo(";", lineNumber) }));
                    return;
                }
                else if(args[args.Count - 1] == "{")
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "ClosingBraceAtEndfLineWithoutControlFlowInstruction", "'{' not allowed at end of line without control flow instruction"));
                }
                else
                {
                    if(compileState.ContainsValidVarType(args[0]))
                    {
                        CheckUsedName(compileState, p, "Local Variable", args[1]);
                        compileState.m_LocalVariables[compileState.m_LocalVariables.Count - 1].Add(args[1]);
                        if (args[2] != ";" && args[2] != "=")
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "ExpectingEqualOrSemicolonAfterVarName0", "Expecting '=' or ';' after variable name {0}"), args[1]));
                        }
                    }
                    block.Add(new LineInfo(args));
                    return;
                }
            }
        }

        private void ParseBlock(CompileState compileState, Parser p, List<LineInfo> block, bool inState, bool addNewLocals = false)
        {
            if (addNewLocals)
            {
                compileState.m_LocalVariables.Add(new List<string>());
            }
            for (; ; )
            {
                int lineNumber;
                string fname;
                var args = new List<TokenInfo>();
                try
                {
                    p.Read(args);
                }
                catch (ArgumentException e)
                {
                    throw ParserException(p, e.Message);
                }
                catch (ParserBase.EndOfStringException)
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "MissingQuoteAtTheEndOfScript", "Missing '\"' at the end of string"));
                }
                catch(ParserBase.StackEmptyException)
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "PrematureEndOfScript", "Premature end of script"));
                }
                catch (ParserBase.EndOfFileException)
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "PrematureEndOfScript", "Premature end of script"));
                }
                p.GetBeginFileInfo(out fname, out lineNumber);

                if (args.Count == 0)
                {
                    /* should not happen but better be safe here */
                }
                else if (args[0] == "}")
                {
                    compileState.m_LocalVariables.RemoveAt(compileState.m_LocalVariables.Count - 1);
                    block.Add(new LineInfo(args));
                    return;
                }
                else
                {
                    ParseBlockLine(compileState, p, block, args, lineNumber, inState);
                }
            }
        }

        private void ParseState(CompileState compileState, Parser p, string stateName)
        {
            compileState.m_States.Add(stateName, new Dictionary<string, List<LineInfo>>());
            compileState.m_LocalVariables.Add(new List<string>());
            try
            {
                for (;;)
                {
                    int lineNumber;
                    var args = new List<TokenInfo>();
                    try
                    {
                        p.Read(args);
                    }
                    catch (ArgumentException e)
                    {
                        throw ParserException(p, e.Message);
                    }
                    catch (ParserBase.EndOfStringException)
                    {
                        throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "MissingQuoteAtTheEndOfScript", "Missing '\"' at the end of string"));
                    }
                    catch (ParserBase.EndOfFileException)
                    {
                        throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "MissingBraceAtEndOfScript", "Missing '}' at end of script"));
                    }
                    catch (ParserBase.StackEmptyException)
                    {
                        throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "PrematureEndOfScript", "Premature end of script"));
                    }
                    lineNumber = p.CurrentLineNumber;

                    if (args.Count == 0)
                    {
                        /* should not happen but better be safe here */
                    }
                    else if (args[args.Count - 1] == ";")
                    {
                        Type stateVarType = null;
                        if (!compileState.TryGetValidVarType(args[0], out stateVarType))
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "NoStatementsOutsideofEventsOffendingState0", "No statements allowed outside of event functions. Offending state {0}."), stateName));
                        }

                        if (!compileState.LanguageExtensions.EnableStateVariables)
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "NeitherVarDeclarationsNorStatementsOutsideofEventsOffendingState0", "Neither variable declarations nor statements allowed outside of event functions. Offending state {0}."), stateName));
                        }
                        if(!BaseTypes.Contains(stateVarType) && Attribute.GetCustomAttribute(stateVarType, typeof(SerializableAttribute)) == null)
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "Type0IsNotSuitableForStateVariable", "Type '{0}' is not suitable for state variable."), args[0]));
                        }
                        CheckUsedName(compileState, p, "State Variable", args[1]);
                        if (args[2] == "=")
                        {
                            if (!compileState.m_StateVariableInitValues.ContainsKey(stateName))
                            {
                                compileState.m_StateVariableInitValues.Add(stateName, new Dictionary<string, LineInfo>());
                            }
                            compileState.m_StateVariableInitValues[stateName].Add(args[1], new LineInfo(args.GetRange(3, args.Count - 4)));
                        }
                        else if(args[2] != ";")
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "StateVarMustBeFollowedByOffendingState0", "State variable name must be followed by ';' or '='. Offending state {0}."), stateName));
                        }
                        if(!compileState.m_StateVariableDeclarations.ContainsKey(stateName))
                        {
                            compileState.m_StateVariableDeclarations.Add(stateName, new Dictionary<string, Type>());
                        }
                        compileState.m_StateVariableDeclarations[stateName].Add(args[1], stateVarType);
                    }
                    else if (args[args.Count - 1] == "{")
                    {
                        if (!compileState.ApiInfo.EventDelegates.ContainsKey(args[0]))
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "Name0IsNotAValidEvent", "'{0}' is not a valid event."), args[0]));
                        }
                        if (compileState.m_LocalVariables.Count != 1)
                        {
                            throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InternalParserError", "Internal parser error"));
                        }
                        if(args.Count < 4)
                        {
                            throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "IncompleteEventSignature", "Incomplete event signature."));
                        }
                        List<FuncParamInfo> fp = CheckFunctionParameters(compileState, p, args.GetRange(2, args.Count - 3));
                        MethodInfo m = compileState.ApiInfo.EventDelegates[args[0]];
                        ParameterInfo[] pi = m.GetParameters();
                        if (fp.Count != pi.Length)
                        {
                            var parameterNames = new List<string>();
                            foreach(ParameterInfo it in pi)
                            {
                                parameterNames.Add(compileState.MapType(it.ParameterType) + " " + it.Name);
                            }
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "Param0DoesNotHaveTheCorrectParameters1", "'{0}' does not have the correct parameters. Reference: {0}({1})"), args[0], string.Join(",", parameterNames)));
                        }
                        int i;
                        for (i = 0; i < fp.Count; ++i)
                        {
                            if (!fp[i].Type.Equals(pi[i].ParameterType))
                            {
                                var parameterNames = new List<string>();
                                foreach (ParameterInfo it in pi)
                                {
                                    parameterNames.Add(compileState.MapType(it.ParameterType) + " " + it.Name);
                                }
                                throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "Param0DoesNotMatchInParameterTypes1", "'{0}' does not match in parameter types. Reference: {0}({1})"), args[0], string.Join(",", parameterNames)));
                            }
                        }
                        if (compileState.m_States[stateName].ContainsKey(args[0]))
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "Event0AlreadyDefined", "Event '{0}' already defined"), args[0]));
                        }
                        var stateList = new List<LineInfo>();
                        compileState.m_States[stateName].Add(args[0], stateList);
                        stateList.Add(new LineInfo(args));
                        ParseBlock(compileState, p, stateList, true);
                    }
                    else if (args[0] == "}")
                    {
                        if (compileState.m_States[stateName].Count == 0)
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "State0DoesNotHaveAnyEvents", "state '{0}' does not have any events."), stateName));
                        }
                        return;
                    }
                }
            }
            finally
            {
                compileState.m_LocalVariables.RemoveAt(compileState.m_LocalVariables.Count - 1);
            }
        }

        private void ParseStruct(CompileState compileState, Parser p, string structName)
        {
            compileState.m_Structs.Add(structName, new Dictionary<string, LineInfo>());
            for (; ; )
            {
                int lineNumber;
                var args = new List<TokenInfo>();
                try
                {
                    p.Read(args);
                }
                catch (ArgumentException e)
                {
                    throw ParserException(p, e.Message);
                }
                catch (ParserBase.EndOfStringException)
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "MissingQuoteAtTheEndOfScript", "Missing '\"' at the end of string"));
                }
                catch (ParserBase.EndOfFileException)
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "MissingBraceAtEndOfScript", "Missing '}' at end of script"));
                }
                catch (ParserBase.StackEmptyException)
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "PrematureEndOfScript", "Premature end of script"));
                }
                lineNumber = p.CurrentLineNumber;

                if (args.Count == 0)
                {
                    /* should not happen but better be safe here */
                }
                else if (args[args.Count - 1] == ";")
                {
                    Type stateVarType;
                    if(compileState.m_Structs.ContainsKey(args[0]))
                    {
                        /* we have a custom struct as member */
                    }
                    else if (!compileState.TryGetValidVarType(args[0], out stateVarType))
                    {
                        throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "NoStatementsInsideOfStructOffendingState0", "No statements allowed inside struct. Offending struct {0}."), structName));
                    }
                    else if (!BaseTypes.Contains(stateVarType) && Attribute.GetCustomAttribute(stateVarType, typeof(SerializableAttribute)) == null)
                    {
                        throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "Type0IsNotSuitableForStructMember", "Type '{0}' is not suitable for struct member."), args[0]));
                    }
                    CheckUsedFieldName(compileState, p, "Struct Member", args[1]);
                    if(compileState.m_Structs[structName].ContainsKey(args[1]))
                    {
                        throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "DuplicateMember0InStruct1", "Duplicate member '{0}' in struct '{1}'."), args[1], structName));
                    }
                    compileState.m_Structs[structName].Add(args[1], new LineInfo(args));
                    if (args[2] != ";")
                    {
                        throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "StructVarMustBeFollowedByOffendingStruct0", "Struct variable name must be followed by ';' or '='. Offending struct {0}."), structName));
                    }
                }
                else if (args[args.Count - 1] == "{")
                {
                    throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "OnlyMemberFieldsAllowedOffendingState0", "Only member fields allowed. Offending struct {0}."), structName));
                }
                else if (args[0] == "}")
                {
                    if (compileState.m_Structs[structName].Count == 0)
                    {
                        throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "Struct0DoesNotHaveAnyFields", "struct '{0}' does not have any fields."), structName));
                    }
                    return;
                }
            }
        }

        private readonly Type[] BaseTypes = new Type[] { typeof(int), typeof(long), typeof(string), typeof(double), typeof(Vector3), typeof(AnArray), typeof(Quaternion), typeof(LSLKey) };

        private CompileState Preprocess(Dictionary<int, string> shbangs, TextReader reader, int lineNumber = 1, CultureInfo cultureInfo = null, Func<string, TextReader> includeOpen = null)
        {
            var compileState = new CompileState(cultureInfo);
            APIFlags acceptedFlags;
            var apiExtensions = new List<string>();
            acceptedFlags = APIFlags.OSSL | APIFlags.LSL;
            string windLightApiType = APIExtension.LightShare;
            compileState.ForcedSleepDefault = true;
            compileState.LanguageExtensions.EnableImplicitTypecastToStringOnAddOperator = false;
            compileState.LanguageExtensions.EnableNonFirstDefaultState = true;
            compileState.LanguageExtensions.EnableLogicalModifyAssignments = true;
            compileState.LanguageExtensions.EnableAllowImplicitCastToString = true;
            foreach (KeyValuePair<int, string> shbang in shbangs)
            {
                if (shbang.Value.StartsWith("//#!Mode:"))
                {
                    /* we got a sh-bang here, it is a lot safer than what OpenSimulator uses */
                    string mode = shbang.Value.Substring(9).Trim().ToLower();
                    if (mode == "lsl")
                    {
                        windLightApiType = string.Empty;
                        acceptedFlags = APIFlags.LSL;
                        compileState.ForcedSleepDefault = true;
                        compileState.LanguageExtensions.EnableFunctionOverloading = false;
                        compileState.LanguageExtensions.EnableNonFirstDefaultState = false;
                        compileState.LanguageExtensions.EnableLogicalModifyAssignments = false;
                        compileState.LanguageExtensions.EnableAllowImplicitCastToString = false;
                    }
                    else if(mode == "ossl")
                    {
                        windLightApiType = APIExtension.LightShare;
                        acceptedFlags = APIFlags.OSSL | APIFlags.LSL;
                        compileState.ForcedSleepDefault = true;
                        compileState.LanguageExtensions.EnableNonFirstDefaultState = true;
                        compileState.LanguageExtensions.EnableFunctionOverloading = true;
                        compileState.LanguageExtensions.EnableAllowImplicitCastToString = true;
                    }
                    else if (mode == "assl")
                    {
                        windLightApiType = APIExtension.LightShare;
                        acceptedFlags = APIFlags.ASSL | APIFlags.OSSL | APIFlags.LSL;
                        compileState.LanguageExtensions.EnableExtendedTypecasts = true;
                        compileState.LanguageExtensions.EnableStateVariables = true;
                        compileState.LanguageExtensions.EnableBreakContinueStatement = true;
                        compileState.LanguageExtensions.EnableSwitchBlock = true;
                        compileState.ForcedSleepDefault = false;
                        compileState.LanguageExtensions.EnableArrayThisOperator = true;
                        compileState.LanguageExtensions.EnableNonFirstDefaultState = true;
                        compileState.LanguageExtensions.EnableLogicalModifyAssignments = true;
                        compileState.LanguageExtensions.EnableAllowImplicitCastToString = true;
                        if (!apiExtensions.Contains(APIExtension.LongInteger.ToLower()))
                        {
                            apiExtensions.Add(APIExtension.LongInteger.ToLower());
                        }
                        if (!apiExtensions.Contains(APIExtension.Properties.ToLower()))
                        {
                            apiExtensions.Add(APIExtension.Properties.ToLower());
                        }
                        if(!apiExtensions.Contains(APIExtension.MemberFunctions.ToLower()))
                        {
                            apiExtensions.Add(APIExtension.MemberFunctions.ToLower());
                        }
                        if(!apiExtensions.Contains(APIExtension.Structs.ToLower()))
                        {
                            apiExtensions.Add(APIExtension.Structs.ToLower());
                        }
                        compileState.LanguageExtensions.EnableFunctionOverloading = true;
                    }
                    else if (mode == "aurora" || mode == "whitecore")
                    {
                        windLightApiType = APIExtension.WindLight_Aurora;
                        acceptedFlags = APIFlags.OSSL | APIFlags.LSL;
                        compileState.ForcedSleepDefault = true;
                        compileState.LanguageExtensions.EnableNonFirstDefaultState = true;
                        compileState.LanguageExtensions.EnableFunctionOverloading = true;
                        compileState.LanguageExtensions.EnableAllowImplicitCastToString = true;
                    }
                }
                else if (shbang.Value.StartsWith("//#!Enable:"))
                {
                    string api = shbang.Value.Substring(11).Trim().ToLower();
                    if (!apiExtensions.Contains(api))
                    {
                        apiExtensions.Add(api);
                    }
                }
                else if(shbang.Value.StartsWith("//#!UsesSinglePrecision"))
                {
                    compileState.UsesSinglePrecision = true;
                }
            }

            if((acceptedFlags & APIFlags.OSSL) != 0 || (acceptedFlags & APIFlags.ASSL) != 0)
            {
                compileState.LanguageExtensions.EnableImplicitTypecastToStringOnAddOperator = true;
            }

            if (apiExtensions.Contains(APIExtension.Structs.ToLower()))
            {
                compileState.LanguageExtensions.EnableStructTypes = true;
                if(!apiExtensions.Contains(APIExtension.MemberFunctions.ToLower()))
                {
                    apiExtensions.Add(APIExtension.MemberFunctions.ToLower());
                }
            }

            if(apiExtensions.Contains(APIExtension.LongInteger.ToLower()))
            {
                compileState.LanguageExtensions.EnableLongIntegers = true;
            }

            if(apiExtensions.Contains(APIExtension.ExtendedTypecasts.ToLower()))
            {
                compileState.LanguageExtensions.EnableExtendedTypecasts = true;
            }

            if(apiExtensions.Contains(APIExtension.StateVariables.ToLower()))
            {
                compileState.LanguageExtensions.EnableStateVariables = true;
            }

            if (apiExtensions.Contains(APIExtension.BreakContinue.ToLower()))
            {
                compileState.LanguageExtensions.EnableBreakContinueStatement = true;
            }

            if(apiExtensions.Contains(APIExtension.SwitchBlock.ToLower()))
            {
                compileState.LanguageExtensions.EnableSwitchBlock = true;
            }

            if(apiExtensions.Contains(APIExtension.Properties.ToLower()))
            {
                compileState.LanguageExtensions.EnableProperties = true;
            }

            if (apiExtensions.Contains(APIExtension.MemberFunctions.ToLower()) ||
                apiExtensions.Contains(APIExtension.Properties.ToLower()))
            {
                compileState.LanguageExtensions.EnableMemberFunctions = true;
            }

            foreach (KeyValuePair<APIFlags, ApiInfo> kvp in m_ApiInfos)
            {
                if((kvp.Key & acceptedFlags) != 0)
                {
                    compileState.ApiInfo.Add(kvp.Value);
                }
            }

            if(windLightApiType.Length != 0 && !apiExtensions.Contains(windLightApiType.ToLower()))
            {
                apiExtensions.Add(windLightApiType.ToLower());
            }

            foreach(string extension in apiExtensions)
            {
                ApiInfo apiInfo;
                if (m_ApiExtensions.TryGetValue(extension.ToLower(), out apiInfo))
                {
                    compileState.ApiInfo.Add(apiInfo);
                }
            }

            compileState.FinalizeTypeList();

            var p = new Parser(cultureInfo);
            p.Push(reader, string.Empty, lineNumber);
            var includes = new List<string>();

            for (; ; )
            {
                var args = new List<TokenInfo>();
                try
                {
                    p.Read(args);
                }
                catch (ArgumentException e)
                {
                    throw ParserException(p, e.Message);
                }
                catch (ParserBase.EndOfStringException)
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "MissingQuoteAtTheEndOfScript", "Missing '\"' at the end of string"));
                }
                catch (ParserBase.StackEmptyException)
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "PrematureEndOfScript", "Premature end of script"));
                }
                catch (ParserBase.EndOfFileException)
                {
                    break;
                }
                lineNumber = p.CurrentLineNumber;
                if (args.Count == 0)
                {
                    /* should not happen but better be safe here */
                }
                else if (args[args.Count - 1] == ";")
                {
                    /* variable definition */
                    if (args[2] != "=" && args[2] != ";")
                    {
                        throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InvalidVariableDefinitionEitherSemicolonOrEqual", "Invalid variable definition. Either ';' or an expression preceeded by '='"));
                    }
                    Type varType;
                    if (compileState.TryGetValidVarType(args[0], out varType))
                    {
                        if(!BaseTypes.Contains(varType) && Attribute.GetCustomAttribute(varType, typeof(SerializableAttribute)) == null)
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "Type0IsNotSuitableForGlobalVariable", "Type '{0}' is not suitable for global variable."), args[0]));
                        }
                        CheckUsedName(compileState, p, "Variable", args[1]);
                        compileState.m_VariableDeclarations[args[1]] = varType;
                        if (args[2] == "=")
                        {
                            compileState.m_VariableInitValues[args[1]] = new LineInfo(args.GetRange(3, args.Count - 4));
                        }
                    }
                    else
                    {
                        throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "InvalidVariableDefinitionWrongType0", "Invalid variable definition. Wrong type {0}."), args[0]));
                    }
                }
                else if (args[args.Count - 1] == "{")
                {
                    if (args[0] == "default")
                    {
                        /* default state begin */
                        if (args[1] != "{")
                        {
                            throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InvalidDefaultStateDeclaration", "Invalid default state declaration"));
                        }
                        ParseState(compileState, p, "default");
                    }
                    else if(args[0] == "struct" && compileState.LanguageExtensions.EnableStructTypes)
                    {
                        /* struct type begin */
                        if(args[1] == "{" || args[2] != "{")
                        {
                            throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InvalidDefaultStateDeclaration", "Invalid struct declaration"));
                        }
                        CheckValidName(compileState, p, "Struct", args[1]);
                        if(compileState.ContainsValidVarType(args[1]) || compileState.m_Structs.ContainsKey(args[1]))
                        {
                            throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "AlreadyADefinedType0", "Already defined a type '{0}'"), args[1]));
                        }
                        ParseStruct(compileState, p, args[1]);
                    }
                    else if (args[0] == "state")
                    {
                        /* state begin */
                        if (args[1] == "default")
                        {
                            throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "DefaultStateCannotBeDeclaredWithState", "default state cannot be declared with state"));
                        }
                        else if (compileState.m_States.Count == 0 && !compileState.LanguageExtensions.EnableNonFirstDefaultState)
                        {
                            throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "DefaultstateMustBeFirstDeclaredStateInScript", "default state must be first declared state in script"));
                        }
                        CheckValidName(compileState, p, "State", args[1]);
                        if (compileState.m_States.ContainsKey(args[1]))
                        {
                            throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "StateDefinitionCannotBeDeclaredTwice", "state definition cannot be declared twice"));
                        }

                        if (args[2] != "{")
                        {
                            throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InvalidStateDeclaration", "Invalid state declaration"));
                        }
                        ParseState(compileState, p, args[1]);
                    }
                    else
                    {
                        List<FuncParamInfo> funcparam;
                        var funcList = new List<LineInfo>();
                        /* either type or function name */
                        if (compileState.ApiInfo.Types.ContainsKey(args[0]))
                        {
                            CheckUsedName(compileState, p, "Function", args[1]);
                            if (compileState.m_LocalVariables.Count != 0)
                            {
                                throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InternalParserError", "Internal parser error"));
                            }
                            funcparam = CheckFunctionParameters(compileState, p, args.GetRange(3, args.Count - 4));
                            funcList.Add(new LineInfo(args));
                            ParseBlock(compileState, p, funcList, false);
                            if (!compileState.m_Functions.ContainsKey(args[1]))
                            {
                                compileState.m_Functions.Add(args[1], new List<FunctionInfo>());
                            }
                            compileState.m_Functions[args[1]].Add(new FunctionInfo(funcparam, funcList));
                        }
                        else
                        {
                            if (args.Count < 3)
                            {
                                throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InvalidStateDeclaration", "Invalid state declaration"));
                            }
                            if (compileState.m_LocalVariables.Count != 0)
                            {
                                throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InternalParserError", "Internal parser error"));
                            }
                            funcparam = CheckFunctionParameters(compileState, p, args.GetRange(2, args.Count - 3));
                            args.Insert(0, new TokenInfo("void", lineNumber));
                            funcList.Add(new LineInfo(args));
                            ParseBlock(compileState, p, funcList, false);
                            if (!compileState.m_Functions.ContainsKey(args[1]))
                            {
                                compileState.m_Functions.Add(args[1], new List<FunctionInfo>());
                            }
                            compileState.m_Functions[args[1]].Add(new FunctionInfo(funcparam, funcList));
                        }
                    }
                }
                else if (args[0] == "}")
                {
                    throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "ClosingBraceWithoutMatchingOpeningBrace", "'}' found without matching '{'"));
                }
                else if(args[0] == "#" && args.Count > 1)
                {
                    switch(args[1])
                    {
                        case "include":
                            if(args.Count != 3)
                            {
                                throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InvalidArgumentsToInclude", "Invalid arguments to #include"));
                            }
                            else if(!args[2].StartsWith("\"") || !args[2].EndsWith("\""))
                            {
                                throw ParserException(p, this.GetLanguageString(compileState.CurrentCulture, "InvalidArgumentsToInclude", "Invalid arguments to #include"));
                            }
                            else if(includeOpen == null)
                            {
                                throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "CannotInclude0", "Could not include '{0}'."), args[2].Substring(1, args[2].Length - 2)));
                            }
                            else
                            {
                                string includename = args[2].Substring(1, args[2].Length - 2);
                                TextReader includeReader;
                                try
                                {
                                    includeReader = includeOpen(includename);
                                }
                                catch
                                {
                                    throw ParserException(p, string.Format(this.GetLanguageString(compileState.CurrentCulture, "CannotInclude0", "Could not include '{0}'."), includename));
                                }
                                p.Push(includeReader, includename);
                            }
                            break;

                        case "endinclude":
                            if(p.IsIncluded)
                            {
                                p.Pop();
                            }
                            break;
                    }
                }
            }
            return compileState;
        }
    }
}
