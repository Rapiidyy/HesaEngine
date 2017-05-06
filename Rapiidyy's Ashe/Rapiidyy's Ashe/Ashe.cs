using System;
using HesaEngine.SDK;
using HesaEngine.SDK.Enums;
using HesaEngine.SDK.GameObjects;

namespace Rapiidyy_s_Ashe
{
    public class Ashe : IScript
    {
        public string Name => ("Rapiidyy's Ashe");
        public string Version => ("1.0.3");
        public string Author => ("Rapiidyy");
        
        public static Spell Q, W, E, R;
        public static Item qss;
        public static Item mercurial;
        public static Item cutlass;
        public static Item bork;
        public static AIHeroClient Player => ObjectManager.Me;
        public static Orbwalker.OrbwalkerInstance MyOrbwalker;
        public static Menu Menu;

        public void OnInitialize()
        {
            Game.OnGameLoaded += () =>
            {
                Core.DelayAction(OnLoad, new Random().Next(2500, 3000));//Just delay the load so we do not ask league to do 100 millions things on game start.
            };
        }

        public void OnLoad()
        {
            if (Player.Hero != Champion.Ashe) return;

            Chat.Print("Welcome To <b><font color='#389BFF'>Rapiidyy's Ashe</font></b> v.1.0.3");

            CreateSpells();
            CreateMenu();

            Game.OnTick += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnBuffGained += Obj_AI_Base_OnBuffGained;
        }

        private void Obj_AI_Base_OnBuffGained(Obj_AI_Base sender, HesaEngine.SDK.Args.Obj_AI_BaseBuffGainedEventArgs args)
        {
            if(args.Buff.Target.IsMe)
            {
                if (Menu.Get<MenuCheckbox>("qssItem").Checked)
                {
                    if (Player.IsRooted || Player.IsStunned || Player.IsTaunted || Player.IsFeared || Player.IsCharmed)
                    {
                        if (qss.IsOwned() && qss.IsReady())
                        {
                            if (Player.CountEnemiesInRange(Player.GetAutoAttackRange(Player)) >= 1 || Player.HealthPercent < 10)
                            {
                                qss.Cast();
                            }
                        }
                        else if (mercurial.IsOwned() && mercurial.IsReady())
                        {
                            if (Player.CountEnemiesInRange(Player.GetAutoAttackRange(Player)) >= 1 || Player.HealthPercent < 10)
                            {
                                mercurial.Cast();
                            }
                        }
                    }
                }
            }
        }

        private void CreateSpells()
        {
            //this works like on L#
            Q = new Spell(SpellSlot.Q, 600);

            W = new Spell(SpellSlot.W, 1200);
            W.SetSkillshot(delay: 0, width: 60, speed: int.MaxValue, collision: true, type: SkillshotType.SkillshotCone);

            E = new Spell(SpellSlot.E, 10000);
            E.SetSkillshot(delay: 0, width: 0, speed: 1200, collision: false, type: SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R, 10000);
            R.SetSkillshot(delay: 250, speed: 1600, width: 100, collision: true, type: SkillshotType.SkillshotLine);

            qss = new Item(ItemId.Quicksilver_Sash, 0);
            mercurial = new Item(ItemId.Mercurial_Scimitar, 0);
            bork = new Item(ItemId.Blade_of_the_Ruined_King, 550);
            cutlass = new Item(ItemId.Bilgewater_Cutlass, 550);
        }

