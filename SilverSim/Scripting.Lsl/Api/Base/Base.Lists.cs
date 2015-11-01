﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Types;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Common;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Scripting.Lsl.Api.Base
{
    public partial class BaseApi
    {
        [APILevel(APIFlags.LSL, "llDeleteSubList")]
        [LSLTooltip("Returns a list that is a copy of src but with the slice from start to end removed.")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal AnArray DeleteSubList(ScriptInstance instance,
            [LSLTooltip("source")]
            AnArray src,
            [LSLTooltip("start index")]
            int start,
            [LSLTooltip("end index")]
            int end)
        {
            if (start < 0)
            {
                start = src.Count - start;
            }
            if (end < 0)
            {
                end = src.Count - end;
            }

            if (start < 0)
            {
                start = 0;
            }
            else if (start > src.Count)
            {
                start = src.Count;
            }

            if (end < 0)
            {
                end = 0;
            }
            else if (end > src.Count)
            {
                end = src.Count;
            }

            if (start > end)
            {
                AnArray res = new AnArray();
                for (int i = start; i <= end; ++i)
                {
                    res.Add(src[i]);
                }

                return res;
            }
            else
            {
                AnArray res = new AnArray();

                for (int i = 0; i < start + 1; ++i)
                {
                    res.Add(src[i]);
                }

                for (int i = end; i < src.Count; ++i)
                {
                    res.Add(src[i]);
                }

                return res;
            }
        }

        [APILevel(APIFlags.LSL, "llList2ListStrided")]
        [LSLTooltip("Returns a list of all the entries in the strided list whose index is a multiple of stride in the range start to end.")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal AnArray List2ListStrided(ScriptInstance instance,
            AnArray src,
            [LSLTooltip("start index")]
            int start,
            [LSLTooltip("end index")]
            int end,
            [LSLTooltip("number of entries per stride, if less than 1 it is assumed to be 1")]
            int stride)
        {

            AnArray result = new AnArray();
            int[] si = new int[2];
            int[] ei = new int[2];
            bool twopass = false;

            /*
             * First step is always to deal with negative indices
             */

            if (start < 0)
            {
                start = src.Count + start;
            }
            if (end < 0)
            {
                end = src.Count + end;
            }

            /*
             * Out of bounds indices are OK, just trim them accordingly
             */

            if (start > src.Count)
            {
                start = src.Count;
            }

            if (end > src.Count)
            {
                end = src.Count;
            }

            if (stride == 0)
            {
                stride = 1;
            }

            /*
             * There may be one or two ranges to be considered
             */

            if (start != end)
            {

                if (start <= end)
                {
                    si[0] = start;
                    ei[0] = end;
                }
                else
                {
                    si[1] = start;
                    ei[1] = src.Count;
                    si[0] = 0;
                    ei[0] = end;
                    twopass = true;
                }

                /*
                 * The scan always starts from the beginning of the
                 * source list, but members are only selected if they
                 * fall within the specified sub-range. The specified
                 * range values are inclusive.
                 * A negative stride reverses the direction of the
                 * scan producing an inverted list as a result.
                 */

                if (stride > 0)
                {
                    for (int i = 0; i < src.Count; i += stride)
                    {
                        if (i <= ei[0] && i >= si[0])
                        {
                            result.Add(src[i]);
                        }
                        if (twopass && i >= si[1] && i <= ei[1])
                        {
                            result.Add(src[i]);
                        }
                    }
                }
                else if (stride < 0)
                {
                    for (int i = src.Count - 1; i >= 0; i += stride)
                    {
                        if (i <= ei[0] && i >= si[0])
                        {
                            result.Add(src[i]);
                        }
                        if (twopass && i >= si[1] && i <= ei[1])
                        {
                            result.Add(src[i]);
                        }
                    }
                }
            }
            else
            {
                if (start % stride == 0)
                {
                    result.Add(src[start]);
                }
            }

            return result;
        }

        [APILevel(APIFlags.LSL, "llList2List")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal AnArray List2List(ScriptInstance instance, AnArray src, int start, int end)
        {
            if (start < 0)
            {
                start = src.Count - start;
            }
            if (end < 0)
            {
                end = src.Count - end;
            }

            if (start < 0)
            {
                start = 0;
            }
            else if (start > src.Count)
            {
                start = src.Count;
            }

            if (end < 0)
            {
                end = 0;
            }
            else if (end > src.Count)
            {
                end = src.Count;
            }

            if (start <= end)
            {
                AnArray res = new AnArray();
                for (int i = start; i <= end; ++i )
                {
                    res.Add(src[i]);
                }

                return res;
            }
            else
            {
                AnArray res = new AnArray();

                for (int i = 0; i < end + 1; ++i)
                {
                    res.Add(src[i]);
                }

                for (int i = start; i < src.Count; ++i)
                {
                    res.Add(src[i]);
                }

                return res;
            }
        }

        [APILevel(APIFlags.LSL, "llList2Float")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal double List2Float(ScriptInstance instance, AnArray src, int index)
        {
            if(index < 0)
            {
                index = src.Count - index;
            }

            if(index < 0 ||index >=src.Count)
            {
                return 0;
            }

            return src[index].AsReal;
        }

        [APILevel(APIFlags.LSL, "llListFindList")]
        [LSLTooltip("Returns the integer index of the first instance of test in src.")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal int ListFindList(ScriptInstance instance,
            [LSLTooltip("what to search in (haystack)")]
            AnArray src,
            [LSLTooltip("what to search for (needle)")]
            AnArray test)
        {
            int index = -1;
            int length = src.Count - test.Count + 1;

            /* If either list is empty, do not match */
            if (src.Count != 0 && test.Count != 0)
            {
                for (int i = 0; i < length; i++)
                {
                    if (src[i].Equals(test[0]) || test[0].Equals(src[i]))
                    {
                        int j;
                        for (j = 1; j < test.Count; j++)
                            if (!(src[i + j].Equals(test[j]) || test[j].Equals(src[i + j])))
                                break;

                        if (j == test.Count)
                        {
                            index = i;
                            break;
                        }
                    }
                }
            }

            return index;
        }

        [APILevel(APIFlags.LSL, "llList2Integer")]
        [LSLTooltip("Returns an integer that is at index in src")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal int List2Integer(ScriptInstance instance,
            [LSLTooltip("List containing the element of interest")]
            AnArray src,
            [LSLTooltip("Index of the element of interest.")]
            int index)
        {
            if (index < 0)
            {
                index = src.Count - index;
            }

            if (index < 0 || index >= src.Count)
            {
                return 0;
            }

            if(src[index] is Real)
            {
                return LSLCompiler.ConvToInt((Real)src[index]);
            }
            else if (src[index] is AString)
            {
                return LSLCompiler.ConvToInt(src[index].ToString());
            }
            else
            {
                return src[index].AsInteger;
            }
        }

        [APILevel(APIFlags.LSL, "llList2Key")]
        [LSLTooltip("Returns a key that is at index in src")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal LSLKey List2Key(ScriptInstance instance,
            [LSLTooltip("List containing the element of interest")]
            AnArray src,
            [LSLTooltip("Index of the element of interest.")]
            int index)
        {
            if (index < 0)
            {
                index = src.Count - index;
            }

            if (index < 0 || index >= src.Count)
            {
                return UUID.Zero;
            }

            return src[index].ToString();
        }

        [APILevel(APIFlags.LSL, "llList2Rot")]
        [LSLTooltip("Returns a rotation that is at index in src")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal Quaternion List2Rot(ScriptInstance instance,
            [LSLTooltip("List containing the element of interest")]
            AnArray src,
            [LSLTooltip("Index of the element of interest.")]
            int index)
        {
            if (index < 0)
            {
                index = src.Count - index;
            }

            if (index < 0 || index >= src.Count)
            {
                return Quaternion.Identity;
            }

            return src[index].AsQuaternion;
        }

        [APILevel(APIFlags.LSL, "llList2String")]
        [LSLTooltip("Returns a string that is at index in src")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal string List2String(ScriptInstance instance,
            [LSLTooltip("List containing the element of interest")]
            AnArray src,
            [LSLTooltip("Index of the element of interest.")]
            int index)
        {
            if (index < 0)
            {
                index = src.Count - index;
            }

            if (index < 0 || index >= src.Count)
            {
                return string.Empty;
            }

            return src[index].AsString.ToString();
        }

        [APILevel(APIFlags.LSL, "llList2Vector")]
        [LSLTooltip("Returns a vector that is at index in src")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal Vector3 List2Vector(ScriptInstance instance,
            [LSLTooltip("List containing the element of interest")]
            AnArray src,
            [LSLTooltip("Index of the element of interest.")]
            int index)
        {
            if (index < 0)
            {
                index = src.Count - index;
            }

            if (index < 0 || index >= src.Count)
            {
                return Vector3.Zero;
            }

            return src[index].AsVector3;
        }

        [APILevel(APIFlags.LSL, "llDumpList2String")]
        [LSLTooltip("Returns a string that is the list src converted to a string with separator between the entries.")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal string DumpList2String(ScriptInstance instance, AnArray src, string separator)
        {
            string s = string.Empty;

            foreach(IValue val in src)
            {
                if(!string.IsNullOrEmpty(s))
                {
                    s += separator;
                }
                s += val.ToString();
            }
            return s;
        }

        [APILevel(APIFlags.LSL, "llList2CSV")]
        [LSLTooltip("Returns a string of comma separated values taken in order from src.")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal string List2CSV(ScriptInstance instance, AnArray src)
        {
            return DumpList2String(instance, src, ", ");
        }

        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int TYPE_INTEGER = 1;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int TYPE_FLOAT = 2;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int TYPE_STRING = 3;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int TYPE_KEY = 4;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int TYPE_VECTOR = 5;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int TYPE_ROTATION = 6;
        [APILevel(APIFlags.LSL, APILevel.KeepCsName)]
        internal const int TYPE_INVALID = 0;

        [APILevel(APIFlags.LSL, "llGetListEntryType")]
        [LSLTooltip("Returns the type (an integer) of the entry at index in src.")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal int GetListEntryType(ScriptInstance instance,
            [LSLTooltip("List containing the element of interest")]
            AnArray src,
            [LSLTooltip("Index of the element of interest")]
            int index)
        {
            if (index < 0)
            {
                index = src.Count - index;
            }

            if (index < 0 || index >= src.Count)
            {
                return TYPE_INVALID;
            }

            return (int)src[index].LSL_Type;
        }

        [APILevel(APIFlags.LSL, "llGetListLength")]
        [LSLTooltip("Returns an integer that is the number of elements in the list src")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal int GetListLength(ScriptInstance instance, AnArray src)
        {
            return src.Count;
        }

        AnArray ParseString2List(ScriptInstance instance, string src, AnArray separators, AnArray spacers, bool keepNulls)
        {
            AnArray res = new AnArray();
            string value = null;
            
            while(src.Length != 0)
            {
                IValue foundSpacer = null;
                foreach(IValue spacer in spacers)
                {
                    if(spacer.LSL_Type != LSLValueType.String)
                    {
                        continue;
                    }
                    if(src.StartsWith(spacer.ToString()))
                    {
                        foundSpacer = spacer;
                        break;
                    }
                }

                if (foundSpacer != null)
                {
                    src = src.Substring(foundSpacer.ToString().Length);
                    continue;
                }

                IValue foundSeparator = null;
                foreach(IValue separator in separators)
                {
                    if(separator.LSL_Type != LSLValueType.String)
                    {
                        continue;
                    }

                    if(src.StartsWith(separator.ToString()))
                    {
                        foundSeparator = separator;
                        break;
                    }
                }

                if(foundSeparator != null)
                {
                    if(value == null && keepNulls)
                    {
                        res.Add(value);
                    }
                    else if(value != null)
                    {
                        res.Add(value);
                    }
                    value = null;
                    src = src.Substring(foundSeparator.ToString().Length);
                    if(src.Length == 0)
                    {
                        /* special case we consumed all entries but a separator at end */
                        if(keepNulls)
                        {
                            res.Add(string.Empty);
                        }
                    }
                }

                int minIndex = src.Length;

                foreach(IValue spacer in spacers)
                {
                    if (spacer.LSL_Type != LSLValueType.String)
                    {
                        continue;
                    }
                    int resIndex = src.IndexOf(spacer.ToString());
                    if(resIndex < 0)
                    {
                        continue;
                    }
                    else if(resIndex < minIndex)
                    {
                        minIndex = resIndex;
                    }
                }
                foreach(IValue separator in separators)
                {
                    if(spacers.LSL_Type != LSLValueType.String)
                    {
                        continue;
                    }
                    int resIndex = src.IndexOf(separator.ToString());
                    if (resIndex < 0)
                    {
                        continue;
                    }
                    else if (resIndex < minIndex)
                    {
                        minIndex = resIndex;
                    }
                }

                value = src.Substring(0, minIndex);
                src = src.Substring(minIndex);
            }

            if (value != null)
            {
                res.Add(value);
            }

            return res;
        }

        [APILevel(APIFlags.LSL, "llParseString2List")]
        [LSLTooltip("Returns a list that is src broken into a list of strings, discarding separators, keeping spacers, discards any null (empty string) values generated.")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal AnArray ParseString2List(ScriptInstance instance,
            [LSLTooltip("source string")]
            string src,
            [LSLTooltip("separators to be discarded")]
            AnArray separators,
            [LSLTooltip("spacers to be kept")]
            AnArray spacers)
        {
            return ParseString2List(instance, src, separators, spacers, false);
        }

        [APILevel(APIFlags.LSL, "llParseStringKeepNulls")]
        [LSLTooltip("Returns a list that is src broken into a list, discarding separators, keeping spacers, keeping any null values generated.")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal AnArray ParseStringKeepNulls(ScriptInstance instance,
            [LSLTooltip("source string")]
            string src,
            [LSLTooltip("separators to be discarded")]
            AnArray separators,
            [LSLTooltip("spacers to be kept")]
            AnArray spacers)
        {
            return ParseString2List(instance, src, separators, spacers, true);
        }

        [APILevel(APIFlags.LSL, "llCSV2List")]
        [LSLTooltip("This function takes a string of values separated by commas, and turns it into a list.")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidUncalledPrivateCodeRule")]
        internal AnArray CSV2List(ScriptInstance instance, string src)
        {
            bool wsconsume = true;
            bool inbracket = false;
            string value = string.Empty;
            AnArray ret = new AnArray();

            foreach(char c in src)
            {
                switch(c)
                {
                    case ' ': case '\t':
                        if(wsconsume)
                        {
                            break;
                        }
                        value += c.ToString();
                        break;

                    case '<':
                        inbracket = true;
                        value += c.ToString();
                        break;

                    case '>':
                        inbracket = false;
                        value += c.ToString();
                        break;

                    case ',':
                        if(inbracket)
                        {
                            value += c.ToString();
                            break;
                        }

                        ret.Add(value);
                        wsconsume = true;
                        break;

                    default:
                        wsconsume = false;
                        value += c.ToString();
                        break;
                }
            }

            ret.Add(string.Empty);
            return ret;
        }
    }
}