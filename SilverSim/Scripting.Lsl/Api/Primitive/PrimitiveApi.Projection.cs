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

#pragma warning disable IDE0018, RCS1029

using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Object.Parameters;
using SilverSim.Scene.Types.Script;
using SilverSim.Types;

namespace SilverSim.Scripting.Lsl.Api.Primitive
{
    public partial class PrimitiveApi
    {
        [APILevel(APIFlags.OSSL, "osSetProjectionParams")]
        public void SetProjectionParams(ScriptInstance instance, int projection, LSLKey texture, double fov, double focus, double amb)
        {
            SetLinkProjectionParams(instance, LINK_THIS, projection, texture, fov, focus, amb);
        }

        [APILevel(APIFlags.OSSL, "osSetLinkProjectionParams")]
        public void SetLinkProjectionParams(ScriptInstance instance, int link, int projection, LSLKey texture, double fov, double focus, double amb)
        {
            lock (instance)
            {
                var p = new ProjectionParam
                {
                    IsProjecting = projection != 0,
                    ProjectionTextureID = instance.GetTextureAssetID(texture.ToString()),
                    ProjectionFOV = fov,
                    ProjectionFocus = focus,
                    ProjectionAmbience = amb
                };

                foreach (ObjectPart part in instance.GetLinkTargets(link))
                {
                    part.Projection = p;
                }
            }
        }

        [APILevel(APIFlags.OSSL, "osSetProjectionParams")]
        public void SetProjectionParams(ScriptInstance instance, LSLKey prim, int projection, LSLKey texture, double fov, double focus, double amb)
        {
            lock (instance)
            {
                if (UUID.Zero != prim)
                {
                    ((Script)instance).CheckThreatLevel("osSetProjectionParams");
                }

                ObjectPart part;
                if (prim == UUID.Zero)
                {
                    part = instance.Part;
                }
                else if(!instance.Part.ObjectGroup.Scene.Primitives.TryGetValue(prim, out part))
                {
                    return;
                }

                part.Projection = new ProjectionParam
                {
                    IsProjecting = projection != 0,
                    ProjectionTextureID = instance.GetTextureAssetID(texture.ToString()),
                    ProjectionFOV = fov,
                    ProjectionFocus = focus,
                    ProjectionAmbience = amb
                };
            }
        }
    }
}
