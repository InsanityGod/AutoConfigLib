﻿using System;
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
        /// If enabled you will see messages in the in config if auto parsing failed to find a way to display an property/field in the config
        /// </summary>
        public bool ShowPresenceOfUnsupportedTypes { get; set; } = false;



        /// <summary>
        /// Do not touch this
        /// </summary>
        public bool DoNotTouchThis { get; set; } = false;
        public uint MaxStringLength { get; set; } = 128;
    }
}
