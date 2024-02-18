﻿using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Display.Text;
using System;

using Dawnsbury.Core.Mechanics.Core;
using System.Linq;
using Dawnsbury.Phases;




namespace Dawnsbury.Mods.DawnniExpanded;

public class SpellFalseLife{
    
    public static void LoadMod()
    {
        ModdedIllustration illustrationFalseLife= new ModdedIllustration("DawnniburyExpandedAssets/FalseLife.png");
        
        QEffect FalseLifeEffect = new QEffect()
                                {
                                    Illustration = illustrationFalseLife,
                                    ExpiresAt = ExpirationCondition.Never,
                                    Description = "You are have temporary Hit Points from False Life.",
                                    Name = "False Life - ",
                                    DoNotShowUpOverhead = true,
                                    CountsAsABuff = true,

                                    YouAreDealtDamage = async (QEffect qEffect, Creature attacker, DamageStuff damageStuff, Creature you) =>
                                    {

                                        qEffect.Value -= Math.Min(damageStuff.Amount,qEffect.Value);
                                        if (qEffect.Value <= 0)
                                        {
                                            qEffect.Value = 0;
                                            qEffect.ExpiresAt = ExpirationCondition.Immediately;
                                        }
                                        
                                        return null;
                                    },

                                    EndOfCombat = async (QEffect qf, bool winstate) =>{
                                    qf.Owner.PersistentUsedUpResources.UsedUpActions.Add("FalseLife:" + qf.Value);
                                    },
                                
                                };



        ModManager.RegisterNewSpell("False Life", 1, (spellId, spellcaster, spellLevel, inCombat) =>
        {
    
        CombatAction falseLife =  Spells.CreateModern(illustrationFalseLife, "False Life", new Trait[]
            {
                Trait.Necromancy,
                Trait.Arcane,
                Trait.Occult
            }, "You ward yourself with shimmering magical energy.",
                "You gain " + S.HeightenedVariable(6+(spellLevel-1)*3, 6)+ " + " + S.SpellcastingModifier(spellcaster) + " temporary Hit Points.\n\n{b}Special{/b} You can cast this spell as a free action at the beginning of the encounter if not casting from a scroll." + S.HeightenText(spellLevel > 1, inCombat, "{b}Heightened (+1){/b} The temporary Hit Points increase by 3.")
                ,Target.Self(),
                1, 
                 null)
                .WithSoundEffect(SfxName.Healing)
                .WithActionCost(2)
                .WithEffectOnEachTarget(async (CombatAction spell, Creature caster, Creature target, CheckResult result) =>
                            {
                                int FalseLifeTHP = 6+caster.Spellcasting.SpellcastingAbilityModifier+(spell.SpellLevel-1)*3;
                                QEffect falseLifeEffect = FalseLifeEffect;
                                
                                QEffect qEffect2 = target.QEffects.FirstOrDefault((QEffect qf) => qf.Name == "False Life - ");
                                if (qEffect2 != null)
                                {
                                if (FalseLifeTHP > qEffect2.Value){
                                    caster.GainTemporaryHP(FalseLifeTHP);
                                    qEffect2.Value = FalseLifeTHP;
                                    }
                                
                                } else {
                                falseLifeEffect.Source = caster;
                                falseLifeEffect.Value = FalseLifeTHP;
                                caster.GainTemporaryHP(FalseLifeTHP);
                                caster.AddQEffect(falseLifeEffect);
                                }
                                
                        });
                        
                        falseLife.WhenCombatBegins = delegate (Creature self)
                        {
                            string LastfalseLifeString = self.PersistentUsedUpResources.UsedUpActions.Find(word => word.Contains("FalseLife:"));
                            int LastfalseLifeValue = 0;
                            if(LastfalseLifeString != null){
                              string[] LastfalseLifeStringSplit = LastfalseLifeString.Split(':');
                              QEffect falseLifeEffect = FalseLifeEffect;
                                
                            FalseLifeEffect.Source = self;
                            FalseLifeEffect.Value = Int32.Parse(LastfalseLifeStringSplit[1]);
                            self.GainTemporaryHP(FalseLifeEffect.Value);
                            LastfalseLifeValue = FalseLifeEffect.Value;
                            self.AddQEffect(falseLifeEffect);
                            self.PersistentUsedUpResources.UsedUpActions.Remove(LastfalseLifeString);

                            }
                            if (LastfalseLifeValue < self.TemporaryHP || self.TemporaryHP == 0){
                            self.AddQEffect(new QEffect
                            {
                                StartOfCombat = async delegate
                                {
                                    if (await self.Battle.AskForConfirmation(self, illustrationFalseLife, "Do you want to cast {i}false life level "+ falseLife.SpellLevel + " {/i} as a free action?", "Cast {i}false life{/i}"))
                                    {
                                        await self.Battle.GameLoop.FullCast(falseLife);
                                    }
                                }
                            });
                            }
                        };
                   
                        

                    return falseLife;
                    
    });
        
    }
}
                
                