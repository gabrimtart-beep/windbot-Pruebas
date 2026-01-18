using System;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    [Deck("YuseiFudo", "AI_Yusei")]
    public class YuseiFudoExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int JunkSynchron = 63977008;
            public const int Doppelwarrior = 53855409;
            public const int JunkWarrior = 60800381;
            public const int ArriveInLight = 365213;
            public const int QuickdrawSynchron = 20932152;
            public const int JunkAnchor = 96182448;
            public const int GravityWarrior = 44035031;
            public const int StardustDragon = 44508094;
            public const int Tuning = 96363153;
            public const int StarlightRoad = 58120309;
            public const int ScrapIronScarecrow = 98427577;
            public const int EffectVeiler = 97268402;
        }

        public YuseiFudoExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // --- 1. REACCIONES Y TRAMPAS (Prioridad de Activación) ---
            AddExecutor(ExecutorType.Activate, CardId.StarlightRoad);
            AddExecutor(ExecutorType.Activate, CardId.ScrapIronScarecrow, () => Duel.Phase == DuelPhase.Battle);
            AddExecutor(ExecutorType.Activate, CardId.EffectVeiler, DefaultEffectVeiler);
            AddExecutor(ExecutorType.Activate, CardId.ArriveInLight, ArriveInLightLogic);

            // --- 2. BUSCADORES ---
            AddExecutor(ExecutorType.Activate, CardId.Tuning, TuningLogic);

            // --- 3. INVOCACIONES ESPECIALES (Combos) ---
            AddExecutor(ExecutorType.SpSummon, CardId.QuickdrawSynchron, QuickdrawSynchronLogic);
            AddExecutor(ExecutorType.SpSummon, CardId.JunkWarrior, JunkWarriorComboLogic);
            AddExecutor(ExecutorType.SpSummon, CardId.GravityWarrior, () => Enemy.GetMonsterCount() >= 3);
            AddExecutor(ExecutorType.SpSummon, CardId.StardustDragon);
            AddExecutor(ExecutorType.Activate, CardId.Doppelwarrior); // Activar tokens

            // --- 4. INVOCACIONES NORMALES ---
            // Regla Veiler: No invocar ni colocar
            AddExecutor(ExecutorType.Summon, CardId.EffectVeiler, () => false);
            AddExecutor(ExecutorType.MonsterSet, CardId.EffectVeiler, () => false);

            AddExecutor(ExecutorType.Summon, CardId.JunkSynchron, JunkSynchronNormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.Doppelwarrior, () => Bot.HasInMonstersZone(CardId.JunkSynchron));
            
            // Invocación inteligente genérica
            AddExecutor(ExecutorType.Summon, SmartSummonLogic);
            AddExecutor(ExecutorType.MonsterSet, () => !HasTunerInField());

            // --- 5. OTROS ---
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.Repos, MonsterReposLogic);
        }

        private bool TuningLogic()
        {
            // Activar automáticamente si no hay Tuners en mano
            bool hasTunerInHand = Bot.Hand.Any(c => c != null && c.HasType(CardType.Tuner));
            return !hasTunerInHand;
        }

        private bool ArriveInLightLogic()
        {
            if (Card.Location == CardLocation.Hand) return true;
            bool hasTunerInHand = Bot.Hand.Any(c => c != null && c.HasType(CardType.Tuner));
            AI.SelectOption(hasTunerInHand ? 1 : 0);
            return true;
        }

        private bool QuickdrawSynchronLogic()
        {
            ClientCard discardTarget = Bot.Hand.FirstOrDefault(c => c.IsCode(CardId.JunkAnchor)) 
                                      ?? Bot.Hand.FirstOrDefault(c => c.HasType(CardType.Tuner) && c.Level <= 3);
            if (discardTarget != null)
            {
                AI.SelectCard(discardTarget);
                AI.SelectPosition(CardPosition.FaceUpDefence);
                return true;
            }
            return false;
        }

        private bool JunkWarriorComboLogic()
        {
            if (Bot.HasInMonstersZone(CardId.JunkSynchron) && Bot.HasInMonstersZone(CardId.Doppelwarrior))
            {
                AI.SelectMaterials(new[] { CardId.JunkSynchron, CardId.Doppelwarrior });
                return true;
            }
            return false;
        }

        private bool JunkSynchronNormalSummon()
        {
            return Bot.HasInHand(CardId.Doppelwarrior) || Bot.HasInGraveyard(CardId.Doppelwarrior) || Bot.Graveyard.Any(c => c.Level <= 2);
        }

        private bool SmartSummonLogic()
        {
            // Evitar invocar Effect Veiler (doble chequeo)
            if (Card.IsCode(CardId.EffectVeiler)) return false;

            if (Util.IsOneEnemyBetterThanValue(Card.Attack, false))
            {
                return HasTunerInField() || Bot.Hand.Any(c => c.HasType(CardType.Tuner));
            }
            return true;
        }

        private bool MonsterReposLogic()
        {
            if (Card.IsAttack() && Util.IsOneEnemyBetterThanValue(Card.Attack, true))
                return true;
            return false;
        }

        private bool HasTunerInField()
        {
            return Bot.MonsterZone.Any(c => c != null && c.HasType(CardType.Tuner));
        }

        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            if (attacker.IsCode(CardId.JunkWarrior))
            {
                ClientCard strongestEnemy = Enemy.MonsterZone.GetHighestAttackMonster();
                if (strongestEnemy != null && attacker.Attack > strongestEnemy.Attack)
                {
                    return AI.Attack(attacker, strongestEnemy);
                }
            }
            return base.OnSelectAttackTarget(attacker, defenders);
        }
    }
}
