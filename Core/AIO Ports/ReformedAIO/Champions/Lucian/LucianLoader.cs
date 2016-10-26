﻿namespace ReformedAIO.Champions.Lucian
{
    using System.Collections.Generic;
    using System.Drawing;

    using EloBuddy;
    using LeagueSharp.Common;

    using Drawings;

    using OrbwalkingMode.Combo;
    using OrbwalkingMode.JungleClear;
    using OrbwalkingMode.LaneClear;
    using OrbwalkingMode.Harass;

    using Damage;
    using Killsteal;
    using Spells;
    using Spells.SpellParent;
    using Core.Dash_Handler;

    using RethoughtLib.FeatureSystem.Guardians;
    using RethoughtLib.Bootstraps.Abstract_Classes;
    using RethoughtLib.FeatureSystem.Abstract_Classes;
    using RethoughtLib.FeatureSystem.Implementations;
    using RethoughtLib.Orbwalker.Implementations;

    using Color = SharpDX.Color;

    internal sealed class LucianLoader : LoadableBase
    {
        public override string DisplayName { get; set; } = "Reformed Lucian";

        public override string InternalName { get; set; } = "Lucian";

        public override IEnumerable<string> Tags { get; set; } = new[] { "Lucian" };

        public override void Load()
        {
            var superParent = new SuperParent(DisplayName);
            superParent.Initialize();

            var qSpell = new QSpell();
           var q2Spell = new Q2Spell();
            var wSpell = new WSpell();
            var eSpell = new ESpell();
            var rSpell = new RSpell();

            var spellParent = new SpellParent();
            spellParent.Add(new List<Base>
                                  {
                                     qSpell,
                                     q2Spell,
                                     wSpell,
                                     eSpell,
                                     rSpell
                                  });
            spellParent.Load();

            var dmg = new LucDamage(eSpell, wSpell, qSpell, rSpell);

            var dashSmart = new DashSmart();

            var orbwalkerModule = new OrbwalkerModule();
            orbwalkerModule.Load();

            var comboParent  = new OrbwalkingParent("Combo", orbwalkerModule.OrbwalkerInstance, Orbwalking.OrbwalkingMode.Combo);
            var harassParent = new OrbwalkingParent("Harass", orbwalkerModule.OrbwalkerInstance, Orbwalking.OrbwalkingMode.Mixed);
            var laneParent   = new OrbwalkingParent("Lane", orbwalkerModule.OrbwalkerInstance, Orbwalking.OrbwalkingMode.LaneClear);
            var jungleParent = new OrbwalkingParent("Jungle", orbwalkerModule.OrbwalkerInstance, Orbwalking.OrbwalkingMode.LaneClear);

            var killstealParnet = new Parent("Killsteal");
            var drawingParent = new Parent("Drawings");
            var reformedUtilityParent = new Parent("Reformed Utility");

            var lucianPassiveGuardian = new PlayerMustHaveBuff("LucianPassiveBuff") { Negated = true };
            var eMustNotBeReadyGuardian = new SpellMustBeReady(SpellSlot.E) { Negated = true };
            var qMustNotBeReadyGuardian = new SpellMustBeReady(SpellSlot.Q) { Negated = true };
            var qReadyGuardian = new SpellMustBeReady(SpellSlot.Q);
            var wReadyGuardian = new SpellMustBeReady(SpellSlot.W);
            var eReadyGuardian = new SpellMustBeReady(SpellSlot.E);

            comboParent.Add(new List<Base>
                                {
                                    new QCombo(qSpell, q2Spell).Guardian(lucianPassiveGuardian)
                                    .Guardian(eMustNotBeReadyGuardian)
                                    .Guardian(qReadyGuardian),

                                    new WCombo(wSpell).Guardian(lucianPassiveGuardian)
                                    .Guardian(qMustNotBeReadyGuardian)
                                    .Guardian(wReadyGuardian),

                                    new ECombo(eSpell, dmg, dashSmart).Guardian(lucianPassiveGuardian)
                                    .Guardian(eReadyGuardian),

                                    new RCombo(rSpell, dmg).Guardian(lucianPassiveGuardian)
                                    .Guardian(new SpellMustBeReady(SpellSlot.R)),
                                 });

            harassParent.Add(new List<Base>
                                 {
                                    new QHarass(qSpell, q2Spell).Guardian(lucianPassiveGuardian).Guardian(qReadyGuardian),
                                    new WHarass(wSpell).Guardian(lucianPassiveGuardian).Guardian(qMustNotBeReadyGuardian).Guardian(wReadyGuardian),
                                    new EHarass(eSpell, dmg, dashSmart).Guardian(lucianPassiveGuardian)
                                    .Guardian(eReadyGuardian),
                                 });

            laneParent.Add(new List<Base>
                               {
                                    new QLaneClear(qSpell).Guardian(lucianPassiveGuardian).Guardian(qReadyGuardian).Guardian(eMustNotBeReadyGuardian),
                                    new WLaneClear(wSpell).Guardian(lucianPassiveGuardian).Guardian(wReadyGuardian).Guardian(eMustNotBeReadyGuardian),
                                    new ELaneClear(eSpell, dashSmart).Guardian(lucianPassiveGuardian).Guardian(eReadyGuardian),
                               });

            jungleParent.Add(new List<Base>
                                 {
                                     new QJungleClear(qSpell).Guardian(lucianPassiveGuardian).Guardian(eMustNotBeReadyGuardian),
                                     new WJungleClear(wSpell).Guardian(lucianPassiveGuardian).Guardian(wReadyGuardian).Guardian(qMustNotBeReadyGuardian),
                                     new EJungleClear(eSpell).Guardian(eReadyGuardian)
                                 });

            killstealParnet.Add(new List<Base>
                                    {
                                        new Q(qSpell, q2Spell).Guardian(lucianPassiveGuardian).Guardian(qReadyGuardian),
                                        new W(wSpell).Guardian(wReadyGuardian),
                                        new R(rSpell).Guardian(new SpellMustBeReady(SpellSlot.R))
                                    });

            drawingParent.Add(new List<Base>
                                  {
                                    new DmgDraw(dmg),
                                    new RDraw(rSpell),
                                    new WDraw(wSpell)
                                  });

            superParent.Add(new List<Base>
                                  {
                                     reformedUtilityParent,
                                     orbwalkerModule,
                                     comboParent,
                                     harassParent,
                                     laneParent,
                                     jungleParent,
                                     killstealParnet,
                                     drawingParent,
                                  });

            superParent.Load();

            reformedUtilityParent.Menu.Style = FontStyle.Bold;
            reformedUtilityParent.Menu.Color = Color.Cyan;

            superParent.Menu.Style = FontStyle.Bold;
            superParent.Menu.Color = Color.Cyan;

            if (superParent.Loaded)
            {
                Chat.Print(DisplayName + " - Loaded");
            }
        }
    }
}
