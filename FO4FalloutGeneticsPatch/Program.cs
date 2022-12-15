﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.FormKeys.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Newtonsoft.Json;
using Noggog;

namespace FO4FalloutGeneticsPatch
{
    public class Program
    {
        private static Lazy<Settings> _settings;
        private static Settings Settings => _settings.Value;

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<IFallout4Mod, IFallout4ModGetter>(RunPatch)
                .SetAutogeneratedSettings("Settings", "settings.json", out _settings)
                .SetTypicalOpen(GameRelease.Fallout4, "FalloutGenetics_Patch.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<IFallout4Mod, IFallout4ModGetter> state)
        {
            var female = new GenderRecord();
            var male = new GenderRecord();
            var neutral = new GenderRecord();

            foreach (var hdptContext in state.LoadOrder.PriorityOrder.HeadPart().WinningContextOverrides())
            {
                var record = hdptContext.Record;
                if (record is null) continue;
                if (record.IsDeleted) continue;
                if (record.MajorFlags.HasFlag(HeadPart.MajorFlag.NonPlayable)) continue;

                if (record.ValidRaces.IsNull ||
                    (!record.ValidRaces.FormKey.Equals(Fallout4.FormList.HeadPartsGhouls.FormKey) &&
                     !record.ValidRaces.FormKey.Equals(Fallout4.FormList.HeadPartsHuman.FormKey))) continue;

                if ((record.Flags.HasFlag(HeadPart.Flag.Female) && record.Flags.HasFlag(HeadPart.Flag.Male)) ||
                    (!record.Flags.HasFlag(HeadPart.Flag.Female) && !record.Flags.HasFlag(HeadPart.Flag.Male)))
                    switch (record.Type)
                    {
                        case HeadPart.TypeEnum.Eyes:
                            neutral.Eyes.Add(record);
                            break;
                        case HeadPart.TypeEnum.Hair:
                            neutral.Hair.Add(record);
                            break;
                        case HeadPart.TypeEnum.FacialHair:
                            neutral.FacialHair.Add(record);
                            break;
                        case HeadPart.TypeEnum.Scars:
                            neutral.Scar.Add(record);
                            break;
                        case HeadPart.TypeEnum.Eyebrows:
                            neutral.Brows.Add(record);
                            break;
                    }
                else if (record.Flags.HasFlag(HeadPart.Flag.Female))
                    switch (record.Type)
                    {
                        case HeadPart.TypeEnum.Eyes:
                            female.Eyes.Add(record);
                            break;
                        case HeadPart.TypeEnum.Hair:
                            female.Hair.Add(record);
                            break;
                        case HeadPart.TypeEnum.Scars:
                            female.Scar.Add(record);
                            break;
                        case HeadPart.TypeEnum.Eyebrows:
                            female.Brows.Add(record);
                            break;
                    }
                else if (record.Flags.HasFlag(HeadPart.Flag.Male))
                    switch (record.Type)
                    {
                        case HeadPart.TypeEnum.Eyes:
                            male.Eyes.Add(record);
                            break;
                        case HeadPart.TypeEnum.Hair:
                            male.Hair.Add(record);
                            break;
                        case HeadPart.TypeEnum.FacialHair:
                            male.FacialHair.Add(record);
                            break;
                        case HeadPart.TypeEnum.Scars:
                            male.Scar.Add(record);
                            break;
                        case HeadPart.TypeEnum.Eyebrows:
                            male.Brows.Add(record);
                            break;
                    }
            }

            Console.WriteLine(
                $"Neutral headparts:\n\tEyes - {neutral.Eyes.Count}\n\tHair - {neutral.Hair.Count}\n\tEyebrows - {neutral.Brows.Count}\n\tScars - {neutral.Scar.Count}\n\tFacial Hair - {neutral.FacialHair.Count}");
            Console.WriteLine(
                $"Female headparts:\n\tEyes - {female.Eyes.Count}\n\tHair - {female.Hair.Count}\n\tEyebrows - {female.Brows.Count}\n\tScars - {female.Scar.Count}");
            Console.WriteLine(
                $"Male headparts:\n\tEyes - {male.Eyes.Count}\n\tHair - {male.Hair.Count}\n\tEyebrows - {male.Brows.Count}\n\tScars - {male.Scar.Count}\n\tFacial Hair - {male.FacialHair.Count}");

            male.Eyes.AddRange(neutral.Eyes);
            male.Hair.AddRange(neutral.Hair);
            male.FacialHair.AddRange(neutral.FacialHair);
            male.Scar.AddRange(neutral.Scar);
            male.Brows.AddRange(neutral.Brows);
            female.Eyes.AddRange(neutral.Eyes);
            female.Hair.AddRange(neutral.Hair);
            female.Scar.AddRange(neutral.Scar);
            female.Brows.AddRange(neutral.Brows);

            male.DefaultPreset.Add(Fallout4.HeadPart.MaleHeadHuman.FormKey);
            male.DefaultPreset.Add(Fallout4.HeadPart.MaleHeadHumanRearTEMP.FormKey);
            male.DefaultPreset.Add(Fallout4.HeadPart.MaleEyesHumanLashes.FormKey);
            male.DefaultPreset.Add(Fallout4.HeadPart.MaleMouthHumanoidDefault.FormKey);
            male.DefaultPreset.Add(Fallout4.HeadPart.MaleEyesHumanAO.FormKey);
            male.DefaultPreset.Add(Fallout4.HeadPart.MaleEyesHumanWet.FormKey);

            female.DefaultPreset.Add(Fallout4.HeadPart.FemaleHeadHuman.FormKey);
            female.DefaultPreset.Add(Fallout4.HeadPart.FemaleHeadHumanRearTEMP.FormKey);
            female.DefaultPreset.Add(Fallout4.HeadPart.FemaleEyesHumanLashes.FormKey);
            female.DefaultPreset.Add(Fallout4.HeadPart.FemaleMouthHumanoidDefault.FormKey);
            female.DefaultPreset.Add(Fallout4.HeadPart.FemaleEyesHumanAO.FormKey);
            female.DefaultPreset.Add(Fallout4.HeadPart.FemaleEyesHumanWet.FormKey);

            var random = new Random(Settings.Seed);

            var presetPath = Path.Combine(state.DataFolderPath, "F4SE\\Plugins\\F4EE\\Presets\\falloutGenetics");
            foreach (var file in Directory.EnumerateFiles(presetPath, "*.json", SearchOption.TopDirectoryOnly))
            {
                var preset = JsonConvert.DeserializeObject<Preset>(File.ReadAllText(file));
                if (preset.Morphs.Values is null) preset.Morphs.Values = new List<double> {0, 0, 0, 0, 0};

                if (preset.Gender == 1)
                    female.Presets.Add(preset);
                else
                    male.Presets.Add(preset);
            }

            Console.WriteLine($"Found {female.Presets.Count} female presets and {male.Presets.Count} male presets.");

            foreach (var npcContext in state.LoadOrder.PriorityOrder.Npc().WinningContextOverrides())
            {
                var record = npcContext.Record;
                if (record is null) continue;
                if (record.IsDeleted) continue;
                if (Settings.IgnoreCharGen && record.Flags.HasFlag(Npc.Flag.IsCharGenFacePreset)) continue;
                if (record.Race.IsNull || !record.Race.FormKey.Equals(Fallout4.Race.HumanRace.FormKey)) continue;

                var newRecord = npcContext.GetOrAddAsOverride(state.PatchMod);

                newRecord.HeadParts.Clear();
                var parts = new List<FormKey>();
                var presets = new List<Preset>();
                if ((record.Flags.HasFlag(Npc.Flag.Female) && Settings.FemaleParts == PartGenderType.Female) ||
                    (!record.Flags.HasFlag(Npc.Flag.Female) && Settings.MaleParts == PartGenderType.Female))
                {
                    parts.AddRange(female.DefaultPreset);
                    if (female.Eyes.Count > 0) parts.Add(female.Eyes[random.Next(female.Eyes.Count)].FormKey);
                    if (female.Hair.Count > 0) parts.Add(female.Hair[random.Next(female.Hair.Count)].FormKey);
                    if (female.Brows.Count > 0) parts.Add(female.Brows[random.Next(female.Brows.Count)].FormKey);
                    if (female.Scar.Count > 0) parts.Add(female.Scar[random.Next(female.Scar.Count)].FormKey);
                    presets = female.Presets;
                }
                else
                {
                    parts.AddRange(male.DefaultPreset);
                    if (male.Eyes.Count > 0) parts.Add(male.Eyes[random.Next(male.Eyes.Count)].FormKey);
                    if (male.Hair.Count > 0) parts.Add(male.Hair[random.Next(male.Hair.Count)].FormKey);
                    if (male.Brows.Count > 0) parts.Add(male.Brows[random.Next(male.Brows.Count)].FormKey);
                    if (male.Scar.Count > 0) parts.Add(male.Scar[random.Next(male.Scar.Count)].FormKey);
                    if (male.FacialHair.Count > 0)
                        parts.Add(male.FacialHair[random.Next(male.FacialHair.Count)].FormKey);
                    presets = male.Presets;
                }

                newRecord.HeadParts.AddRange(parts);

                if (Settings.UseMorphs && presets.Count > 0)
                {
                    var p1 = presets[random.Next(presets.Count)];
                    var p2 = presets[random.Next(presets.Count)];

                    var child = Genetics(p1.Morphs, p2.Morphs, random.NextDouble());
                    Morph(newRecord, child);
                }
            }
        }

        private static PresetMorph Genetics(PresetMorph p1, PresetMorph p2, double t)
        {
            p1.Values ??= new List<double> {0, 0, 0, 0, 0};
            p2.Values ??= new List<double> {0, 0, 0, 0, 0};
            var child = new PresetMorph
            {
                Presets = ConvolvePresets(p1.Presets, p2.Presets, t),
                Regions = ConvolveRegions(p1.Regions, p2.Regions, t),
                Values = Combine(p1.Values, p2.Values, t)
            };
            return child;
        }

        private static List<double> Combine(List<double> a, List<double> b, double t)
        {
            var c = new List<double>();
            for (var i = 0; i < Math.Min(a.Count, b.Count); i++) c.Add(t * a[i] + (1 - t) * b[i]);
            return c;
        }

        private static Dictionary<string, List<double>> ConvolveRegions(Dictionary<string, List<double>> x,
            Dictionary<string, List<double>> y, double t)
        {
            var child = new Dictionary<string, List<double>>();
            foreach (var i in x.Keys)
            {
                var yi = new List<double> {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0};
                if (y.ContainsKey(i)) yi = y[i];
                child.Add(i, Combine(x[i], yi, t));
            }

            foreach (var i in y.Keys)
            {
                var xi = new List<double> {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0};
                if (x.ContainsKey(i)) continue;
                child.Add(i, Combine(xi, y[i], t));
            }

            return child;
        }

        private static Dictionary<string, double> ConvolvePresets(Dictionary<string, double> x,
            Dictionary<string, double> y, double t)
        {
            var child = new Dictionary<string, double>();
            foreach (var i in x.Keys)
            {
                var yi = 0.0;
                if (y.ContainsKey(i)) yi = y[i];
                child.Add(i, t * x[i] + (1 - t) * yi);
            }

            foreach (var i in y.Keys)
            {
                if (x.ContainsKey(i)) continue;
                child.Add(i, (1 - t) * y[i]);
            }

            return child;
        }

        private static void Morph(INpc r, PresetMorph p)
        {
            r.FaceMorphs.Clear();
            var reg = p.Regions;
            foreach (var pt in reg.Keys)
            {
                var fm = new NpcFaceMorph
                {
                    Index = uint.Parse(pt, System.Globalization.NumberStyles.HexNumber),
                    Position = new P3Float((float) reg[pt][0], (float) reg[pt][1], (float) reg[pt][2]),
                    Rotation = new P3Float((float) reg[pt][3], (float) reg[pt][4], (float) reg[pt][5]),
                    Scale = (float) reg[pt][6]
                };
                r.FaceMorphs.Add(fm);
            }

            var c = p.Presets;
            r.Morphs.Clear();
            foreach (var pt in c.Keys)
            {
                var m = new NpcMorph
                {
                    Key = uint.Parse(pt, System.Globalization.NumberStyles.HexNumber),
                    Value = (float) c[pt]
                };
                r.Morphs.Add(m);
            }

            var bm = p.Values;
            r.BodyMorphRegionValues = new NpcBodyMorphRegionValues
            {
                Head = (float) bm[0],
                UpperTorso = (float) bm[1],
                Arms = (float) bm[2],
                LowerTorso = (float) bm[3],
                Legs = (float) bm[4]
            };
            r.FacialMorphIntensity = null;
        }
    }
}