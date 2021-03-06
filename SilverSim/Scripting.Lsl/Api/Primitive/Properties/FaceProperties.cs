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
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Types;
using SilverSim.Types.Asset.Format;
using SilverSim.Types.Primitive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SilverSim.Scripting.Lsl.Api.Primitive.Properties
{
    [LSLImplementation]
    [ScriptApiName("FaceProperties")]
    [Description("Face Properties API")]
    public class FaceProperties : IPlugin, IScriptApi
    {
        public const int ALL_SIDES = -1;

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }

        [APIExtension(APIExtension.Properties, "normalmap")]
        [APIDisplayName("normalmap")]
        [APIIsVariableType]
        [APIAccessibleMembers(
            "Texture",
            "Repeats",
            "Offset",
            "Rotation")]
        [Serializable]
        [APICloneOnAssignment]
        public class NormalMap
        {
            public LSLKey Texture = PrimitiveApi.TEXTURE_BLANK;
            public Vector3 Repeats = Vector3.One;
            public Vector3 Offset;
            public double Rotation;

            public NormalMap()
            {
            }

            public NormalMap(NormalMap m)
            {
                Texture = m.Texture;
                Offset = m.Offset;
                Repeats = m.Repeats;
                Rotation = m.Rotation;
            }
        }

        [APIExtension(APIExtension.Properties, "specularmap")]
        [APIDisplayName("specularmap")]
        [APIIsVariableType]
        [APIAccessibleMembers(
            "Texture",
            "Repeats",
            "Offset",
            "Rotation",
            "Color",
            "Alpha",
            "Glossiness",
            "Environment")]
        [Serializable]
        [APICloneOnAssignment]
        public class SpecularMap
        {
            public LSLKey Texture = PrimitiveApi.TEXTURE_BLANK;
            public Vector3 Repeats = Vector3.One;
            public Vector3 Offset;
            public double Rotation;
            public Vector3 Color = Vector3.One;
            public double Alpha = 1;
            public int Glossiness = (int)(0.2f * 255);
            public int Environment;

            public SpecularMap()
            {
            }

            public SpecularMap(SpecularMap m)
            {
                Texture = m.Texture;
                Offset = m.Offset;
                Repeats = m.Repeats;
                Rotation = m.Rotation;
                Color = m.Color;
                Alpha = m.Alpha;
                Glossiness = m.Glossiness;
                Environment = m.Environment;
            }
        }

        [APIExtension(APIExtension.Properties, "alphamode")]
        [APIDisplayName("alphamode")]
        [APIIsVariableType]
        [APIAccessibleMembers(
            "DiffuseMode",
            "MaskCutoff")]
        [Serializable]
        [APICloneOnAssignment]
        public class AlphaMode
        {
            public int DiffuseMode;
            public int MaskCutoff = 1;

            public AlphaMode()
            {
            }

            public AlphaMode(AlphaMode m)
            {
                DiffuseMode = m.DiffuseMode;
                MaskCutoff = m.MaskCutoff;
            }
        }

        [APIExtension(APIExtension.Properties, "linkface")]
        [APIDisplayName("linkface")]
        [APIIsVariableType]
        [APIAccessibleMembers(
            "Texture",
            "TextureOffset",
            "TextureScale",
            "TextureRotation",
            "Color",
            "Alpha",
            "Bump",
            "Shiny",
            "FullBright",
            "TexGen",
            "Flow",
            "NormalMap",
            "SpecularMap",
            "AlphaMode")]
        [ImplementsCustomTypecasts]
        [Serializable]
        public class TextureFace
        {
            [NonSerialized]
            [XmlIgnore]
            public WeakReference<ScriptInstance> WeakInstance;
            [NonSerialized]
            [XmlIgnore]
            public List<WeakReference<ObjectPart>> WeakParts = new List<WeakReference<ObjectPart>>();
            public int[] LinkNumbers;
            public int FaceNumber;

            public TextureFace()
            {
                LinkNumbers = new int[0];
            }

            public TextureFace(ScriptInstance instance, ObjectPart[] parts, int[] linkNumbers, int faceNumber)
            {
                WeakInstance = new WeakReference<ScriptInstance>(instance);
                foreach (ObjectPart part in parts)
                {
                    WeakParts.Add(new WeakReference<ObjectPart>(part));
                }
                LinkNumbers = linkNumbers;
                FaceNumber = faceNumber;
            }

            public void RestoreFromSerialization(ScriptInstance instance)
            {
                WeakInstance = new WeakReference<ScriptInstance>(instance);
                var parts = new List<WeakReference<ObjectPart>>();
                foreach (int linkNumber in LinkNumbers)
                {
                    if (linkNumber == PrimitiveApi.LINK_THIS)
                    {
                        parts.Add(new WeakReference<ObjectPart>(instance.Part));
                    }
                    else
                    {
                        ObjectPart part;
                        if (instance.Part.ObjectGroup.TryGetValue(linkNumber, out part))
                        {
                            parts.Add(new WeakReference<ObjectPart>(part));
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                WeakParts = parts;
            }

            private T With<T>(Func<ObjectPart, T> getter)
            {
                ScriptInstance instance;
                ObjectPart part;
                if (WeakInstance != null && WeakInstance.TryGetTarget(out instance))
                {
                    if (WeakParts.Count == 1 && WeakParts[0].TryGetTarget(out part))
                    {
                        lock (instance)
                        {
                            return getter(part);
                        }
                    }
                    else if (WeakParts.Count > 1)
                    {
                        throw new LocalizedScriptErrorException(this, "MultipleLinksCannotBeRead", "Multiple links cannot be read.");
                    }
                    else
                    {
                        throw new LocalizedScriptErrorException(this, "ValueContentsNotAssignedType0", "Value contents not assigned. (Type {0})", "linkface");
                    }
                }
                else
                {
                    throw new LocalizedScriptErrorException(this, "ValueContentsNotAssignedType0", "Value contents not assigned. (Type {0})", "linkface");
                }
            }

            private T With<T>(Func<Material, T> getter) => With(getter, default(T));

            private T With<T>(Func<Material, T> getter, T defvalue)
            {
                ScriptInstance instance;
                ObjectPart part;
                if (WeakInstance != null && WeakInstance.TryGetTarget(out instance))
                {
                    if (WeakParts.Count == 1 && WeakParts[0].TryGetTarget(out part))
                    {
                        lock (instance)
                        {
                            try
                            {
                                TextureEntryFace face = part.TextureEntry[(uint)FaceNumber];
                                Material mat;
                                try
                                {
                                    mat = part.ObjectGroup.Scene.GetMaterial(face.MaterialID);
                                }
                                catch
                                {
                                    mat = new Material();
                                }
                                return getter(mat);
                            }
                            catch
                            {
                                return defvalue;
                            }
                        }
                    }
                    else if (WeakParts.Count > 1)
                    {
                        throw new LocalizedScriptErrorException(this, "MultipleLinksCannotBeRead", "Multiple links cannot be read.");
                    }
                    else
                    {
                        throw new LocalizedScriptErrorException(this, "ValueContentsNotAssignedType0", "Value contents not assigned. (Type {0})", "linkface");
                    }
                }
                else
                {
                    throw new LocalizedScriptErrorException(this, "ValueContentsNotAssignedType0", "Value contents not assigned. (Type {0})", "linkface");
                }
            }

            private T With<T>(Func<TextureEntryFace, T> getter) => With(getter, default(T));

            private T With<T>(Func<TextureEntryFace, T> getter, T defvalue)
            {
                ScriptInstance instance;
                ObjectPart part;
                if (WeakInstance != null && WeakInstance.TryGetTarget(out instance))
                {
                    if (WeakParts.Count == 1 && WeakParts[0].TryGetTarget(out part))
                    {
                        lock (instance)
                        {
                            try
                            {
                                return getter(part.TextureEntry[(uint)FaceNumber]);
                            }
                            catch
                            {
                                return defvalue;
                            }
                        }
                    }
                    else if (WeakParts.Count > 1)
                    {
                        throw new LocalizedScriptErrorException(this, "MultipleLinksCannotBeRead", "Multiple links cannot be read.");
                    }
                    else
                    {
                        throw new LocalizedScriptErrorException(this, "ValueContentsNotAssignedType0", "Value contents not assigned. (Type {0})", "linkface");
                    }
                }
                else
                {
                    throw new LocalizedScriptErrorException(this, "ValueContentsNotAssignedType0", "Value contents not assigned. (Type {0})", "linkface");
                }
            }

            private void With<T>(Action<ObjectPart, T> setter, T value)
            {
                ScriptInstance instance;
                ObjectPart part;
                if (WeakInstance != null && WeakInstance.TryGetTarget(out instance))
                {
                    foreach (WeakReference<ObjectPart> weakPart in WeakParts)
                    {
                        if (weakPart.TryGetTarget(out part))
                        {
                            lock (instance)
                            {
                                setter(part, value);
                            }
                        }
                    }
                }
                else
                {
                    throw new LocalizedScriptErrorException(this, "ValueContentsNotAssignedType0", "Value contents not assigned. (Type {0})", "linkface");
                }
            }

            private void With<T>(Action<TextureEntryFace, T> setter, T value)
            {
                With((ScriptInstance instance, TextureEntryFace face, T v) => setter(face, v), value);
            }

            private void With<T>(Action<ScriptInstance, TextureEntryFace, T> setter, T value)
            {
                ScriptInstance instance;
                ObjectPart part;
                if (WeakInstance != null && WeakInstance.TryGetTarget(out instance))
                {
                    foreach (WeakReference<ObjectPart> weakPart in WeakParts)
                    {
                        if (weakPart.TryGetTarget(out part))
                        {
                            lock (instance)
                            {
                                if (FaceNumber == ALL_SIDES)
                                {
                                    TextureEntry te = part.TextureEntry;
                                    for (int face = 0; face < TextureEntry.MAX_TEXTURE_FACES && face < part.NumberOfSides; ++face)
                                    {
                                        setter(instance, te[(uint)face], value);
                                    }
                                    part.TextureEntry = te;
                                }
                                else
                                {
                                    TextureEntry te = part.TextureEntry;
                                    TextureEntryFace face = te[(uint)FaceNumber];
                                    setter(instance, face, value);
                                    part.TextureEntry = te;
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new LocalizedScriptErrorException(this, "ValueContentsNotAssignedType0", "Value contents not assigned. (Type {0})", "linkface");
                }
            }

            private void With<T>(Action<Material, T> setter, T value)
            {
                With((instance, face, v) =>
                {
                    SceneInterface scene = instance.Part.ObjectGroup.Scene;
                    Material mat;
                    try
                    {
                        mat = scene.GetMaterial(face.MaterialID);
                    }
                    catch
                    {
                        mat = new Material();
                    }
                    setter(mat, v);
                    mat.MaterialID = UUID.Random;
                    scene.StoreMaterial(mat);
                    face.MaterialID = mat.MaterialID;
                }, value);
            }

            [XmlIgnore]
            public LSLKey Texture
            {
                get { return With((f) => new LSLKey(f.TextureID), new LSLKey()); }
                set
                {
                    UUID textureID = UUID.Zero;
                    ScriptInstance actInstance;
                    if (WeakInstance != null && WeakInstance.TryGetTarget(out actInstance))
                    {
                        lock (actInstance)
                        {
                            textureID = actInstance.GetTextureAssetID(value.ToString());
                        }
                    }
                    With((instance, f, texture) => f.TextureID = texture, textureID);
                }
            }

            [XmlIgnore]
            public Vector3 TextureOffset
            {
                get { return With((f) => new Vector3(f.OffsetU, f.OffsetV, 0)); }
                set
                {
                    With((f, v) =>
                    {
                        f.OffsetU = (float)v.X;
                        f.OffsetV = (float)v.Y;
                    }, value);
                }
            }

            [XmlIgnore]
            public Vector3 TextureScale
            {
                get { return With((f) => new Vector3(f.RepeatU, f.RepeatV, 0)); }
                set
                {
                    With((f, v) =>
                    {
                        f.RepeatU = (float)v.X;
                        f.RepeatV = (float)v.Y;
                    }, value);
                }
            }

            [XmlIgnore]
            public double TextureRotation
            {
                get { return With((TextureEntryFace f) => f.Rotation); }
                set { With((f, v) => f.Rotation = v, (float)value); }
            }

            [XmlIgnore]
            public Vector3 Color
            {
                get
                {
                    if(FaceNumber == ALL_SIDES)
                    {
                        return With((p) =>
                        {
                            Vector3 v = Vector3.Zero;
                            int n = 0;

                            TextureEntry entry = p.TextureEntry;
                            for (int face = 0; face < TextureEntry.MAX_TEXTURE_FACES && face < p.NumberOfSides; ++face)
                            {
                                v += entry[(uint)face].TextureColor.AsVector3;
                                ++n;
                            }
                            v /= n;
                            return v;
                        });
                    }
                    else
                    {
                        return With((f) => f.TextureColor);
                    }
                }
                set
                {
                    With((f, v) =>
                    {
                        ColorAlpha c = f.TextureColor;
                        c.R = v.X;
                        c.G = v.Y;
                        c.B = v.Z;
                        f.TextureColor = c;
                    }, value);
                }
            }

            [XmlIgnore]
            public double Alpha
            {
                get { return With((f) => f.TextureColor.A); }
                set
                {
                    With((f, v) =>
                    {
                        ColorAlpha c = f.TextureColor;
                        c.A = v;
                        f.TextureColor = c;
                    }, value);
                }
            }

            [XmlIgnore]
            public int Bump
            {
                get { return With((f) => (int)f.Bump); }
                set
                {
                    if (value >= 0 && value <= (int)Bumpiness.Weave)
                    {
                        With((f, v) => f.Bump = v, (Bumpiness)value);
                    }
                }
            }

            [XmlIgnore]
            public int Shiny
            {
                get { return With((f) => (int)f.Shiny); }
                set
                {
                    if (value >= 0 && value <= (int)Shininess.High)
                    {
                        With((f, v) => f.Shiny = v, (Shininess)value);
                    }
                }
            }

            [XmlIgnore]
            public int FullBright
            {
                get { return With((f) => f.FullBright.ToLSLBoolean()); }
                set { With((f, v) => f.FullBright = v, value != 0); }
            }

            [XmlIgnore]
            public int TexGen
            {
                get { return With((f) => (int)f.TexMapType); }
                set
                {
                    if (value >= 0 && value <= 1)
                    {
                        With((f, v) => f.TexMapType = v, (MappingType)value);
                    }
                }
            }

            [XmlIgnore]
            public double Glow
            {
                get { return With((f) => f.Glow); }
                set { With((f, v) => f.Glow = v, (float)value); }
            }

            [XmlIgnore]
            public NormalMap NormalMap
            {
                get
                {
                    return With((m) => new NormalMap
                    {
                        Texture = m.NormMap,
                        Offset = new Vector3(m.NormOffsetX, m.NormOffsetY, 0) / Material.MATERIALS_MULTIPLIER,
                        Repeats = new Vector3(m.NormRepeatX, m.NormRepeatY, 0) / Material.MATERIALS_MULTIPLIER,
                        Rotation = m.NormRepeatY / Material.MATERIALS_MULTIPLIER
                    });
                }
                set
                {
                    UUID textureID = UUID.Zero;
                    ScriptInstance actInstance;
                    if (WeakInstance != null && WeakInstance.TryGetTarget(out actInstance))
                    {
                        lock (actInstance)
                        {
                            textureID = actInstance.GetTextureAssetID(value.Texture.ToString());
                        }
                    }

                    With((mat, m) =>
                    {
                        m.Offset *= Material.MATERIALS_MULTIPLIER;
                        m.Repeats *= Material.MATERIALS_MULTIPLIER;
                        mat.NormMap = textureID;
                        mat.NormOffsetX = (int)Math.Round(m.Offset.X);
                        mat.NormOffsetY = (int)Math.Round(m.Offset.Y);
                        mat.NormRepeatX = (int)Math.Round(m.Repeats.X);
                        mat.NormRepeatY = (int)Math.Round(m.Repeats.Y);
                    }, value);
                }
            }

            [XmlIgnore]
            public SpecularMap SpecularMap
            {
                get
                {
                    return With((m) => new SpecularMap
                    {
                        Texture = m.SpecMap,
                        Offset = new Vector3(m.SpecOffsetX, m.SpecOffsetY, 0) / Material.MATERIALS_MULTIPLIER,
                        Repeats = new Vector3(m.SpecRepeatX, m.SpecRepeatY, 0) / Material.MATERIALS_MULTIPLIER,
                        Color = m.SpecColor,
                        Alpha = m.SpecColor.A,
                        Rotation = m.SpecRotation / Material.MATERIALS_MULTIPLIER,
                        Environment = m.EnvIntensity,
                        Glossiness = m.SpecExp
                    });
                }
                set
                {
                    UUID textureID = UUID.Zero;
                    ScriptInstance actInstance;
                    if (WeakInstance != null && WeakInstance.TryGetTarget(out actInstance))
                    {
                        lock (actInstance)
                        {
                            textureID = actInstance.GetTextureAssetID(value.Texture.ToString());
                        }
                    }
                    value.Offset *= Material.MATERIALS_MULTIPLIER;
                    value.Repeats *= Material.MATERIALS_MULTIPLIER;
                    value.Rotation %= Math.PI * 2;
                    value.Rotation *= Material.MATERIALS_MULTIPLIER;
                    value.Environment = value.Environment.Clamp(0, 255);
                    value.Glossiness = value.Glossiness.Clamp(0, 255);

                    With((mat, v) =>
                    {
                        mat.SpecMap = textureID;
                        mat.SpecColor = new ColorAlpha(value.Color, value.Alpha);
                        mat.SpecOffsetX = (int)Math.Round(value.Offset.X);
                        mat.SpecOffsetY = (int)Math.Round(value.Offset.Y);
                        mat.SpecRepeatX = (int)Math.Round(value.Repeats.X);
                        mat.SpecRepeatY = (int)Math.Round(value.Repeats.Y);
                        mat.EnvIntensity = v.Environment;
                        mat.SpecExp = v.Glossiness;
                    }, value);
                }
            }

            [XmlIgnore]
            public AlphaMode AlphaMode
            {
                get
                {
                    return With((m) => new AlphaMode
                    {
                        DiffuseMode = m.DiffuseAlphaMode,
                        MaskCutoff = m.AlphaMaskCutoff
                    });
                }
                set
                {
                    value.DiffuseMode = value.DiffuseMode.Clamp(0, 3);
                    value.MaskCutoff = value.MaskCutoff.Clamp(0, 3);

                    With((mat, v) =>
                    {
                        mat.DiffuseAlphaMode = v.DiffuseMode;
                        mat.AlphaMaskCutoff = v.MaskCutoff;
                    }, value);
                }
            }

            [APIExtension(APIExtension.Properties)]
            public static implicit operator bool(TextureFace tf) =>
                tf.With((TextureEntryFace f) => true);
        }
    }
}
