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
            public const int QuickdrawSynchron = 20932152;
            public const int UnknownSynchron = 15310033;
            public const int JetSynchron = 9742784;
            public const int Tuning = 96363153;
            public const int QuillboltHedgehog = 23571046;
            public const int StardustDragon = 44508094;
            public const int JunkWarrior = 60800381;
            public const int ShieldWing = 17201951;
        }

        public YuseiFudoExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // --- FASE 1: PREPARACIÓN INTELECTUAL (Buscadores) ---
            AddExecutor(ExecutorType.Activate, CardId.Tuning);

            // --- FASE 2: CÁLCULO DE POSIBILIDADES (Invocaciones Especiales) ---
            AddExecutor(ExecutorType.SpSummon, CardId.QuickdrawSynchron, QuickdrawLogic);
            AddExecutor(ExecutorType.SpSummon, CardId.UnknownSynchron);
            AddExecutor(ExecutorType.SpSummon, CardId.QuillboltHedgehog);

            // --- FASE 3: EJECUCIÓN DE COMBOS DINÁMICOS (Extra Deck) ---
            // El bot invocará del Extra Deck si el ATK resultante ayuda a ganar o superar al rival
            AddExecutor(ExecutorType.SpSummon, () => {
                int enemyMaxAtk = Enemy.MonsterZone.Where(c => c != null && c.IsFaceup()).Max(c => (int?)c.Attack) ?? 0;
                return Card.Attack > enemyMaxAtk || Bot.LifePoints < 3000;
            });

            // --- FASE 4: LÓGICA DE INVOCACIÓN NORMAL ADAPTATIVA ---
            // 1. Combo principal si el cementerio está listo
            AddExecutor(ExecutorType.Summon, CardId.JunkSynchron, () => Bot.Graveyard.Any(c => c.Level <= 2));

            // 2. Lógica de "Parejas": Si tengo un Tuner, busco un No-Tuner y viceversa
            AddExecutor(ExecutorType.Summon, () => {
                bool hasTuner = Bot.MonsterZone.Any(m => m != null && m.IsFaceup() && m.HasType(CardType.Tuner));
                bool hasNonTuner = Bot.MonsterZone.Any(m => m != null && m.IsFaceup() && !m.HasType(CardType.Tuner));
                
                if (hasTuner && !Card.HasType(CardType.Tuner)) return true; // Completar pareja para Sincro
                if (hasNonTuner && Card.HasType(CardType.Tuner)) return true; // Completar pareja para Sincro
                
                // Si el campo está vacío, no "pasar turno". Invocar para defender.
                return Bot.GetMonsterCount() == 0;
            });

            // 3. Supervivencia: Shield Wing si el riesgo es alto
            AddExecutor(ExecutorType.Summon, CardId.ShieldWing, () => Bot.GetMonsterCount() == 0 || Enemy.GetMonsterCount() > 1);
            AddExecutor(ExecutorType.MonsterSet, CardId.ShieldWing, () => Bot.GetMonsterCount() == 0);

            // --- FASE 5: REACCIÓN Y MAGIAS ---
            AddExecutor(ExecutorType.Activate, GenericSmartActivation);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool QuickdrawLogic()
        {
            // El bot "razona" que solo debe bajar a Quickdraw si tiene algo útil que descartar
            return Bot.Hand.Any(c => c.IsCode(CardId.QuillboltHedgehog, CardId.JetSynchron));
        }

        private bool GenericSmartActivation()
        {
            // Lógica de activación basada en el estado del duelo
            // Si es un efecto de robo, siempre es bueno
            if (Card.Id == 67169062) return Bot.Graveyard.Count >= 5; // Pot of Avarice
            
            // Si el efecto destruye cartas, solo usarlo si el rival tiene monstruos fuertes
            int enemyMaxAtk = Enemy.MonsterZone.Where(c => c != null && c.IsFaceup()).Max(c => (int?)c.Attack) ?? 0;
            if (enemyMaxAtk > 2000) return true;

            return true;
        }

        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            // El bot prioriza destruir Cantantes (Tuners) del rival para evitar que el rival haga Sincro
            foreach (ClientCard defender in defenders)
            {
                if (defender.HasType(CardType.Tuner) && attacker.Attack > defender.Attack)
                    return AI.Attack(attacker, defender);
            }
            return base.OnSelectAttackTarget(attacker, defenders);
        }
    }
}
