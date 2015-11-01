﻿using System.Diagnostics.CodeAnalysis;
// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3
using System.Reflection;

[assembly: AssemblyTitle("LSL Scripting Implementation")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

/* gendarme suppressors */
[assembly: SuppressMessage("Gendarme.Rules.Design.Generic", "AvoidDeclaringCustomDelegatesRule")]
[assembly: SuppressMessage("Gendarme.Rules.Naming", "DoNotUseReservedInEnumValueNamesRule")]
[assembly: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectSuffixRule")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.HTTP")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.XMLRPC")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.WindLight")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Vehicles")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Sound")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Region")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Primitive")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Physics")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Permissions")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Parcel")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Notecards")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Money")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.LogListen")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.LightShare")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Inventory")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.IM")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Experience")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Detected")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Controls")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Chat")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Base")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.AnimationOverride")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.API.Animation")]
[module: SuppressMessage("Gendarme.Rules.Naming", "UseCorrectCasingRule", Scope = "namespace", Target = "SilverSim.Scripting.LSL.Expression")]