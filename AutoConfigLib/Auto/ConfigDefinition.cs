using ConfigLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AutoConfigLib.Auto;

public class ConfigDefinition
{
    public Type Type { get; init; }

    public string ConfigPath { get; init; }

    public object ClientValue { get; internal set; }
    
    public object ServerValue { get; internal set; }

    public object PrimaryValue { get; internal set; }

    public Mod Mod { get; internal set; }

}