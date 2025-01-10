using AutoConfigLib.Auto;
using AutoConfigLib.Auto.Rendering;
using AutoConfigLib.AutoConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Config
{
    public class ModConfig
    {

        [Description("If set to true LoadModConfig will return the previous returned ref\n(this can be usefull in case your playing singlePlayer and the mod calls LoadModConfig on both client and server mod system)")]
        public bool AutoMergeClientServerConfig { get; set; } = true;

        public bool LoadWorldConfig { get; set; } = true;

        [DisplayName("Maximum String Length")]
        public int DefaultMaxStringLength { get; set; } = 128;

        [Description("If enabled, BrowseAbleAttribute will no longer affect display of members\n(Methods are by default considered to have BrowseAbleAttribute(false))\n*Clear render cache for immediate update")]
        public bool IgnoreBrowseAbleAttribute { get; set; } = false;

        [Description("If set to true will automatically attempt to create a new instance of fields/properties that are null, otherwise you will see a button to initialize them")]
        public bool AutoInitializeNullFields { get; set; } = false;

        [Description("If enabled you will see messages in the in config if it could not dislay something because the type isn't supported")]
        public bool ShowPresenceOfUnsupportedTypes { get; set; } = false;

        [Description("If enabled will attempt to show any ICollection using a default implementation\n(Beware that editing through this implementation will affect the order of the collection)\n*Clear render cache for immediate update")]
        public bool UseDefaultImplementationForCollections { get; set; } = false;

        [Description("If set to true, accessmodifiers (public, protected, private, itnernal) will be ignored when building config\n*Clear render cache for immediate update")]
        public bool IgnoreAccessModifier { get; set; } = false;

        //TODO

        [Browsable(true)]
        [Description("Will invalidate the renderer cache, so it can re-initialize renders")]
        public static void ClearRenderCache() => Renderer.ClearCache();

        [Browsable(true)]
        [DisplayName("Do Not Touch (1)")]
        [Description("Can you resist the big red button?")]
        public static void DoNotTouch1() => DoNotTouchThis.Touch_1();
    }
}