        private void CreateMenu()
        {
            Menu = Menu.AddMenu("Rapiidyy's Ashe");

            var itemMenu = Menu.AddSubMenu("Item Usage");
            itemMenu.Add(new MenuCheckbox("qssItem", "Use QSS (Quicksilver)"));
            itemMenu.Add(new MenuCheckbox("borkItem", "Use BOTRK"));
            itemMenu.Add(new MenuCheckbox("borkCombo", "Use BOTRK Only in Combo"));
            itemMenu.Add(new MenuCheckbox("borkLowHP", "Use BOTRK Only If Low HP"));

            //comboMenu
            var comboMenu = Menu.AddSubMenu("Combo");
            comboMenu.Add(new MenuCheckbox("useQ", "Use Q"));
            comboMenu.Add(new MenuCheckbox("useW", "Use W"));
            comboMenu.Add(new MenuCheckbox("useR", "Use R"));

            //fleemenu here
            var fleeMenu = Menu.AddSubMenu("Flee");
            fleeMenu.Add(new MenuCheckbox("FuseW", "Use W"));
            fleeMenu.Add(new MenuCheckbox("FuseR", "Use R"));

            //do the E menu here just E wait ok 
            var EMenu = Menu.AddSubMenu("EMenu");
            EMenu.Add(new MenuCheckbox("useE", "Use E"));
            EMenu.Add(new MenuSlider("ManaSlider", "Minimum Mana for Laneclear", 0, 100, 30));

            //Laneclear Menu here
            var LaneClear = Menu.AddSubMenu("LaneClear");
            LaneClear.Add(new MenuSlider(name: "ManaSlider", text: "Minimum Mana for LaneClear", minimumValue: 0, maximumValue: 100, currentValue: 30));
            LaneClear.Add(new MenuCheckbox("LCuseQ", "Use Q"));
            LaneClear.Add(new MenuCheckbox("LCuseW", "Use W"));
            LaneClear.Add(new MenuSlider("LCminMinions", "Cast W if will hit {0} Minions", 0, 10, 3));

            //Harass menu here
            var Harrass = Menu.AddSubMenu("Harrass");
            Harrass.Add(new MenuCheckbox("HSuseQ", "Use Q"));
            Harrass.Add(new MenuCheckbox("HSuseW", "Use W"));

            var JungleClear = Menu.AddSubMenu("JungleClear");
            JungleClear.Add(new MenuCheckbox("JCuseQ", "Use Q"));
            JungleClear.Add(new MenuCheckbox("JCuseW", "Use W"));
            JungleClear.Add(new MenuSlider(name: "JCManaSlider", text: "Minimum Mana for JungleClear", minimumValue: 0, maximumValue: 100, currentValue: 30));

            MyOrbwalker = new Orbwalker.OrbwalkerInstance(Menu.AddSubMenu("Orbwalker"));

        }//create menu ends here

        private void OnDraw(EventArgs args)
        {
            Drawing.DrawCircle(Player.Position, W.Range, SharpDX.Color.Pink); 
        }

        public void OnUpdate()
        {
                //if you press space, do ComboExecute();
                if (MyOrbwalker.ActiveMode == (Orbwalker.OrbwalkingMode.Combo)) ComboExecute();
                else if (MyOrbwalker.ActiveMode == (Orbwalker.OrbwalkingMode.LaneClear)) LaneClearExecute();
                else if (MyOrbwalker.ActiveMode == (Orbwalker.OrbwalkingMode.Harass)) HarrassExecute();
                else if (MyOrbwalker.ActiveMode == (Orbwalker.OrbwalkingMode.JungleClear)) JungleClearExecute();
                else if (MyOrbwalker.ActiveMode == (Orbwalker.OrbwalkingMode.Flee)) FleeExecute();

                ItemUsage();
        }

        public void ItemUsage()
        {
            if (Menu.Get<MenuCheckbox>("borkCombo").Checked && MyOrbwalker.ActiveMode != Orbwalker.OrbwalkingMode.Combo) return;

            if (Menu.Get<MenuCheckbox>("borkItem").Checked)
            {
                if (bork.IsOwned() && bork.IsReady())
                {
                    var enemy = TargetSelector.GetTarget(Player.GetAutoAttackRange(Player), TargetSelector.DamageType.Physical);
                    if (Menu.Get<MenuCheckbox>("borkLowHP").Checked)
                    {
                        if (enemy.HealthPercent > 50)
                        {
                            bork.Cast(enemy);
                        }
                    }
                    else
                    {
                        bork.Cast(enemy);
                    }
                }else if(cutlass.IsOwned() && cutlass.IsReady())
                {
                    var enemy = TargetSelector.GetTarget(Player.GetAutoAttackRange(Player), TargetSelector.DamageType.Physical);
                    if (Menu.Get<MenuCheckbox>("borkLowHP").Checked)
                    {
                        if (enemy.HealthPercent > 50)
                        {
                            cutlass.Cast(enemy);
                        }
                    }
                    else
                    {
                        cutlass.Cast(enemy);
                    }
                }
            }
        }

