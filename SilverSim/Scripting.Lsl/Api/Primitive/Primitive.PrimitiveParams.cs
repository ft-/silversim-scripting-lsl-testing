﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Types;
using SilverSim.Types.Primitive;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Scripting.Lsl.Api.Primitive
{
    public partial class PrimitiveApi
    {
        #region Primitives

        [APILevel(APIFlags.LSL, "llGetLocalRot")]
        public Quaternion GetLocalRot(ScriptInstance instance)
        {
            lock(instance)
            {
                return instance.Part.LocalRotation;
            }
        }

        [APILevel(APIFlags.LSL, "llSetLocalRot")]
        [ForcedSleep(0.2)]
        public void SetLocalRot(ScriptInstance instance, Quaternion rot)
        {
            lock(instance)
            {
                instance.Part.LocalRotation = rot;
            }
        }

        [APILevel(APIFlags.LSL, "llGetKey")]
        public LSLKey GetKey(ScriptInstance instance)
        {
            lock (instance)
            {
                return instance.Part.ID;
            }
        }

        [APILevel(APIFlags.LSL, "llAllowInventoryDrop")]
        public void AllowInventoryDrop(ScriptInstance instance, int add)
        {
            lock (instance)
            {
                instance.Part.IsAllowedDrop = add != 0;
            }
        }

        [APILevel(APIFlags.LSL, "llGetLinkPrimitiveParams")]
        public AnArray GetLinkPrimitiveParams(ScriptInstance instance, int link, AnArray param)
        {
            AnArray parout = new AnArray();
            lock (instance)
            {
                instance.Part.ObjectGroup.GetPrimitiveParams(instance.Part.LinkNumber, link, param.GetEnumerator(), ref parout);
            }
            return parout;
        }

        [APILevel(APIFlags.LSL, "llGetPrimitiveParams")]
        [ForcedSleep(0.2)]
        public AnArray GetPrimitiveParams(ScriptInstance instance, AnArray param)
        {
            AnArray parout = new AnArray();
            lock (instance)
            {
                instance.Part.ObjectGroup.GetPrimitiveParams(instance.Part.LinkNumber, LINK_THIS, param.GetEnumerator(), ref parout);
            }
            return parout;
        }

        [APILevel(APIFlags.OSSL, "osGetLinkPrimitiveParams")]
        public AnArray OsGetLinkPrimitiveParams(ScriptInstance instance, int linknumber, AnArray param)
        {
            throw new NotImplementedException();
        }

        [APILevel(APIFlags.LSL, "llGetLocalPos")]
        public Vector3 GetLocalPos(ScriptInstance instance)
        {
            lock (instance)
            {
                return instance.Part.LocalPosition;
            }
        }

        [APILevel(APIFlags.LSL, "llGetPos")]
        public Vector3 GetPos(ScriptInstance instance)
        {
            lock (instance)
            {
                return instance.Part.Position;
            }
        }

        [APILevel(APIFlags.LSL, "llGetRootPosition")]
        public Vector3 GetRootPosition(ScriptInstance instance)
        {
            lock (instance)
            {
                return instance.Part.ObjectGroup.Position;
            }
        }

        [APILevel(APIFlags.LSL, "llGetRootRotation")]
        public Quaternion GetRootRotation(ScriptInstance instance)
        {
            lock (instance)
            {
                return instance.Part.ObjectGroup.RootPart.Rotation;
            }
        }

        [APILevel(APIFlags.LSL, "llGetRot")]
        public Quaternion GetRot(ScriptInstance instance)
        {
            lock (instance)
            {
                return instance.Part.Rotation;
            }
        }

        [APILevel(APIFlags.LSL, "llGetScale")]
        public Vector3 GetScale(ScriptInstance instance)
        {
            lock (instance)
            {
                return instance.Part.Size;
            }
        }

        [APILevel(APIFlags.LSL, "llPassCollisions")]
        public void PassCollisions(ScriptInstance instance, int pass)
        {
            lock (instance)
            {
                instance.Part.IsPassCollisions = pass != 0;
            }
        }

        [APILevel(APIFlags.LSL, "llPassTouches")]
        public void PassTouches(ScriptInstance instance, int pass)
        {
            lock (instance)
            {
                instance.Part.IsPassTouches = pass != 0;
            }
        }

        [APILevel(APIFlags.LSL, "llSetClickAction")]
        public void SetClickAction(ScriptInstance instance, int action)
        {
            lock (instance)
            {
                instance.Part.ClickAction = (ClickActionType)action;
            }
        }

        [APILevel(APIFlags.LSL, "llSetPayPrice")]
        [SuppressMessage("Gendarme.Rules.Performance", "AvoidRepetitiveCallsToPropertiesRule")]
        public void SetPayPrice(ScriptInstance instance, int price, AnArray quick_pay_buttons)
        {
            lock (instance)
            {
                ObjectGroup thisGroup = instance.Part.ObjectGroup;
                if (quick_pay_buttons.Count < 4)
                {
                    instance.ShoutError("llSetPayPrice: List must have at least 4 elements.");
                    return;
                }
                thisGroup.PayPrice0 = price;
                thisGroup.PayPrice1 = quick_pay_buttons[0].AsInt;
                thisGroup.PayPrice2 = quick_pay_buttons[1].AsInt;
                thisGroup.PayPrice3 = quick_pay_buttons[2].AsInt;
                thisGroup.PayPrice4 = quick_pay_buttons[3].AsInt;
            }
        }

        [APILevel(APIFlags.LSL, "llSetPos")]
        public void SetPos(ScriptInstance instance, Vector3 pos)
        {
            lock (instance)
            {
                instance.Part.Position = pos;
            }
        }

        [APILevel(APIFlags.LSL, "llSetRot")]
        public void SetRot(ScriptInstance instance, Quaternion rot)
        {
            lock(instance)
            {
                instance.Part.Rotation = rot;
            }
        }

        [APILevel(APIFlags.OSSL, "osSetPrimitiveParams")]
        public void SetPrimitiveParams(ScriptInstance instance, LSLKey key, AnArray rules)
        {
            lock(instance)
            {
                ObjectPart thisPart = instance.Part;
                SceneInterface scene = thisPart.ObjectGroup.Scene;
                ObjectPart part;
                if (!scene.Primitives.TryGetValue(key.AsUUID, out part))
                {
                    return;
                }
                if (part.Owner != thisPart.Owner)
                {
                    return;
                }
                using(AnArray.MarkEnumerator enumerator = rules.GetMarkEnumerator())
                {
                    part.SetPrimitiveParams(enumerator);
                }
            }
        }

        [APILevel(APIFlags.OSSL, "osGetPrimitiveParams")]
        public AnArray GetPrimitiveParams(ScriptInstance instance, LSLKey key, AnArray paramList)
        {
            lock (instance)
            {
                ObjectPart thisPart = instance.Part;
                SceneInterface scene = thisPart.ObjectGroup.Scene;
                ObjectPart part;
                AnArray res = new AnArray();
                if(!scene.Primitives.TryGetValue(key.AsUUID, out part))
                {
                    return res;
                }
                if (part.Owner != thisPart.Owner)
                {
                    return res;
                }

                using(AnArray.Enumerator enumerator = paramList.GetEnumerator())
                {
                    part.GetPrimitiveParams(enumerator, ref res);
                }
                return res;
            }
        }

        [APILevel(APIFlags.LSL, "llSetPrimitiveParams")]
        [ForcedSleep(0.2)]
        public void SetPrimitiveParams(ScriptInstance instance, AnArray rules)
        {
            lock (instance)
            {
                ObjectPart thisPart = instance.Part;
                using(AnArray.MarkEnumerator enumerator = rules.GetMarkEnumerator())
                {
                    thisPart.ObjectGroup.SetPrimitiveParams(thisPart.LinkNumber, LINK_THIS, enumerator);
                }
            }
        }

        [APILevel(APIFlags.LSL, "llSetLinkPrimitiveParams")]
        [ForcedSleep(0.2)]
        public void SetLinkPrimitiveParams(ScriptInstance instance, int linkTarget, AnArray rules)
        {
            SetLinkPrimitiveParamsFast(instance, linkTarget, rules);
        }

        [APILevel(APIFlags.LSL, "llSetLinkPrimitiveParamsFast")]
        public void SetLinkPrimitiveParamsFast(ScriptInstance instance, int linkTarget, AnArray rules)
        {
            lock (instance)
            {
                ObjectPart thisPart = instance.Part;
                using (AnArray.MarkEnumerator enumerator = rules.GetMarkEnumerator())
                {
                    thisPart.ObjectGroup.SetPrimitiveParams(thisPart.LinkNumber, linkTarget, enumerator);
                }
            }
        }

        [APILevel(APIFlags.LSL, "llSetScale")]
        public void SetScale(ScriptInstance instance, Vector3 size)
        {
            lock (instance)
            {
                instance.Part.Size = size;
            }
        }

        [APILevel(APIFlags.LSL, "llSetSitText")]
        public void SetSitText(ScriptInstance instance, string text)
        {
            lock (instance)
            {
                instance.Part.ObjectGroup.RootPart.SitText = text;
            }
        }

        [APILevel(APIFlags.LSL, "llSetText")]
        public void SetText(ScriptInstance instance, string text, Vector3 color, double alpha)
        {
            ObjectPart.TextParam tp = new ObjectPart.TextParam();
            tp.Text = text;
            tp.TextColor = new ColorAlpha(color, alpha);
            lock (instance)
            {
                instance.Part.Text = tp;
            }
        }

        [APILevel(APIFlags.LSL, "llSetTouchText")]
        public void SetTouchText(ScriptInstance instance, string text)
        {
            lock (instance)
            {
                instance.Part.ObjectGroup.RootPart.TouchText = text;
            }
        }

        [APILevel(APIFlags.LSL, "llTargetOmega")]
        public void TargetOmega(ScriptInstance instance, Vector3 axis, double spinrate, double gain)
        {
            ObjectPart.OmegaParam op = new ObjectPart.OmegaParam();
            op.Axis = axis;
            op.Spinrate = spinrate;
            op.Gain = gain;
            lock (instance)
            {
                instance.Part.Omega = op;
            }
        }
        #endregion
    }
}
