using System;
using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;
using System.Linq;

namespace WindBot.Game.AI.Decks
{
    [Deck("YuseiFudo", "AI_Yusei")]
    public class YuseiFudoExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int StardustDragon = 44508094;
            public const int JunkSynchron = 63977066;
            public const int JunkWarrior = 60800381;
            public const int QuillboltHedgehog = 23574823;
            public const int Tuning = 63180001;
        }

        public YuseiFudoExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // --- 1. Lógica de Activación ---
            AddExecutor(ExecutorType.Activate, CardId.Tuning);
            AddExecutor(ExecutorType.Activate, DefaultDontChainMyself);

            // --- 2. Invocación Inteligente ---
            // Monstruos "As" siempre en ataque si es posible
            AddExecutor(ExecutorType.SpSummon, CardId.StardustDragon);
            
            // Lógica de supervivencia: Invocar en defensa si el rival es más fuerte
            AddExecutor(ExecutorType.Summon, SummonEnDefensaSiEsDebil);
            AddExecutor(ExecutorType.SpSummon, SummonEnDefensaSiEsDebil);

            AddExecutor(ExecutorType.Summon);
            AddExecutor(ExecutorType.SpSummon);

            // --- 3. Gestión de Campo ---
            AddExecutor(ExecutorType.Repos, ReubicarMonstruosSegunPeligro);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        // Lógica de posición al entrar al campo
        private bool SummonEnDefensaSiEsDebil()
        {
            if (Enemy.GetMonsterCount() > 0 && Util.IsOneEnemyBetterThanValue(Card.Attack, false))
            {
                AI.SelectPosition(CardPosition.FaceUpDefence);
                return true;
            }
            return false;
        }

        // Cambio de posición dinámico (Basado en BlueEyesExecutor)
        private bool ReubicarMonstruosSegunPeligro()
        {
            if (Card.IsAttack() && Util.IsOneEnemyBetterThanValue(Card.Attack, true))
                return true;

            if (Card.IsDefense() && !Util.IsOneEnemyBetterThanValue(Card.Attack, true))
            {
                if (Card.Attack >= Card.Defense || Card.Attack > 1000)
                    return true;
            }
            return false;
        }

        // --- LÓGICA DE BATALLA AVANZADA ---
        public override bool OnPreBattleBetween(ClientCard attacker, ClientCard defender)
        {
            // 1. REGLA DEL BACKROW (NUEVA):
            // Si el oponente tiene 3 o más cartas boca abajo y yo no tengo nada en mi zona de magia/trampas para defenderme
            if (Enemy.GetSpellCountWithoutField() >= 3 && Bot.GetSpellCountWithoutField() == 0)
            {
                // A menos que sea Stardust Dragon (que puede protegerse a sí mismo)
                if (!attacker.IsCode(CardId.StardustDragon))
                {
                    return false; // Abortar ataque por precaución
                }
            }

            // 2. No suicidarse:
            if (attacker.Attack <= defender.GetDefensePower())
                return false;

            return base.OnPreBattleBetween(attacker, defender);
        }

        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            // Si el enemigo tiene muchas cartas seteadas, solo atacar con el más fuerte
            if (Enemy.GetSpellCountWithoutField() >= 3 && attacker.Attack < 2500)
            {
                return null; // Detener este ataque específico
            }

            // Atacar al más débil primero (Lógica de eficiencia)
            var sortedDefenders = defenders.OrderBy(d => d.Attack).ToList();
            return base.OnSelectAttackTarget(attacker, sortedDefenders);
        }
    }
}
