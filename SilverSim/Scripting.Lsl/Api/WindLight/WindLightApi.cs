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

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.WindLight;
using SilverSim.Types;
using System;
using System.ComponentModel;

namespace SilverSim.Scripting.Lsl.Api.WindLight
{
    [ScriptApiName("WindLight")]
    [LSLImplementation]
    [Description("ASSL WindLight API")]
    public class WindLightApi : IScriptApi, IPlugin
    {
        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }

        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_AMBIENT = 0;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_BLUE_DENSITY = 1;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_BLUR_HORIZON = 2;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_CLOUD_COLOR = 3;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_CLOUD_POS_DENSITY1 = 4;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_CLOUD_POS_DENSITY2 = 5;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_CLOUD_SCALE = 6;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_CLOUD_SCROLL_X = 7;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_CLOUD_SCROLL_Y = 8;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_CLOUD_SCROLL_X_LOCK = 9;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_CLOUD_SCROLL_Y_LOCK = 10;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_CLOUD_SHADOW = 11;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_DENSITY_MULTIPLIER = 12;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_DISTANCE_MULTIPLIER = 13;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_GAMMA = 14;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_GLOW = 15;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_HAZE_DENSITY = 16;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_HAZE_HORIZON = 17;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_LIGHT_NORMALS = 18;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_MAX_ALTITUDE = 19;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_STAR_BRIGHTNESS = 20;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_SKY_SUNLIGHT_COLOR = 21;

        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_BLUR_MULTIPLIER = 22;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_FRESNEL_OFFSET = 23;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_FRESNEL_SCALE = 24;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_NORMAL_MAP = 25;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_NORMAL_SCALE = 26;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_SCALE_ABOVE = 27;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_SCALE_BELOW = 28;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_UNDERWATER_FOG_MODIFIER = 29;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_FOG_COLOR = 30;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_FOG_DENSITY = 31;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_BIG_WAVE_DIRECTION = 32;
        [APIExtension(APIExtension.WindLight_New)]
        public const int REGION_WL_WATER_LITTLE_WAVE_DIRECTION = 33;

        [APIExtension(APIExtension.WindLight_New, "rwlWindlightGetWaterSettings")]
        public AnArray WindlightGetWaterSettings(ScriptInstance instance, AnArray rules)
        {
            var res = new AnArray();
            EnvironmentSettings envsettings;
            lock(instance)
            {
                envsettings = instance.Part.ObjectGroup.Scene.EnvironmentSettings;
            }

            if(envsettings == null)
            {
                return res;
            }

            WaterEntry waterSettings = envsettings.WaterSettings;

            foreach(IValue iv in rules)
            {
                if (!(iv is Integer))
                {
                    lock (instance)
                    {
                        instance.ShoutError(string.Format("Invalid parameter type {0}", iv.LSL_Type.ToString()));
                        return res;
                    }
                }

                switch(iv.AsInt)
                {
                    case REGION_WL_WATER_BLUR_MULTIPLIER:
                        res.Add(waterSettings.BlurMultiplier);
                        break;

                    case REGION_WL_WATER_FRESNEL_OFFSET:
                        res.Add(waterSettings.FresnelOffset);
                        break;

                    case REGION_WL_WATER_FRESNEL_SCALE:
                        res.Add(waterSettings.FresnelScale);
                        break;

                    case REGION_WL_WATER_NORMAL_MAP:
                        res.Add(waterSettings.NormalMap);
                        break;

                    case REGION_WL_WATER_UNDERWATER_FOG_MODIFIER:
                        res.Add(waterSettings.UnderwaterFogModifier);
                        break;

                    case REGION_WL_WATER_SCALE_ABOVE:
                        res.Add(waterSettings.ScaleAbove);
                        break;

                    case REGION_WL_WATER_SCALE_BELOW:
                        res.Add(waterSettings.ScaleBelow);
                        break;

                    case REGION_WL_WATER_FOG_DENSITY:
                        res.Add(waterSettings.WaterFogDensity);
                        break;

                    case REGION_WL_WATER_FOG_COLOR:
                        {
                            Vector4 col = waterSettings.WaterFogColor;
                            res.Add(new Quaternion(
                                col.X,
                                col.Y,
                                col.Z,
                                col.W));
                        }
                        break;

                    case REGION_WL_WATER_BIG_WAVE_DIRECTION:
                        res.Add(waterSettings.Wave1Direction);
                        break;

                    case REGION_WL_WATER_LITTLE_WAVE_DIRECTION:
                        res.Add(waterSettings.Wave2Direction);
                        break;

                    case REGION_WL_WATER_NORMAL_SCALE:
                        res.Add(waterSettings.NormScale);
                        break;

                    default:
                        instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0", "Invalid parameter type {0}", iv.AsInt));
                        return res;
                }
            }
            return res;
        }

