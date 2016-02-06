using System;
using System.Collections.Generic;
using System.Linq;
using USITools.Logistics;

namespace KolonyTools
{
    public class ModuleDistributedWarehouse : PartModule
    {
        [KSPField] 
        public float LogisticsRange = 2000f;

        private double lastCheck;

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (Math.Abs(Planetarium.GetUniversalTime() - lastCheck) < LogisticsSetup.Instance.Config.WarehouseTime)
                return;

            lastCheck = Planetarium.GetUniversalTime();

            foreach(var res in part.Resources.list)
            {
                LevelResources(res.resourceName);
            }
        }

        private void LevelResources(string resource)
        {
            var nearbyShips = LogisticsTools.GetNearbyVessels(LogisticsRange, true, vessel, true);
            var depots = new List<Vessel>();
            foreach (var d in nearbyShips)
            {
                if (d.FindPartModulesImplementing<ModuleDistributedWarehouse>().Any()
                    && d.Parts.Any(p=>p.Resources.Contains(resource)))
                    depots.Add(d);
            }
            //Get relevant parts
            var resParts = new List<Part>();
            foreach (var d in depots)
            {
                resParts.AddRange(d.parts.Where(p => p.Resources.Contains(resource) && p.Modules.Contains("ModuleDistributedWarehouse")));
            }
            var amountSum = 0d;
            var maxSum = 0d;

            //Figure out our average fill percent
            foreach (var p in resParts)
            {
                var res = p.Resources[resource];
                amountSum += res.amount;
                maxSum += res.maxAmount;
            }
            if (maxSum > 0 && amountSum > 0)
            {
                double fillPercent = amountSum/maxSum;
                //Level everything
                foreach (var p in resParts)
                {
                    var res = p.Resources[resource];
                    res.amount = res.maxAmount*fillPercent;
                }
            }

        }
    }
}