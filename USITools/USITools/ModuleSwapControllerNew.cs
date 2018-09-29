using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace USITools
{
    public class ModuleSwapControllerNew : PartModule
    {
        [KSPField]
        public string ResourceCosts = "";

        [KSPField]
        public string typeName = "Loadout";

        public List<ResourceRatio> ResCosts;
        public List<ModuleSwapOption> Loadouts;
        private List<ModuleResourceHarvester_USI> _harvesters;
        private List<ModuleResourceConverter_USI> _converters;

        private double lastCheck;
        private double checkInterval = 5d;

        public override void OnStart(StartState state)
        {
            Loadouts = part.FindModulesImplementing<ModuleSwapOption>();
            _converters = part.FindModulesImplementing<ModuleResourceConverter_USI>();
            _harvesters = part.FindModulesImplementing<ModuleResourceHarvester_USI>();
            SetupResourceCosts();
        }

        private void SetupResourceCosts()
        {
            ResCosts = new List<ResourceRatio>();
            if (String.IsNullOrEmpty(ResourceCosts))
                return;

            var resources = ResourceCosts.Split(',');
            for (int i = 0; i < resources.Length; i += 2)
            {
                ResCosts.Add(new ResourceRatio
                {
                    ResourceName = resources[i],
                    Ratio = double.Parse(resources[i + 1])
                });
            }
        }

        public override string GetInfo()
        {
            if (String.IsNullOrEmpty(ResourceCosts))
                return "";

            var output = new StringBuilder("Resource Cost:\n\n");
            var resources = ResourceCosts.Split(',');
            for (int i = 0; i < resources.Length; i += 2)
            {
                output.Append(string.Format("{0} {1}\n", double.Parse(resources[i + 1]), resources[i]));
            }
            return output.ToString();
        }

        public void ApplyLoadout(int loadIdx, int moduleIdx, bool isConverter)
        {
            var loadout = Loadouts[loadIdx];
            if (isConverter)
            {
                ApplyConverterChanges(_converters[moduleIdx], loadout);
            }
            else
            {
                ApplyHarvesterChanges(_harvesters[moduleIdx], loadout);
            }
        }

        private void ApplyConverterChanges(ModuleResourceConverter_USI converter, ModuleSwapOption loadout)
        {
            converter.ConverterName = loadout.ConverterName;
            converter.StartActionName = loadout.StartActionName;
            converter.StopActionName = loadout.StopActionName;
            converter.UseSpecialistBonus = loadout.UseSpecialistBonus;
            if (converter.UseSpecialistBonus)
                converter.ExperienceEffect = loadout.ExperienceEffect;
            converter.UseBonus = loadout.UseBonus;
            converter.eMultiplier = loadout.eMultiplier;
            converter.eTag = loadout.eTag;

            converter.inputList.Clear();
            converter.outputList.Clear();
            converter.reqList.Clear();

            converter.Recipe.Inputs.Clear();
            converter.Recipe.Outputs.Clear();
            converter.Recipe.Requirements.Clear();

            converter.inputList.AddRange(loadout.inputList);
            converter.outputList.AddRange(loadout.outputList);
            converter.reqList.AddRange(loadout.reqList);

            converter.Recipe.Inputs.AddRange(loadout.inputList);
            converter.Recipe.Outputs.AddRange(loadout.outputList);
            converter.Recipe.Requirements.AddRange(loadout.reqList);
        }

        private void ApplyHarvesterChanges(ModuleResourceHarvester_USI harvester, ModuleSwapOption loadout)
        {
            harvester.Efficiency = loadout.Efficiency;
            harvester.ResourceName = loadout.ResourceName;
            harvester.ConverterName = loadout.ConverterName;
            harvester.StartActionName = loadout.StartActionName;
            harvester.StopActionName = loadout.StopActionName;
            MonoUtilities.RefreshContextWindows(part);
        }
    }
}