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
using System;
using System.Collections.Generic;

namespace SilverSim.Scripting.Lsl.Api.Base
{
    public partial class BaseApi
    {
        [APILevel(APIFlags.LSL, "timer")]
        [StateEventDelegate]
        public delegate void State_timer();

        [APILevel(APIFlags.LSL, "llSetTimerEvent")]
        public void SetTimerEvent(ScriptInstance instance, double sec)
        {
            Script script = (Script)instance;
            lock (script)
            {
                script.SetTimerEvent(sec);
            }
        }

        [ExecutedOnDeserialization("timer")]
        public void Deserialize(ScriptInstance instance, List<object> param)
        {
            if(param.Count < 2)
            {
                return;
            }
            Script script = (Script)instance;
            lock(script)
            {
                double interval = (double)param[0];
                double elapsed = (double)param[1];
                elapsed %= interval;
                script.SetTimerEvent(interval, elapsed);
            }
        }

        [ExecutedOnSerialization("timer")]
        public void Serialize(ScriptInstance instance, List<object> res)
        {
            Script script = (Script)instance;
            lock(script)
            {
                if (script.Timer.Enabled)
                {
                    res.Add("timer");
                    res.Add(2);
                    double interval = script.CurrentTimerInterval;
                    res.Add(interval);
                    int timeElapsed = Environment.TickCount - script.LastTimerEventTick;
                    double timeToElapse = interval - timeElapsed / 1000f;
                    res.Add(timeToElapse);
                }
            }
        }
    }
}
