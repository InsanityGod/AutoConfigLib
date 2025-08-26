using InsanityLib.Attributes.Auto.Config;
using System.ComponentModel;

namespace AutoConfigLib.Config;

public class ModConfig
{
    public static ModConfig Instance { get; private set; }

    [Description("If set to true LoadModConfig will return the previous returned ref\n(this can be usefull in case your playing singlePlayer and the mod calls LoadModConfig on both client and server mod system)")]
    [DefaultValue(true)]
    public bool AutoMergeClientServerConfig { get; set; } = true;

    [DefaultValue(true)]
    public bool RegisterWorldConfig { get; set; } = true;

    [DisplayName("Config Window Improvements")]
    [Description("Some minor improvements to the config window (from configlib itself)\nSuch as increased size constraint\n*requires mod reload to apply")]
    [Category("Styling")]
    [DefaultValue(true)]
    public bool ConfigLibWindowImprovements { get; set;}  = true;
}