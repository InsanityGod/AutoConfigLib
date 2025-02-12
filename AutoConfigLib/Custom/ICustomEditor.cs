using ConfigLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Custom
{
    public interface ICustomEditor
    {
        public ControlButtons SupportedButtons { get; }

        public void Save()
        {
            //Empty stub
        }

        public void Restore()
        {
            //Empty stub
        }

        public void Reload()
        {
            //Empty stub
        }

        public void Defaults()
        {
            //Empty stub
        }
    }
}
