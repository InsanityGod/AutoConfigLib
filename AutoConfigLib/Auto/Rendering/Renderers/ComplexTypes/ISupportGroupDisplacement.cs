using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes
{
    public interface ISupportGroupDisplacement
    {
        public bool UseGroupDisplacement { get; set; }

        public float GroupDisplacementX { get; set; }
        public float GroupExtraSpaceX { get; set; }
    }
}
