using System.Collections.Generic;

namespace IngameScript
{
    public class SurfaceByType
    {
        public IEnumerable<Item.ItemTypes> ItemTypes { get; set; }

        public Surface Block { get; set; }
    }
}