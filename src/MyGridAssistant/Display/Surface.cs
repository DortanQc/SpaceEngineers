using Sandbox.ModAPI.Ingame;

namespace MyGridAssistant
{
    public class Surface
    {
        public IMyTextSurface TextSurface { get; set; }

        public CustomDataManager BlockCustomData { get; set; }

        public long BlockId { get; set; }
    }
}
