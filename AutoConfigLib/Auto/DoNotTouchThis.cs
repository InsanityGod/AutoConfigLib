using AutoConfigLib.Auto.Rendering;
using ConfigLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto
{
    public static class DoNotTouchThis
    {
        internal static bool Touched_1 = false;
        internal static void Touch_1()
        {
            if(Touched_1) return;
            Touched_1 = true;

            var configLib = AutoConfigLibModSystem.CoreClientAPI.ModLoader.GetModSystem<ConfigLibModSystem>();

            if(AutoConfigLibModSystem.CoreServerAPI != null)
            {
                foreach(var mod in AutoConfigLibModSystem.CoreServerAPI.ModLoader.Mods)
                {
                    foreach(var system in mod.Systems)
                    {
                        configLib.RegisterCustomConfig($"{mod.Info.ModID} - server - {system.GetType().Name} (auto)" , (string id, ControlButtons buttons) => Renderer.GetOrCreateRenderForType(system.GetType()).RenderObject(system, id));
                    }
                }
            }

            foreach(var mod in AutoConfigLibModSystem.CoreClientAPI.ModLoader.Mods)
            {
                foreach(var system in mod.Systems)
                {
                    configLib.RegisterCustomConfig($"{mod.Info.ModID} - client - {system.GetType().Name} (auto)" , (string id, ControlButtons buttons) => Renderer.GetOrCreateRenderForType(system.GetType()).RenderObject(system, id));
                }
            }
        }
    }
}
