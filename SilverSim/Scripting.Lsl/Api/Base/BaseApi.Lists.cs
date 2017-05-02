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
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace SilverSim.Scripting.Lsl.Api.Base
{
    public partial class BaseApi
    {
        [APILevel(APIFlags.LSL, "llDeleteSubList")]
        [Description("Returns a list that is a copy of src but with the slice from start to end removed.")]
        public AnArray DeleteSubList(ScriptInstance instance,
            [Description("source")]
            AnArray src,
            [Description("start index")]
            int start,
            [Description("end index")]
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
        [Description("Returns a list of all the entries in the strided list whose index is a multiple of stride in the range start to end.")]
        public AnArray List2ListStrided(ScriptInstance instance,
            AnArray src,
            [Description("start index")]
            int start,
            [Description("end index")]
            int end,
            [Description("number of entries per stride, if less than 1 it is assumed to be 1")]
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
        public AnArray List2List(ScriptInstance instance, AnArray src, int start, int end)
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
        public double List2Float(ScriptInstance instance, AnArray src, int index)
        {
            if(index < 0)
            {
                index = src.Count - index;
            }

            if(index < 0 ||index >=src.Count)
            {
                return 0;
            }

            try
            {
                return src[index].AsReal;
            }
            catch
            {
                return 0;
            }
        }

        [APILevel(APIFlags.LSL, "llListInsertList")]
        public AnArray ListInsertList(ScriptInstance instance, AnArray dest, AnArray src, int index)
        {
            AnArray pref;
            AnArray suff;

            if (index < 0)
            {
                index = index + dest.Count;
                if (index < 0)
                {
                    index = 0;
                }
            }

            if (index != 0)
            {
                pref = List2List(instance, dest, 0, index - 1);
                if (index < dest.Count)
                {
                    suff = List2List(instance, dest, index, -1);
                    return pref + src + suff;
                }
                else
                {
                    return pref + src;
                }
            }
            else
            {
                if (index < dest.Count)
                {
                    suff = List2List(instance, dest, index, -1);
                    return src + suff;
                }
                else
                {
                    return src;
                }
            }
        }

        [APILevel(APIFlags.LSL, "llListFindList")]
        [Description("Returns the integer index of the first instance of test in src.")]
        public int ListFindList(ScriptInstance instance,
            [Description("what to search in (haystack)")]
            AnArray src,
            [Description("what to search for (needle)")]
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
                        {
                            if (!(src[i + j].Equals(test[j]) || test[j].Equals(src[i + j])))
                            {
                                break;
                            }
                        }

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

        [APIExtension(APIExtension.LongInteger, "llList2Long")]
        [Description("Returns an integer that is at index in src")]
        public long List2Long(ScriptInstance instance,
            [Description("List containing the element of interest")]
            AnArray src,
            [Description("Index of the element of interest.")]
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

            if (src[index] is Real)
            {
                return LSLCompiler.ConvToLong((Real)src[index]);
            }
            else if (src[index] is AString)
            {
                return LSLCompiler.ConvToLong(src[index].ToString());
            }
            else
            {
                try
                {
                    return src[index].AsLong;
                }
                catch
                {
                    return 0;
                }
            }
        }

        [APILevel(APIFlags.LSL, "llList2Integer")]
        [Description("Returns an integer that is at index in src")]
        public int List2Integer(ScriptInstance instance,
            [Description("List containing the element of interest")]
            AnArray src,
            [Description("Index of the element of interest.")]
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
                try
                {
                    return src[index].AsInteger;
                }
                catch
                {
                    return 0;
                }
            }
        }

        [APILevel(APIFlags.LSL, "llList2Key")]
        [Description("Returns a key that is at index in src")]
        public LSLKey List2Key(ScriptInstance instance,
            [Description("List containing the element of interest")]
            AnArray src,
            [Description("Index of the element of interest.")]
            int index)
        {
            Script script = (Script)instance;
            if (index < 0)
            {
                index = src.Count - index;
            }

            if (index < 0 || index >= src.Count)
            {
                return UUID.Zero;
            }

            IValue val = src[index];
            Type t = val.GetType();
            if (t == typeof(Real))
            {
                return script.UsesSinglePrecision ?
                    LSLCompiler.SinglePrecision.TypecastFloatToString(val.AsReal) :
                    LSLCompiler.TypecastDoubleToString(val.AsReal);
            }
            else if (t == typeof(Vector3))
            {
                return script.UsesSinglePrecision ?
                    LSLCompiler.SinglePrecision.TypecastVectorToString6Places((Vector3)val) :
                    LSLCompiler.TypecastVectorToString6Places((Vector3)val);
            }
            else if (t == typeof(Quaternion))
            {
                return script.UsesSinglePrecision ?
                    LSLCompiler.SinglePrecision.TypecastRotationToString6Places((Quaternion)val) :
                    LSLCompiler.TypecastRotationToString6Places((Quaternion)val);
            }
            else
            {
                return src[index].ToString();
            }
        }

        [APILevel(APIFlags.LSL, "llList2Rot")]
        [Description("Returns a rotation that is at index in src")]
        public Quaternion List2Rot(ScriptInstance instance,
            [Description("List containing the element of interest")]
            AnArray src,
            [Description("Index of the element of interest.")]
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

            try
            {
                return src[index].AsQuaternion;
            }
            catch
            {
                return Quaternion.Identity;
            }
        }

        [APILevel(APIFlags.LSL, "llList2String")]
        [Description("Returns a string that is at index in src")]
        public string List2String(ScriptInstance instance,
            [Description("List containing the element of interest")]
            AnArray src,
            [Description("Index of the element of interest.")]
            int index)
        {
            Script script = (Script)instance;
            if (index < 0)
            {
                index = src.Count - index;
            }

            if (index < 0 || index >= src.Count)
            {
                return string.Empty;
            }

            IValue val = src[index];
            Type t = val.GetType();
            if (t == typeof(Real))
            {
                return script.UsesSinglePrecision ?
                    LSLCompiler.SinglePrecision.TypecastFloatToString(val.AsReal) :
                    LSLCompiler.TypecastDoubleToString(val.AsReal);
            }
            else if (t == typeof(Vector3))
            {
                return script.UsesSinglePrecision ?
                    LSLCompiler.SinglePrecision.TypecastVectorToString6Places((Vector3)val) :
                    LSLCompiler.TypecastVectorToString6Places((Vector3)val);
            }
            else if (t == typeof(Quaternion))
            {
                return script.UsesSinglePrecision ?
                    LSLCompiler.SinglePrecision.TypecastRotationToString6Places((Quaternion)val) :
                    LSLCompiler.TypecastRotationToString6Places((Quaternion)val);
            }
            else
            {
                return val.ToString();
            }
        }

        [APILevel(APIFlags.LSL, "llList2Vector")]
        [Description("Returns a vector that is at index in src")]
        public Vector3 List2Vector(ScriptInstance instance,
            [Description("List containing the element of interest")]
            AnArray src,
            [Description("Index of the element of interest.")]
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

            try
            {
                return src[index].AsVector3;
            }
            catch
            {
                return Vector3.Zero;
            }
        }

        [APILevel(APIFlags.LSL, "llDumpList2String")]
        [Description("Returns a string that is the list src converted to a string with separator between the entries.")]
        public string DumpList2String(ScriptInstance instance, AnArray src, string separator)
        {
            Script script = (Script)instance;
            StringBuilder sb = new StringBuilder();

            foreach(IValue val in src)
            {
                if(sb.Length != 0)
                {
                    sb.Append(separator);
                }
                Type t = val.GetType();
                if (t == typeof(Real))
                {
                    sb.Append(script.UsesSinglePrecision ? 
                        LSLCompiler.SinglePrecision.TypecastFloatToString(val.AsReal) : 
                        LSLCompiler.TypecastDoubleToString(val.AsReal));
                }
                else if (t == typeof(Vector3))
                {
                    sb.Append(script.UsesSinglePrecision ?
                        LSLCompiler.SinglePrecision.TypecastVectorToString6Places((Vector3)val) :
                        LSLCompiler.TypecastVectorToString6Places((Vector3)val));
                }
                else if (t == typeof(Quaternion))
                {
                    sb.Append(script.UsesSinglePrecision ?
                        LSLCompiler.SinglePrecision.TypecastRotationToString6Places((Quaternion)val) :
                        LSLCompiler.TypecastRotationToString6Places((Quaternion)val));
                }
                else
                {
                    sb.Append(val.ToString());
                }
            }
            return sb.ToString();
        }

        [APILevel(APIFlags.LSL, "llList2CSV")]
        [Description("Returns a string of comma separated values taken in order from src.")]
        public string List2CSV(ScriptInstance instance, AnArray src)
        {
            return DumpList2String(instance, src, ", ");
        }

        [APILevel(APIFlags.LSL)]
        public const int TYPE_INTEGER = 1;
        [APILevel(APIFlags.LSL)]
        public const int TYPE_FLOAT = 2;
        [APILevel(APIFlags.LSL)]
        public const int TYPE_STRING = 3;
        [APILevel(APIFlags.LSL)]
        public const int TYPE_KEY = 4;
        [APILevel(APIFlags.LSL)]
        public const int TYPE_VECTOR = 5;
        [APILevel(APIFlags.LSL)]
        public const int TYPE_ROTATION = 6;
        [APILevel(APIFlags.LSL)]
        public const int TYPE_INVALID = 0;

        [APILevel(APIFlags.LSL, "llGetListEntryType")]
        [Description("Returns the type (an integer) of the entry at index in src.")]
        public int GetListEntryType(ScriptInstance instance,
            [Description("List containing the element of interest")]
            AnArray src,
            [Description("Index of the element of interest")]
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
        [Description("Returns an integer that is the number of elements in the list src")]
        public int GetListLength(ScriptInstance instance, AnArray src)
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
                    if(value != null || keepNulls)
                    {
                        res.Add(value);
                    }
                    value = null;
                    src = src.Substring(foundSeparator.ToString().Length);
                    if(src.Length == 0 && keepNulls)
                    {
                        /* special case we consumed all entries but a separator at end */
                        res.Add(string.Empty);
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
        [Description("Returns a list that is src broken into a list of strings, discarding separators, keeping spacers, discards any null (empty string) values generated.")]
        public AnArray ParseString2List(ScriptInstance instance,
            [Description("source string")]
            string src,
            [Description("separators to be discarded")]
            AnArray separators,
            [Description("spacers to be kept")]
            AnArray spacers)
        {
            return ParseString2List(instance, src, separators, spacers, false);
        }

        [APILevel(APIFlags.LSL, "llParseStringKeepNulls")]
        [Description("Returns a list that is src broken into a list, discarding separators, keeping spacers, keeping any null values generated.")]
        public AnArray ParseStringKeepNulls(ScriptInstance instance,
            [Description("source string")]
            string src,
            [Description("separators to be discarded")]
            AnArray separators,
            [Description("spacers to be kept")]
            AnArray spacers)
        {
            return ParseString2List(instance, src, separators, spacers, true);
        }

        [APILevel(APIFlags.LSL, "llCSV2List")]
        [Description("This function takes a string of values separated by commas, and turns it into a list.")]
        public AnArray CSV2List(ScriptInstance instance, string src)
        {
            bool wsconsume = true;
            bool inbracket = false;
            AnArray ret = new AnArray();
            StringBuilder value = new StringBuilder();

            foreach(char c in src)
            {
                switch(c)
                {
                    case ' ': case '\t':
                        if(wsconsume)
                        {
                            break;
                        }
                        value.Append(c);
                        break;

                    case '<':
                        inbracket = true;
                        value.Append(c);
                        break;

                    case '>':
                        inbracket = false;
                        value.Append(c);
                        break;

                    case ',':
                        if(inbracket)
                        {
                            value.Append(c);
                            break;
                        }

                        ret.Add(value.ToString());
                        value.Clear();
                        wsconsume = true;
                        break;

                    default:
                        wsconsume = false;
                        value.Append(c);
                        break;
                }
            }
            if (value.Length != 0)
            {
                ret.Add(value.ToString());
            }
            return ret;
        }

        [APILevel(APIFlags.LSL, "llListReplaceList")]
        [Description("This function takes a string of values separated by commas, and turns it into a list.")]
        public AnArray ListReplaceList(ScriptInstance instance, AnArray dest, AnArray src, int start, int end)
        {
            AnArray pref;

            // Note that although we have normalized, both
            // indices could still be negative.
            if (start < 0)
            {
                start = start + dest.Count;
            }

            if (end < 0)
            {
                end = end + dest.Count;
            }
            if (start <= end)
            {
                if (start > 0)
                {
                    pref = List2List(instance, dest, 0, start - 1);

                    return (end + 1 < dest.Count) ?
                        (pref + src + List2List(instance, dest, end + 1, -1)) :
                        (pref + src);
                }
                else if (start == 0)
                {
                    return (end + 1 < dest.Count) ?
                        (src + List2List(instance, dest, end + 1, -1)) :
                        src;
                }
                else 
                {
                    return (end + 1 < dest.Count) ?
                        List2List(instance, dest, end + 1, -1) :
                        new AnArray();
                }
            }
            else
            {
                return List2List(instance, dest, end + 1, start - 1) + src;
            }
        }

        static int ElementCompare(IValue left, IValue right)
        {
            Type leftType = left.GetType();
            if (left.GetType() != right.GetType())
            {
                /* as per LSL behaviour, unequal types are considered equal */
                return 0;
            }

            if(leftType == typeof(LSLKey) || leftType == typeof(AString))
            {
                return string.CompareOrdinal(left.ToString(), right.ToString());
            }
            else if(leftType == typeof(Integer))
            {
                return Math.Sign(left.AsInt - right.AsInt);
            }
            else if(leftType == typeof(Real))
            {
                return Math.Sign(left.AsReal - right.AsReal);
            }
            else if(leftType == typeof(Vector3))
            {
                return Math.Sign(left.AsVector3.Length - right.AsVector3.Length);
            }
            else if(leftType == typeof(Quaternion))
            {
                return Math.Sign(left.AsQuaternion.Length - right.AsQuaternion.Length);
            }
            else
            {
                return 0;
            }
        }

        static int ElementCompareDescending(IValue left, IValue right)
        {
            return 0 - ElementCompare(left, right);
        }

        [APILevel(APIFlags.LSL, "llListSort")]
        [Description("Returns a list that is src sorted by stride.")]
        public AnArray ListSort(ScriptInstance instance, 
            [Description("List to be sorted")]
            AnArray src,
            [Description("number of entries per stride. If it is less than 1, it is assumed to be 1.")]
            int stride,
            [Description("If TRUE, the sort order is ascending. Otherwise, the order is descending.")]
            int ascending)
        {
            AnArray res;

            if (0 == src.Count)
            {
                return new AnArray();
            }

            Comparison<IValue> compare;
            compare = (ascending == 1) ?
                (Comparison<IValue>)ElementCompare :
                (Comparison<IValue>)ElementCompareDescending;

            IValue[] ret = src.ToArray();
            if(stride < 2)
            {
                bool homogenousTypeEntries = true;
                int index;
                Type firstEntryType = ret[0].GetType();
                for (index = 1; index < ret.Length; index++)
                {
                    if (firstEntryType != ret[index].GetType())
                    {
                        homogenousTypeEntries = false;
                        break;
                    }
                }

                if (homogenousTypeEntries)
                {
                    Array.Sort(ret, compare);
                    res = new AnArray();
                    res.AddRange(ret);
                    return res;
                }
            }

            int i;
            int j;
            int k;
            int n = ret.Length;

            /* a slow bubble-sort in unoptimized style see LSL wiki for the llSortList description */
            for (i = 0; i < (n - stride); i += stride)
            {
                for (j = i + stride; j < n; j += stride)
                {
                    if (compare(ret[i], ret[j]) > 0)
                    {
                        for (k = 0; k < stride; k++)
                        {
                            IValue tmp = ret[i + k];
                            ret[i + k] = ret[j + k];
                            ret[j + k] = tmp;
                        }
                    }
                }
            }

            res = new AnArray();
            res.AddRange(ret);
            return res;
        }

        [APILevel(APIFlags.LSL, "llListRandomize")]
        public AnArray ListRandomize(ScriptInstance instance, AnArray src, int stride)
        {
            /* From LSL wiki:
             * When you want to randomize the position of every list element, specify a stride of 1. This is perhaps the setting most used.
             * If the stride is not a factor of the list length, the src list is returned. In other words, llGetListLength(src) % stride must be 0.
             * Conceptually, the algorithm selects llGetListLength(src) / stride buckets, and then for each bucket swaps in the contents with another b
             */
            AnArray result;
            Random rand = new Random();

            int chunkcount;
            int[] chunks;

            if (stride <= 0)
            {
                stride = 1;
            }

            if (src.Count != stride && src.Count % stride == 0)
            {
                chunkcount = src.Count / stride;

                chunks = new int[chunkcount];

                for (int i = 0; i < chunkcount; i++)
                {
                    chunks[i] = i;
                }

                /* Knuth shuffle the chunk index */
                for (int i = chunkcount - 1; i >= 1; i--)
                {
                    /* Elect an unrandomized chunk to swap */
                    int index = rand.Next(i + 1);
                    int tmp;

                    /* and swap position with first unrandomized chunk */
                    tmp = chunks[i];
                    chunks[i] = chunks[index];
                    chunks[index] = tmp;
                }

                result = new AnArray();

                /* Construct the randomized list */
                for (int i = 0; i < chunkcount; i++)
                {
                    result.AddRange(src.GetRange(chunks[i] * stride, stride));
                }
            }
            else
            {
                result = new AnArray(src);
            }

            return result;
        }

        [APILevel(APIFlags.LSL)]
        [Description("Calculates the range.")]
        public const int LIST_STAT_RANGE = 0;
        [APILevel(APIFlags.LSL)]
        [Description("Calculates the smallest number.")]
        public const int LIST_STAT_MIN = 1;
        [APILevel(APIFlags.LSL)]
        [Description("Calculates the largest number.")]
        public const int LIST_STAT_MAX = 2;
        [APILevel(APIFlags.LSL)]
        public const int LIST_STAT_MEAN = 3;
        [APILevel(APIFlags.LSL)]
        public const int LIST_STAT_MEDIAN = 4;
        [APILevel(APIFlags.LSL)]
        public const int LIST_STAT_STD_DEV = 5;
        [APILevel(APIFlags.LSL)]
        public const int LIST_STAT_SUM = 6;
        [APILevel(APIFlags.LSL)]
        public const int LIST_STAT_SUM_SQUARES = 7;
        [APILevel(APIFlags.LSL)]
        public const int LIST_STAT_NUM_COUNT = 8;
        [APILevel(APIFlags.LSL)]
        public const int LIST_STAT_GEOMETRIC_MEAN = 9;
        [APILevel(APIFlags.LSL)]
        public const int LIST_STAT_HARMONIC_MEAN = 100;

        [APILevel(APIFlags.LSL, "llListStatistics")]
        public double ListStatistics(ScriptInstance instance, int operation, AnArray src)
        {
            switch(operation)
            {
                case LIST_STAT_RANGE: return ListRange(src);
                case LIST_STAT_MIN: return ListMin(src);
                case LIST_STAT_MAX: return ListMax(src);
                case LIST_STAT_MEAN: return ListMean(src);
                case LIST_STAT_MEDIAN: return ListMedian(src);
                case LIST_STAT_NUM_COUNT: return ListNumericLength(src);
                case LIST_STAT_STD_DEV: return ListStdDev(src);
                case LIST_STAT_SUM: return ListSum(src);
                case LIST_STAT_SUM_SQUARES: return ListSumSquares(src);
                case LIST_STAT_GEOMETRIC_MEAN: return ListGeometricMean(src);
                case LIST_STAT_HARMONIC_MEAN: return ListHarmonicMean(src);
                default: return 0;
            }
        }

        #region llListStatistics function implementation
        bool IsValue(IValue iv, out double v)
        {
            switch (iv.LSL_Type)
            {
                case LSLValueType.Integer:
                case LSLValueType.Float:
                    v = iv.AsReal;
                    return true;

                case LSLValueType.String:
                    return double.TryParse(iv.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);

                default:
                    v = 0;
                    return false;
            }
        }

        double ListMin(AnArray src)
        {
            double minimum = double.PositiveInfinity;
            double entry;

            for (int i = 0; i < src.Count; i++)
            {
                if(IsValue(src[i], out entry) &&
                    entry < minimum)
                {
                    minimum = entry;
                }
            }

            return minimum;
        }

        double ListMax(AnArray src)
        {
            double maximum = double.NegativeInfinity;
            double entry;

            for (int i = 0; i < src.Count; i++)
            {
                if(IsValue(src[i], out entry) &&
                    entry > maximum)
                {
                    maximum = entry;
                }
            }

            return maximum;
        }

        double ListRange(AnArray src)
        {
            double maximum = double.NegativeInfinity;
            double minimum = double.PositiveInfinity;
            double entry;

            for (int i = 0; i < src.Count; i++)
            {
                if (IsValue(src[i], out entry))
                {
                    if (entry > maximum)
                    {
                        maximum = entry;
                    }
                    if (entry > maximum)
                    {
                        maximum = entry;
                    }
                }
            }

            return maximum / minimum;
        }

        int ListNumericLength(AnArray src)
        {
            int count = 0;
            double entry;

            for (int i = 0; i < src.Count; i++)
            {
                if (IsValue(src[i], out entry))
                {
                    count++;
                }
            }

            return count;
        }

        double ListSum(AnArray src)
        {
            double sum = 0;
            double entry;

            for (int i = 0; i < src.Count; i++)
            {
                if (IsValue(src[i], out entry))
                {
                    sum = sum + entry;
                }
            }

            return sum;
        }

        double ListSumSquares(AnArray src)
        {
            double sum = 0;
            double entry;
            for (int i = 0; i < src.Count; i++)
            {
                if (IsValue(src[i], out entry))
                {
                    sum = sum + entry * entry;
                }
            }
            return sum;
        }

        double ListMean(AnArray src)
        {
            double sum = 0;
            double entry;
            int count = 0;

            for (int i = 0; i < src.Count; i++)
            {
                if (IsValue(src[i], out entry))
                {
                    sum = sum + entry;
                    ++count;
                }
            }

            return sum / count;
        }

        double ListMedian(AnArray src)
        {
            return ListQi(src, 0.5);
        }

        double ListGeometricMean(AnArray src)
        {
            double ret = 1.0;
            int count = 0;
            double entry;

            for (int i = 0; i < src.Count; i++)
            {
                if (IsValue(src[i], out entry))
                {
                    ret *= entry;
                    ++count;
                }
            }
            return Math.Exp(Math.Log(ret) / count);
        }

        double ListHarmonicMean(AnArray src)
        {
            double ret = 0.0;
            double entry;
            int count = 0;

            for (int i = 0; i < src.Count; i++)
            {
                if (IsValue(src[i], out entry))
                {
                    ret += 1.0 / entry;
                    ++count;
                }
            }

            return ((double)count / ret);
        }

        double ListVariance(AnArray src)
        {
            double s = 0;
            int count = 0;
            double entry;
            double sum = 0;

            for (int i = 0; i < src.Count; i++)
            {
                if (IsValue(src[i], out entry))
                {
                    s += Math.Pow(entry, 2);
                    sum += entry;
                    ++count;
                }
            }
            return (s - count * Math.Pow(sum / count, 2)) / (count - 1);
        }

        double ListStdDev(AnArray src)
        {
            return Math.Sqrt(ListVariance(src));
        }

        double[] NumericSort(AnArray src)
        {
            List<double> resList = new List<double>();
            double entry;

            for (int i = 0; i < src.Count; i++)
            {
                if (IsValue(src[i], out entry))
                {
                    resList.Add(entry);
                }
            }

            double[] resArray = resList.ToArray();
            Array.Sort(resArray);
            return resArray;
        }

        double ListQi(AnArray src, double i)
        {
            double[] j = NumericSort(src);

            return Math.Abs((Math.Ceiling(j.Length * i) - (j.Length * i))) < Double.Epsilon ?
                (j[(int)(j.Length * i - 1)] + j[(int)(j.Length * i)]) / 2 :
                j[(int)(Math.Ceiling(j.Length * i)) - 1];
        }
        #endregion
    }
}