using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Config
{
    public class ModConfig
    {
        /// <summary>
        /// If set to true LoadModConfig will return the previous returned ref
        /// (this can be usefull in case your playing singlePlayer and the mod calls LoadModConfig on both client and server mod system)
        /// </summary>
        public bool ClientServerConfigAutoMerge { get; set; } = true;

        /// <summary>
        /// Loads the world config for editing
        /// (will only work in singleplayer)
        /// </summary>
        public bool LoadWorldConfig { get; set; } = true;

        /// <summary>
        /// The maximum length of string fields
        /// </summary>
        public int MaxStringLength { get; set; } = 128;

        /// <summary>
        /// If set to true will automatically attempt to create a new instance of fields/properties that are null
        /// </summary>
        public bool AutoInitializeNullFields { get; set; } = false;

        /// <summary>
        /// If enabled you will see messages in the in config if auto parsing failed to find a way to display an property/field in the config
        /// </summary>
        public bool ShowPresenceOfUnsupportedTypes { get; set; } = false;

        /// <summary>
        /// If enabled will attempt to show any ICollection using a default implementation
        /// (Beware that editing through this implementation will affect the order of the collection)
        /// </summary>
        public bool UseDefaultImplementationForCollections { get; set; } = false;

        /// <summary>
        /// Do not touch this
        /// </summary>
        public bool DoNotTouchThis { get; set; } = false;
    }
}
