using Mutagen.Bethesda.WPF.Reflection.Attributes;

namespace FO4FalloutGeneticsPatch
{
    public enum PartGenderType
    {
        Female,
        Male
    }

    public class Settings
    {
        [MaintainOrder]
        [SettingName("Ignore CharGen presets")]
        public bool IgnoreCharGen { get; set; } = true;

        [MaintainOrder]
        [SettingName("Use CharGen presets files")]
        public bool UseMorphs { get; set; } = true;

        [MaintainOrder]
        [SettingName("Random seed")]
        public int Seed { get; set; } = 42;

        [MaintainOrder]
        [SettingName("Default Female parts")]
        public PartGenderType FemaleParts { get; set; } = PartGenderType.Female;

        [MaintainOrder]
        [SettingName("Default Male parts")]
        public PartGenderType MaleParts { get; set; } = PartGenderType.Male;
    }
}