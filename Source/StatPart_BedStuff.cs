﻿using RimWorld;
using System.Text;
using Verse;

namespace SoftWarmBeds
{
    public class StatPart_BedStuff : StatPart
    {
        private float Addend;

        private StatDef additiveStat = null;

        private float Factor;

        private StatDef multiplierStat = null;

        public override string ExplanationPart(StatRequest req)
        {
            Building_Bed bed = req.Thing as Building_Bed;
            if (req.HasThing && bed != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                CompMakeableBed bedComp = req.Thing.TryGetComp<CompMakeableBed>();
                string material = null;
                if (bedComp != null)
                {
                    if (bedComp.Loaded)
                    {
                        ThingDef BedStuff = bedComp.blanketStuff;
                        material = BedStuff.label;
                    }
                    else
                    {
                        material = "NoBeddings".Translate();
                    }
                }
                else if (req.StuffDef != null)
                {
                    material = req.StuffDef.label;
                }
                if (material != null)
                {
                    string number = Addend.ToStringByStyle(parentStat.ToStringStyleUnfinalized, ToStringNumberSense.Absolute);
                    if (additiveStat != null)
                    {
                        stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + material + "): +" + number);
                    }
                    if (multiplierStat != null)
                    {
                        stringBuilder.AppendLine("StatsReport_StuffEffectMultiplier".Translate() + ": x" + Factor.ToStringPercent("F0"));
                    }
                    return stringBuilder.ToString().TrimEndNewlines();
                }
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float value)
        {
            Building_Bed bed = req.Thing as Building_Bed;
            if (req.HasThing && bed != null)
            {
                //Log.Message(bed + " is calculating stats. (value " + value + ")");
                float addend = (additiveStat != null) ? SelectValue(req, additiveStat) : 0f;
                float factor = (multiplierStat != null) ? SelectValue(req, multiplierStat) : 0f;
                //Log.Message("defined: factor: " + (double)factor + " addend: " + (double)addend);
                if (multiplierStat != null)
                {
                    if (additiveStat != null)
                    {
                        value += factor * addend;
                        goto Done;
                    };
                    value *= factor;
                    goto Done;
                }
                value += addend;

                Done:
                Factor = factor;
                Addend = addend;
                return;
            }
            return;
        }

        private float SelectValue(StatRequest req, StatDef stat)
        {
            if (stat != null)
            {
                CompMakeableBed bedComp = req.Thing.TryGetComp<CompMakeableBed>();
                ThingDef stuff = null;
                if (bedComp != null)
                {
                    if (bedComp.Loaded)
                    {
                        stuff = bedComp.blanketStuff; // comp = stuff from bedding
                    }
                    else
                    {
                        if (stat == additiveStat)
                        {
                            //Log.Message(stat + " is +zero for no bedding at " + req.Thing);
                            return 0f; // unmade bed = additive zero
                        }
                    }
                }
                else
                {
                    if (req.StuffDef != null)
                    {
                        stuff = req.StuffDef; // no comp (Bedroll)
                    }
                    else
                    {
                        if (stat == additiveStat)
                        {
                            //Log.Message(stat + " is +zero for no comp at " + req.Thing);
                            return 0f; // no comp, no stuff (SleepingSpot)
                        }
                    }
                }
                bool both = additiveStat != null && multiplierStat != null;
                if (both)
                {
                    if (stat == additiveStat)
                    {
                        //Log.Message(stat + " is " + stuff.GetStatValueAbstract(stat, null) + " on " + stuff + " (case 1)");
                        return stuff.GetStatValueAbstract(stat, null); // if additive = get it from bed/bedding stuff
                    }
                    //Log.Message(stat + " is " + req.Def.GetStatValueAbstract(stat, null) + " on " + req.Def + " (case 2)");
                    //return req.Def.GetStatValueAbstract(stat, null); // Changed on 1.1: method transfered to a Def child:
                    return req.BuildableDef.GetStatValueAbstract(stat, null); // if multiplier = get it from bed
                }
                if (stat == additiveStat)
                {
                    //Log.Message(stat + " is " + stuff.GetStatValueAbstract(stat, null) + " on " + stuff + " (case 3)");
                    return stuff.GetStatValueAbstract(stat, null); // just additive = get it from bed/bedding stuff
                }
                //Log.Message(stat + " is " + req.Def.GetStatValueAbstract(stat, null) + " on " + req.Def + " (case 4)");
                //return req.Def.GetStatValueAbstract(stat, null); // Changed on 1.1: method transfered to a Def child:
                return req.BuildableDef.GetStatValueAbstract(stat, null); // just multiplier (just in case) = get from bed
            }
            //Log.Message(stat + " tried selecting without target");
            return 0f;
        }
    }
}