        [APIExtension(APIExtension.WindLight_New, "rwlWindlightSetWaterSettings")]
        public int WindlightSetWaterSettings(ScriptInstance instance, AnArray rules)
        {
            EnvironmentSettings envsettings;
            lock (instance)
            {
                envsettings = instance.Part.ObjectGroup.Scene.EnvironmentSettings ?? new EnvironmentSettings();
            }

            if(rules.Count % 2 != 0)
            {
                return 0;
            }

            WaterEntry waterSettings = envsettings.WaterSettings;

            for (int paraidx = 0; paraidx < rules.Count; paraidx += 2 )
            {
                IValue ivtype = rules[paraidx];
                IValue ivvalue = rules[paraidx + 1];
                if (!(ivtype is Integer))
                {
                    lock (instance)
                    {
                        instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0", "Invalid parameter type {0}", ivtype.LSL_Type.ToString()));
                        return 0;
                    }
                }

                switch (ivtype.AsInt)
                {
                    case REGION_WL_WATER_BLUR_MULTIPLIER:
                        if(ivvalue.GetType() != typeof(Real))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_BLUR_MODIFIER"));
                            return 0;
                        }
                        waterSettings.BlurMultiplier = ivvalue.AsReal;
                        break;

                    case REGION_WL_WATER_FRESNEL_OFFSET:
                        if(ivvalue.GetType() != typeof(Real))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_FRESNEL_OFFSET"));
                            return 0;
                        }
                        waterSettings.FresnelOffset = ivvalue.AsReal;
                        break;

                    case REGION_WL_WATER_FRESNEL_SCALE:
                        if(ivvalue.GetType() != typeof(Real))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_FRESNEL_SCALE"));
                            return 0;
                        }
                        waterSettings.FresnelScale = ivvalue.AsReal;
                        break;

                    case REGION_WL_WATER_NORMAL_MAP:
                        lock (instance)
                        {
                            try
                            {
                                waterSettings.NormalMap = instance.GetTextureAssetID(ivvalue.ToString());
                            }
                            catch(Exception e)
                            {
                                instance.ShoutError(e.Message);
                                return 0;
                            }
                        }
                        break;

                    case REGION_WL_WATER_UNDERWATER_FOG_MODIFIER:
                        if(ivvalue.GetType() != typeof(Real))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_UNDERWATER_FOG_MODIFIER"));
                            return 0;
                        }
                        waterSettings.UnderwaterFogModifier = ivvalue.AsReal;
                        break;

                    case REGION_WL_WATER_SCALE_ABOVE:
                        if(ivvalue.GetType() != typeof(Real))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_SCALE_ABOVE"));
                            return 0;
                        }
                        waterSettings.ScaleAbove = ivvalue.AsReal;
                        break;

                    case REGION_WL_WATER_SCALE_BELOW:
                        if(ivvalue.GetType() != typeof(Real))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_SCALE_BELOW"));
                            return 0;
                        }
                        waterSettings.ScaleBelow = ivvalue.AsReal;
                        break;

                    case REGION_WL_WATER_FOG_DENSITY:
                        if(ivvalue.GetType() != typeof(Real))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_FOG_DENSITY"));
                            return 0;
                        }
                        waterSettings.WaterFogDensity = ivvalue.AsReal;
                        break;

                    case REGION_WL_WATER_FOG_COLOR:
                        if(ivvalue.GetType() != typeof(Quaternion))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_FOG_COLOR"));
                            return 0;
                        }

                        {
                            Quaternion q = ivvalue.AsQuaternion;
                            waterSettings.WaterFogColor = new Vector4(q.X.Clamp(0, 1), q.Y.Clamp(0, 1), q.Z.Clamp(0,1), q.W.Clamp(0, 1));
                        }
                        break;

                    case REGION_WL_WATER_BIG_WAVE_DIRECTION:
                        if(ivvalue.GetType() != typeof(Vector3))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_BIG_WAVE_DIRECTION"));
                            return 0;
                        }
                        waterSettings.Wave1Direction = ivvalue.AsVector3;
                        break;

                    case REGION_WL_WATER_LITTLE_WAVE_DIRECTION:
                        if(ivvalue.GetType() != typeof(Vector3))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_LITTLE_WAVE_DIRECTION"));
                            return 0;
                        }
                        waterSettings.Wave2Direction = ivvalue.AsVector3;
                        break;

                    case REGION_WL_WATER_NORMAL_SCALE:
                        if(ivvalue.GetType() != typeof(Vector3))
                        {
                            instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0For1", "Invalid parameter type {0} for {1}", ivvalue.LSL_Type.ToString(), "REGION_WL_WATER_NORMAL_SCALE"));
                            return 0;
                        }
                        waterSettings.NormScale = ivvalue.AsVector3;
                        break;

                    default:
                        instance.ShoutError(new LocalizedScriptMessage(this, "InvalidParameterType0", "Invalid parameter type {0}", ivtype.AsInt));
                        return 0;
                }
            }

            envsettings.WaterSettings = waterSettings;
            lock(instance)
            {
                SceneInterface scene = instance.Part.ObjectGroup.Scene;
                scene.EnvironmentSettings = envsettings;
                /* Windlight settings are updated when we send a new RegionInfo to a viewer */
                scene.TriggerRegionSettingsChanged();
            }
            return 1;
        }
    }
}
