using System;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    [Deck("YuseiFudo", "AI_Yusei_V2")]
    public class YuseiFudoExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int StardustDragon = 44508094;
            public const int ShootingStarDragon = 24696097;
            public const int JunkSynchron = 63977066;
            public const int JunkWarrior = 60800381;
            public const int QuillboltHedgehog = 23574823;
            public const int Doppelwarrior = 53855409;
            public const int JetSynchron = 9742784;
            public const int Tuning = 63180001;
            public const int ScrapIronScarecrow = 35346668;
            public const int EffectVeiler = 97268402;
            public const int StarlightRoad = 58120400;
        }

        public YuseiFudoExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // 1. Interrupciones y Magias de búsqueda (Prioridad alta como en Swordsoul)
            AddExecutor(ExecutorType.Activate, CardId.EffectVeiler, DefaultEffectVeiler);
            AddExecutor(ExecutorType.Activate, CardId.Tuning, TuningLogic);

            // 2. Invocaciones Especiales de Sincronía (Lógica de "Shooting Star" primero)
            AddExecutor(ExecutorType.SpSummon, CardId.ShootingStarDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.StardustDragon, StardustLogic);
            AddExecutor(ExecutorType.SpSummon, CardId.JunkWarrior);

            // 3. REGLA DE DEFENSA: Colocar monstruos si el rival es peligroso
            // Inspirado en el check de "enemyBetter" de Blue-Eyes
            AddExecutor(ExecutorType.MonsterSet, () => 
                (Enemy.GetMonsterCount() > 0 && Util.IsOneEnemyBetterThanValue(1800, false)) || 
                Bot.LifePoints < 2000);

            // 4. Invocaciones Normales y combos de Yusei
            AddExecutor(ExecutorType.Summon, CardId.JunkSynchron, JunkSynchronLogic);
            AddExecutor(ExecutorType.Summon, CardId.Doppelwarrior);
            AddExecutor(ExecutorType.Summon); // Invocación genérica si no hay otra opción

            // 5. Soporte y Trampas (Uso de SelectPlace como en TimeThief)
            AddExecutor(ExecutorType.Activate, CardId.ScrapIronScarecrow, ScarecrowLogic);
            AddExecutor(ExecutorType.SpellSet, CardId.ScrapIronScarecrow, TrapSetLogic);
            AddExecutor(ExecutorType.SpellSet, CardId.StarlightRoad, TrapSetLogic);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);

            // 6. Ajustes de posición
            AddExecutor(ExecutorType.Repos, MonsterReposLogic);
        }

        // --- LÓGICA DE CARTAS ESPECÍFICAS ---

        private bool TuningLogic()
        {
            // No activar si el mazo tiene pocas cartas (para evitar Deck Out como en Tearlaments)
            return Bot.Deck.Count > 3;
        }

        private bool JunkSynchronLogic()
        {
            // Situación B: Solo invocar si puede revivir algo para Sincronía inmediata
            return Bot.Graveyard.Any(c => c.Level <= 2);
        }

        private bool StardustLogic()
        {
            // No invocar si el rival tiene cartas que "toman el control" (inspirado en Orcust)
            return !Enemy.HasInMonstersZone(new[] { 10045474 }); // Ejemplo: Infinite Impermanence o similares
        }

        private bool ScarecrowLogic()
        {
            // Solo activar en el paso de batalla del oponente
            return Duel.Player == 1 && Duel.Phase == DuelPhase.Battle;
        }

        private bool TrapSetLogic()
        {
            // No colocar más de 3 trampas para evitar quedar bloqueado (como en ToadallyAwesome)
            if (Bot.GetSpellCountWithoutField() >= 3) return false;
            
            // Colocar en las zonas de los extremos (0 o 4) para evitar columnas de "Mekk-Knight"
            AI.SelectPlace(Zones.z0 | Zones.z4 | Zones.z1 | Zones.z3);
            return true;
        }

        // --- LÓGICA DE BATALLA Y POSICIÓN ---

        private bool MonsterReposLogic()
        {
            // Si el monstruo está boca abajo, siempre voltear si el campo es seguro
            if (Card.IsFacedown()) return true;

            // Cambiar a defensa si el oponente tiene algo más fuerte
            if (Card.IsAttack() && Util.IsOneEnemyBetterThanValue(Card.Attack, true))
                return true;

            // Si es un monstruo débil (Tuner) en defensa, dejarlo ahí
            if (Card.IsDefense() && Card.Attack < 1500)
                return false;

            return DefaultMonsterRepos();
        }

        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            // Si el rival tiene mucho Backrow (3+ cartas), Yusei ataca con cautela (Escenario A)
            if (Enemy.GetSpellCountWithoutField() >= 3 && attacker.Attack < 2000)
            {
                // Si el atacante no es el más fuerte, mejor no arriesgar
                if (attacker != Bot.MonsterZone.GetHighestAttackMonster())
                    return null;
            }
            return base.OnSelectAttackTarget(attacker, defenders);
        }

        // Manejo de efectos de selección (Inspirado en Swordsoul)
        public override bool OnSelectCard(IList<ClientCard> cards, int min, int max, int hint, bool cancelable)
        {
            // Priorizar siempre proteger a Stardust Dragon de efectos de coste propios
            if (hint == HintMsg.Release || hint == HintMsg.Tribute)
            {
                var targets = cards.Where(c => !c.IsCode(CardId.StardustDragon)).ToList();
                if (targets.Count >= min) return base.OnSelectCard(targets, min, max, hint, cancelable);
            }
            return base.OnSelectCard(cards, min, max, hint, cancelable);
        }
    }
}
