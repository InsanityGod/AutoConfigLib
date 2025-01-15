using AutoConfigLib.Auto.Rendering;
using CompactExifLib;
using ConfigLib;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace AutoConfigLib.Auto
{
    public class ConfigDefinition
    {
        [ReadOnly(true)]
        public Type Type { get; set; }

        [ReadOnly(true)]
        public string ConfigPath { get; set; }

        [Browsable(false)]
        public object ClientValue { get; set; }
        
        [Browsable(false)]
        public object ServerValue { get; set; }

        [Browsable(false)]
        public object PrimaryValue { get; set; }

        [Browsable(false)]
        internal bool IsLocalized_Internal { get; set; }

        [Description("Whether config should be writen to World Conifg, to alllow for per world configuration\n*When enabling, current config will be written to world config\n*When disabling, config will be removed from world config and global config will be loaded")]
        [Category("Experimental")]
        public bool IsLocalized
        {
            get => IsLocalized_Internal;
            set
            {
                if(AutoConfigLibModSystem.CoreServerAPI == null || value == IsLocalized_Internal) return;
                IsLocalized_Internal = value;

                var autoConfigLibTree = AutoConfigLibModSystem.CoreServerAPI.World.Config.GetOrAddTreeAttribute("autoconfiglib");
                if (value)
                {
                    string json = JsonConvert.SerializeObject(PrimaryValue, Formatting.None);
                    autoConfigLibTree.SetString(ConfigPath, json);
                }
                else
                {
                    autoConfigLibTree.RemoveAttribute(ConfigPath);
                    ReloadFromFile(AutoConfigLibModSystem.CoreServerAPI);
                }
            }
        }

        public Mod Mod { get; set; }

        public void Save(ICoreAPI api)
        {
            if (IsLocalized)
            {
                AutoConfigLibModSystem.CoreServerAPI?.World.Config.GetOrAddTreeAttribute("autoconfiglib").SetString(ConfigPath, JsonConvert.SerializeObject(PrimaryValue, Formatting.None));
            }
            else api.StoreModConfig(new JsonObject(JToken.FromObject(PrimaryValue)), ConfigPath);
        }

        private static ControlButtons supportedButtons = new()
        {
            Defaults = true,
            Restore = true,
            Save = true,
            Reload = false,
        };

        public ControlButtons Edit(ICoreAPI api, string id, ControlButtons buttons)
        {
            if (buttons.Defaults) ResetToDefaul();
            if (buttons.Restore) ReloadFromFile(api);
            if (buttons.Save) Save(api);

            Renderer.GetOrCreateRenderForType(typeof(ConfigDefinition)).RenderObject(this, id);
            supportedButtons.Save = !IsLocalized || AutoConfigLibModSystem.CoreServerAPI != null;
            return supportedButtons;
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

                object newInstance;
                if (IsLocalized)
                {
                    newInstance = JsonConvert.DeserializeObject((string)api.World.Config.GetOrAddTreeAttribute("autoconfiglib")[ConfigPath].GetValue(), Type);
                }
                else
                {
                    newInstance = typeof(ICoreAPICommon)
                            .GetMethods()
                            .First(method => method.Name == nameof(ICoreAPICommon.LoadModConfig) && method.IsGenericMethod)
                            .MakeGenericMethod(Type)
                            .Invoke(api, new object[] { ConfigPath });
                }
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