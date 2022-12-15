using System.Collections.Generic;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;

namespace FO4FalloutGeneticsPatch
{
    internal class GenderRecord
    {
        public List<IHeadPartGetter> Hair { get; } = new();
        public List<IHeadPartGetter> Eyes { get; } = new();
        public List<IHeadPartGetter> Scar { get; } = new();
        public List<IHeadPartGetter> Brows { get; } = new();
        public List<IHeadPartGetter> FacialHair { get; } = new();
        public List<FormKey> DefaultPreset { get; } = new();
        public List<Preset> Presets { get; } = new();
    }
}