using Sandbox.ModAPI.Ingame;

namespace MyGridAssistant
{
    public class Surface
    {
        public IMyTextSurface TextSurface { get; set; }

        public IConfiguration Configuration { get; set; }

        public long BlockId { get; set; }
    }
}
