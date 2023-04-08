using System.Collections.Generic;

namespace MyGridAssistant
{
    public class SurfaceByType
    {
        public IEnumerable<Item.ItemTypes> ItemTypes { get; set; }

        public Surface Block { get; set; }
    }
}
