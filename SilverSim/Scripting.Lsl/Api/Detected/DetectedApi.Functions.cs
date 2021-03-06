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

namespace SilverSim.Scripting.Lsl.Api.Detected
{
    public partial class DetectedApi
    {
        /* REMARKS: The internal attribute for the LSLScript has been done deliberately here.
         * The other option of implementing this would have been to make it a namespace class of the Script class.
         */
        [APILevel(APIFlags.LSL, "llDetectedGrab")]
        public Vector3 DetectedGrab(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].GrabOffset;
                }
                return Vector3.Zero;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedGroup")]
        public int DetectedGroup(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].Group.Equals(instance.Part.Group).ToLSLBoolean();
                }
                return 0;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedKey")]
        public LSLKey DetectedKey(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].Key;
                }
                return UUID.Zero;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedLinkNumber")]
        public int DetectedLinkNumber(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].LinkNumber;
                }
                return -1;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedName")]
        public string DetectedName(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].Name;
                }
                return string.Empty;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedOwner")]
        public LSLKey DetectedOwner(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].Owner.ID;
                }
                return UUID.Zero;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedPos")]
        public Vector3 DetectedPos(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].Position;
                }
                return Vector3.Zero;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedRot")]
        public Quaternion DetectedRot(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].Rotation;
                }
                return Quaternion.Identity;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedTouchBinormal")]
        public Vector3 DetectedTouchBinormal(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].TouchBinormal;
                }
                return Vector3.Zero;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedTouchFace")]
        public int DetectedTouchFace(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].TouchFace;
                }
                return -1;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedTouchNormal")]
        public Vector3 DetectedTouchNormal(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].TouchNormal;
                }
                return Vector3.Zero;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedTouchPos")]
        public Vector3 DetectedTouchPos(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].TouchPosition;
                }
            }
            return Vector3.Zero;
        }

        [APILevel(APIFlags.LSL, "llDetectedTouchST")]
        public Vector3 DetectedTouchST(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].TouchST;
                }
                return Vector3.Zero;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedTouchUV")]
        public Vector3 DetectedTouchUV(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].TouchUV;
                }
                return Vector3.Zero;
            }
        }

        [APILevel(APIFlags.LSL)]
        [APILevel(APIFlags.LSL, "AGENT_BY_LEGACY_NAME")]
        public const int AGENT = 1;
        [APILevel(APIFlags.LSL)]
        public const int ACTIVE = 2;
        [APILevel(APIFlags.LSL)]
        public const int PASSIVE = 4;
        [APILevel(APIFlags.LSL)]
        public const int SCRIPTED = 8;
        [APILevel(APIFlags.LSL)]
        public const int AGENT_BY_USERNAME = 0x10;
        [APILevel(APIFlags.LSL)]
        public const int NPC = 0x20;

        [APILevel(APIFlags.LSL, "llDetectedType")]
        public int DetectedType(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return (int)script.m_Detected[number].ObjType;
                }
                return 0;
            }
        }

        [APILevel(APIFlags.LSL, "llDetectedVel")]
        public Vector3 DetectedVel(ScriptInstance instance, int number)
        {
            var script = (Script)instance;
            lock (script)
            {
                if (script.m_Detected.Count > number && number >= 0)
                {
                    return script.m_Detected[number].Velocity;
                }
                return Vector3.Zero;
            }
        }
    }
}
