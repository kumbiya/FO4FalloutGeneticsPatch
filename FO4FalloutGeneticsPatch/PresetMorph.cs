using System.Collections.Generic;

namespace FO4FalloutGeneticsPatch
{
    public class PresetMorph
    {
        public Dictionary<string, double> Presets { get; set; }
        public Dictionary<string, List<double>> Regions { get; set; }
        public List<double> Values { get; set; }
    }
}