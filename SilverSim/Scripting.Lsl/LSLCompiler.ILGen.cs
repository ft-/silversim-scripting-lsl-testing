﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl.Expression;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SilverSim.Scripting.Lsl
{
    public partial class LSLCompiler
    {
        CompilerException CompilerException(LineInfo p, string message)
        {
            return new CompilerException(p.LineNumber, message);
        }

        void ProcessExpression(
            CompileState compileState,
            TypeBuilder scriptTypeBuilder,
            TypeBuilder stateTypeBuilder,
            ILGenerator ilgen,
            Type expectedType,
            int startAt,
            int endAt,
            LineInfo functionLine,
            Dictionary<string, object> localVars)
        {
            if(startAt > endAt)
            {
                throw new NotSupportedException();
            }

            Tree expressionTree;
            try
            {
                List<string> expressionLine = functionLine.Line.GetRange(startAt, endAt - startAt + 1);
                CollapseStringConstants(expressionLine);
                expressionTree = new Tree(expressionLine, m_OpChars, m_SingleOps, m_NumericChars);
                SolveTree(compileState, expressionTree, localVars.Keys);
            }
            catch(Exception e)
            {
                throw CompilerException(functionLine, e.Message);
            }
            ProcessExpression(
                compileState, 
                scriptTypeBuilder, 
                stateTypeBuilder, 
                ilgen, 
                expectedType, 
                expressionTree,
                functionLine.LineNumber,
                localVars);
        }

        void ProcessExpression(
            CompileState compileState,
            TypeBuilder scriptTypeBuilder,
            TypeBuilder stateTypeBuilder,
            ILGenerator ilgen,
            Type expectedType,
            Tree functionTree,
            int lineNumber,
            Dictionary<string, object> localVars)
        {
            Type retType = ProcessExpressionPart(
                compileState,
                scriptTypeBuilder,
                stateTypeBuilder,
                ilgen,
                functionTree,
                lineNumber,
                localVars);
            ProcessImplicitCasts(
                ilgen,
                expectedType,
                retType,
                lineNumber);
        }
    }
}