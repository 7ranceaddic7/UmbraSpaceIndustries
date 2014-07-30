﻿using System;
using System.Linq;
using UnityEngine;

namespace USITools
{
    public class USIAnimation : PartModule
    {
        [KSPField]
        public string deployAnimationName = "Deploy";

        [KSPField]
        public string secondaryAnimationName = "";

        [KSPField(isPersistant = true)]
        public bool isDeployed = false;

        [KSPField]
        public bool inflatable = false;

        [KSPField] 
        public string inflatedResources = "";

        public Animation DeployAnimation
        {
            get
            {
                return part.FindModelAnimators(deployAnimationName)[0];
            }
        }

        public Animation SecondaryAnimation
        {
            get
            {
                try
                {
                    return part.FindModelAnimators(secondaryAnimationName)[0]; 
                }
                catch (Exception ex)
                {
                    print("[OKS] Could not find secondary animation - " + secondaryAnimationName);
                    return null;
                }
            }
        }

        
        [KSPEvent(guiName = "Deploy", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void DeployModule()
        {
            if (!isDeployed)
            {
                PlayDeployAnimation();
                isDeployed = true;
                ToggleEvent("DeployModule", false);
                ToggleEvent("RetractModule", true);
                if (inflatable && inflatedResources != "")
                {
                    ExpandResourceCapacity();
                }
            }
        }

        [KSPEvent(guiName = "Retract", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void RetractModule()
        {
            if (isDeployed)
            {
                ReverseDeployAnimation();
                isDeployed = false;
                ToggleEvent("DeployModule", true);
                ToggleEvent("RetractModule", false);
                if (inflatable && inflatedResources != "")
                {
                    CompressResourceCapacity();
                }
            }
        }

        private void PlayDeployAnimation(int speed = 1)
        {
            DeployAnimation[deployAnimationName].speed = speed;
            DeployAnimation.Play(deployAnimationName);
            if (secondaryAnimationName != "")
            {
                SecondaryAnimation[secondaryAnimationName].speed = 1;
                SecondaryAnimation.wrapMode = WrapMode.Loop;
                SecondaryAnimation.Play(secondaryAnimationName);
            }
        }

        public void ReverseDeployAnimation(int speed = -1)
        {
            if (secondaryAnimationName != "")
            {
                SecondaryAnimation.Stop(secondaryAnimationName);
            }
            DeployAnimation[deployAnimationName].time = DeployAnimation[deployAnimationName].length;
            DeployAnimation[deployAnimationName].speed = speed;
            DeployAnimation.Play(deployAnimationName);
        }

        private void ToggleEvent(string eventName, bool state)
        {
            Events[eventName].active = state;
            Events[eventName].externalToEVAOnly = state;
            Events[eventName].guiActive = state;
            Events[eventName].guiActiveEditor = state;
            if (inflatedResources != "")
            {
                Events[eventName].guiActiveEditor = false;
            }

        }

        public override void OnStart(StartState state)
        {
            try
            {
                DeployAnimation[deployAnimationName].layer = 2;
                if (secondaryAnimationName != "")
                {
                    SecondaryAnimation[secondaryAnimationName].layer = 3;
                }
                CheckAnimationState();
                base.OnStart(state);
            }
            catch (Exception ex)
            {
                print("ERROR IN USIAnimationOnStart - " + ex.Message);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            try
            {
                CheckAnimationState();
            }
            catch (Exception ex)
            {
                print("ERROR IN USIAnimationOnLoad - " + ex.Message);
            }
        }

        private void CheckAnimationState()
        {
            if (isDeployed)
            {
                ToggleEvent("DeployModule", false);
                ToggleEvent("RetractModule", true);
                PlayDeployAnimation(1000);
            }
            else
            {
                ToggleEvent("DeployModule", true);
                ToggleEvent("RetractModule", false); 
                ReverseDeployAnimation(-1000);
            }
        }

        private void ExpandResourceCapacity()
        {
            try
            {
                var res = inflatedResources.Split(',');
                for (int i = 0; i < res.Count(); i += 2)
                {
                    var resName = res[i];
                    var resQty = 0m;
                    if (Decimal.TryParse(res[i + 1], out resQty))
                    {
                        if (part.Resources.Contains(resName))
                        {
                            var r = part.Resources[resName];
                            r.maxAmount = (double) resQty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                print("Error in ExpandResourceCapacity - " + ex.Message);
            }
        }

        private void CompressResourceCapacity()
        {
            try
            {
                var res = inflatedResources.Split(',');
                for (int i = 0; i < res.Count(); i += 2)
                {
                    var resName = res[i];
                    if (part.Resources.Contains(resName))
                    {
                        var r = part.Resources[resName];
                        r.maxAmount = 1;
                        if (r.amount > 1) r.amount = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                print("Error in CompressResourceCapacity - " + ex.Message);
            }
        }
    }
}
