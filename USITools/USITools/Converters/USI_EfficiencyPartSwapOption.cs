﻿namespace USITools
{
    public class USI_EfficiencyPartSwapOption : USI_ConverterSwapOption
    {
        [KSPField]
        public double eMultiplier = 1d;

        [KSPField]
        public string eTag = "";

        public override void ApplyConverterChanges(USI_ResourceConverter converter)
        {
            UseBonus = false;  // efficiency parts should not use bonuses from other efficiency parts!
            converter.eMultiplier = eMultiplier;
            converter.eTag = eTag;

            base.ApplyConverterChanges(converter);
        }

        public override string GetInfo()
        {
            if (string.IsNullOrEmpty(eTag))
                return base.GetInfo();
            var resourceConsumption = base.GetInfo();
            int index = resourceConsumption.IndexOf("\n"); // Strip the first line containing the etag
            resourceConsumption = resourceConsumption.Substring(index + 1);
            return "Boosts efficiency of converters benefiting from a " + eTag + "\n\n" +
                "Boost power: " + eMultiplier.ToString() + resourceConsumption;
        }

        public override void PostProcess(USI_ResourceConverter converter, ConverterResults result, double deltaTime)
        {
            base.PostProcess(result, deltaTime);
            converter.EfficiencyMultiplier = result.TimeFactor / deltaTime;
        }
    }
}
