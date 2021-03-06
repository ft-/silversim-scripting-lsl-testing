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

using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Common;
using SilverSim.Scripting.Lsl.Expression;
using SilverSim.Types;
using System;
using System.Collections.Generic;

namespace SilverSim.Scripting.Lsl
{
    public partial class LSLCompiler
    {
        public enum ResultIsModifiedEnum
        {
            No,
            Yes
        }

        private ResultIsModifiedEnum ProcessExpression(
            CompileState compileState,
            Type expectedType,
            int startAt,
            int endAt,
            LineInfo functionLine,
            Dictionary<string, object> localVars,
            bool enableCommaSeparatedExpressions = false)
        {
            if(startAt > endAt)
            {
                throw new NotSupportedException();
            }

            List<TokenInfo> expressionLine = functionLine.Line.GetRange(startAt, endAt - startAt + 1);
            Tree expressionTree = LineToExpressionTree(compileState, expressionLine, localVars.Keys, compileState.CurrentCulture, enableCommaSeparatedExpressions);

            if (expressionTree.SubTree.Count > 1)
            {
                if (expectedType != typeof(void))
                {
                    throw new CompilerException(expressionTree.SubTree[1].LineNumber, this.GetLanguageString(compileState.CurrentCulture, "SyntaxError", "Syntax Error"));
                }

                foreach(Tree subtree in expressionTree.SubTree)
                {
                    ProcessExpression(compileState, expectedType, expressionTree, localVars);
                }
                return ResultIsModifiedEnum.No;
            }

            return ProcessExpression(
                compileState,
                expectedType,
                expressionTree,
                localVars);
        }

        private ResultIsModifiedEnum ProcessExpression(
            CompileState compileState,
            Type expectedType,
            Tree functionTree,
            Dictionary<string, object> localVars)
        {
            var isModified = ResultIsModifiedEnum.No;

            Type retType = ProcessExpressionPart(
                compileState,
                functionTree,
                localVars);
            ProcessImplicitCasts(
                compileState,
                expectedType,
                retType,
                functionTree.LineNumber);

            if(functionTree.SubTree[0].Type == Tree.EntryType.Variable ||
                functionTree.SubTree[0].Type == Tree.EntryType.ThisOperator)
            {
                /* variables are unmodified (original variables shown) */
            }
            else if(functionTree.SubTree[0].Type == Tree.EntryType.OperatorRightUnary)
            {
                /* right unary operators are unmodified (original variable shown) */
            }
            else if(functionTree.SubTree[0].Type != Tree.EntryType.OperatorBinary ||
                    !(functionTree.SubTree[0].Entry == "+=" ||
                    functionTree.SubTree[0].Entry == "-=" ||
                    functionTree.SubTree[0].Entry == "*=" ||
                    functionTree.SubTree[0].Entry == "/=" ||
                    functionTree.SubTree[0].Entry == "%=" ||
                    functionTree.SubTree[0].Entry == "&=" ||
                    functionTree.SubTree[0].Entry == "|=" ||
                    functionTree.SubTree[0].Entry == "^=" ||
                    functionTree.SubTree[0].Entry == "."))
            {
                isModified = ResultIsModifiedEnum.Yes;
            }
            if(functionTree.SubTree[0].Value != null)
            {
                isModified = ResultIsModifiedEnum.Yes;
            }

            if (expectedType != retType)
            {
                isModified = ResultIsModifiedEnum.Yes;
            }
            return isModified;
        }

        private Type ProcessExpressionToAnyType(
            CompileState compileState,
            int startAt,
            int endAt,
            LineInfo functionLine,
            Dictionary<string, object> localVars)
        {
            if (startAt > endAt)
            {
                throw new NotSupportedException();
            }

            List<TokenInfo> expressionLine = functionLine.Line.GetRange(startAt, endAt - startAt + 1);
            Tree expressionTree = LineToExpressionTree(compileState, expressionLine, localVars.Keys, compileState.CurrentCulture);

            return ProcessExpressionPart(
                compileState,
                expressionTree,
                localVars);
        }
    }
}