        private static void FleeExecute()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

            if (target == null || Player.IsDead) return;

            if (Menu.Get<MenuCheckbox>("FuseW").Checked && W.IsReady())
            {
                W.WillHit(target, Player.Position);
                W.Cast(target);
            }

            if (Menu.Get<MenuCheckbox>("FuseR").Checked && R.IsReady() && target.IsInRange(Player,R.Range))
            {
                var Rprediction = R.GetPrediction(target);

                if (Rprediction.Hitchance >= HitChance.VeryHigh)
                {
                    R.Cast(target);
                }
            }
        }

        private static void ComboExecute()
        {
            var target = TargetSelector.GetTarget(Player.GetAutoAttackRange(Player), TargetSelector.DamageType.Physical);

            if (target == null || Player.IsDead) return;

            
            if (Menu.Get<MenuCheckbox>("useQ").Checked && Q.IsReady())
            {
                 Q.Cast();
            }
            if (Menu.Get<MenuCheckbox>("useW").Checked && W.IsReady())
            {
                var Wprediction = W.GetPrediction(target);

                if (Wprediction.Hitchance >= HitChance.High)
                {
                    W.Cast(Wprediction.CastPosition);
                }
            }
            if (Menu.Get<MenuCheckbox>("useR").Checked && R.IsReady())
            {
                var Rprediction = R.GetPrediction(target);

                if (Rprediction.Hitchance >= HitChance.High)
                {
                    R.Cast(Rprediction.CastPosition);
                }

            }
        }

        private static void LaneClearExecute()
        {
            var lanemonster = MinionManager.GetMinions(Player.GetAutoAttackRange(Player), MinionTypes.All, MinionTeam.Enemy);

            if (lanemonster == null)
            {
                return;
            }
            if (lanemonster != null)
            {
                if (Menu.Get<MenuCheckbox>("LCuseQ").Checked && Q.IsReady())
                {
                    if (ObjectManager.Player.ManaPercent >= Menu.Get<MenuSlider>("LaneClearMana").CurrentValue)
                    {
                        Q.Cast();
                    }
                }
                if (Menu.Get<MenuCheckbox>("LCuseW").Checked && W.IsReady())
                {
                    if (ObjectManager.Player.ManaPercent >= Menu.Get<MenuSlider>("LaneClearMana").CurrentValue)
                    {
                        foreach (var minion in lanemonster)
                        {
                            W.CastIfWillHit(minion, Menu.Get<MenuSlider>("LCminMinions").CurrentValue);
                        }
                    }
                }
            }
        }

        private static void JungleClearExecute()
        {
            var minion = MinionManager.GetMinions(Player.GetAutoAttackRange(Player), MinionTypes.All, MinionTeam.Neutral);

            if (minion == null)
            {
                return;
            }

            if (minion != null)
            {
                if (Menu.Get<MenuCheckbox>("JCuseQ").Checked && Q.IsReady())
                {
                    if (ObjectManager.Player.ManaPercent >= Menu.Get<MenuSlider>("JunngleClearMana").CurrentValue)
                    {
                        Q.Cast();
                    }
                }
                if (Menu.Get<MenuCheckbox>("JCuseW").Checked && W.IsReady())
                {
                    if (ObjectManager.Player.ManaPercent >= Menu.Get<MenuSlider>("JungleClearMana").CurrentValue)

                    {
                        foreach (var monster in minion)
                        {
                            W.Cast(monster); 
                        }
                    }
                }
            }
        }

        private static void HarrassExecute()
        {
            var target = TargetSelector.GetTarget(Player.GetAutoAttackRange(Player), TargetSelector.DamageType.Physical);

            if (target == null || Player.IsDead) return;

            if (Menu.Get<MenuCheckbox>("HSuseQ").Checked && Q.IsReady())
            {
                Q.Cast();
            }
            if (Menu.Get<MenuCheckbox>("HSuseW").Checked && W.IsReady())
            {
                var Wprediction = W.GetPrediction(target);

                if (Wprediction.Hitchance >= HitChance.High)
                {
                    W.Cast(Wprediction.CastPosition);
                }
            }
        }

    }
}