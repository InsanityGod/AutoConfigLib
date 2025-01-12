using AutoConfigLib.Auto;
using AutoConfigLib.Auto.Rendering;
using AutoConfigLib.Auto.Rendering.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AutoConfigLib.Config
{
    public class ModConfig
    {
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

        [Description("Wether categories should be automatically created based on commen terms in the name")]
        [Category("Styling")]
        [DefaultValue(true)]
        public bool AutoGroupByName { get; set; } = true;

        [Description("This changes the position of complex types (objects that you see with a collapseable header)\n*Clear render cache for immediate update")]
        [Category("Styling")]
        [DefaultValue(EAlignment.Bottom)]
        public EAlignment ComplexTypeAllignment { get; set; } = EAlignment.Bottom;

        [Description("This is multiplied with line height to calculate how much padding should be bellow an opened complex type")]
        [DefaultValue(1f)]
        [Range(0, float.PositiveInfinity)]
        [Category("Styling")]
        public float ComplexTypePaddingMultiplier { get; set; } = 1f;

        [DisplayName("Maximum String Length")]
        [DefaultValue(128)]
        public int DefaultMaxStringLength { get; set; } = 128;

        [Description("If set to true will automatically attempt to create a new instance of fields/properties that are null, otherwise you will see a button to initialize them")]
        [DefaultValue(false)]
        public bool AutoInitializeNullFields { get; set; } = false;

        [Description("If enabled, BrowseAbleAttribute will no longer affect display of members\n(Methods are by default considered to have BrowseAbleAttribute(false))\n*Clear render cache for immediate update")]
        [Category("Dangerous But Fun")]
        [DefaultValue(false)]
        public bool IgnoreBrowseAbleAttribute { get; set; } = false;

        [Description("If enabled you will see messages in the in config if it could not dislay something because the type isn't supported")]
        [Category("Debug")]
        [DefaultValue(false)]
        public bool ShowPresenceOfUnsupportedTypes { get; set; } = false;

        [Description("If enabled you will see members that have been been given the obsolete attribute\n*Clear render cache for immediate update")]
        [Category("Debug")]
        [DefaultValue(false)]
        public bool ShowObsoleteMembers { get; set; } = false;

        [Description("If enabled you will see members that haven't been marked with ProtoMeber inside a class marked with ProtoContract")]
        [Category("Dangerous But Fun")]
        [DefaultValue(false)]
        public bool ShowNonProtoMembersInProtoClass { get; set; } = false;

        [Description("If enabled will attempt to show any ICollection using a default implementation\n(Beware that editing through this implementation will affect the order of the collection)\n*Clear render cache for immediate update")]
        [Category("Dangerous But Fun")]
        [DefaultValue(false)]
        public bool UseDefaultImplementationForCollections { get; set; } = false;

        [Description("If set to true, accessmodifiers (public, protected, private, itnernal) will be ignored when building config\n*Clear render cache for immediate update")]
        [Category("Dangerous But Fun")]
        [DefaultValue(false)]
        public bool IgnoreAccessModifier { get; set; } = false;

        [Browsable(true)]
        [Description("Will invalidate the renderer cache, so it can re-initialize renders")]
        public static void ClearRenderCache() => Renderer.ClearCache();

        [Browsable(true)]
        [DisplayName("Do Not Touch (1)")]
        [Description("Can you resist the big red button?")]
        [Category("Dangerous But Fun")]
        private static void DoNotTouch1() => DoNotTouchThis.Touch_1();
    }
}