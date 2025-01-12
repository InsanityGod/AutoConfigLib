using AutoConfigLib.Auto.Rendering;
using ConfigLib;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AutoConfigLib.Auto
{
    public class ConfigDefinition
    {
        public Type Type { get; set; }

        public string ConfigPath { get; set; }

        public object ClientValue { get; set; }

        public object ServerValue { get; set; }

        public object PrimaryValue { get; set; }

        public Mod Mod { get; set; }

        public void Save(ICoreAPI api) => api.StoreModConfig(new JsonObject(JToken.FromObject(PrimaryValue)), ConfigPath);

        public void Edit(ICoreAPI api, string id, ControlButtons buttons)
        {
            if (buttons.Defaults) ResetToDefaul();
            if (buttons.Restore) ReloadFromFile(api);
            if (buttons.Save) Save(api);

            Renderer.GetOrCreateRenderForType(Type).RenderObject(PrimaryValue, id);
        }

        public void ResetToDefaul()
        {
            try
            {
                var newInstance = Activator.CreateInstance(Type);
                OverwriteWith(newInstance);
            }
            catch
            {
                Console.WriteLine($"AutoConfigLib: Cannot reset '{Type}' as it cannot be initialized");
            }
        }

        //TODO: find out why reloading config is appending default values to dictionaries
        public void ReloadFromFile(ICoreAPI api)
        {
            try
            {
                var newInstance = typeof(ICoreAPICommon)
                            .GetMethods()
                            .First(method => method.Name == nameof(ICoreAPICommon.LoadModConfig) && method.IsGenericMethod)
                            .MakeGenericMethod(Type)
                            .Invoke(api, new object[] { ConfigPath });
                OverwriteWith(newInstance);
            }
            catch (Exception ex)
            {
                api.Logger.Error(ex);
            }
        }

        private void OverwriteWith(object newInstance)
        {
            if (newInstance.GetType() != Type)
            {
                Console.WriteLine($"AutoConfig: Illegal overwrite attempt, expected '{Type}' but got '{newInstance.GetType()}'");
                return;
            }

            try
            {
                foreach (var member in Type.GetMembers())
                {
                    if (member is PropertyInfo property && property.CanRead && property.CanWrite)
                    {
                        property.SetValue(PrimaryValue, property.GetValue(newInstance));
                    }
                    else if (member is FieldInfo field && !field.IsInitOnly)
                    {
                        field.SetValue(PrimaryValue, field.GetValue(newInstance));
                    }
                }
            }
            catch
            {
                Console.WriteLine($"AutoConfig: Failed to overwrite '{Type}'");
            }
        }
    }
}