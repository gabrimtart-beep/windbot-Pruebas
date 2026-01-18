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
            // 1. Interrupciones y Buscadores
            AddExecutor(ExecutorType.Activate, CardId.EffectVeiler, DefaultEffectVeiler);
            AddExecutor(ExecutorType.Activate, CardId.Tuning, TuningLogic);

            // 2. Extra Deck - Sincronía (Shooting Star > Stardust)
            AddExecutor(ExecutorType.SpSummon, CardId.ShootingStarDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.StardustDragon, StardustLogic);
            AddExecutor(ExecutorType.SpSummon, CardId.JunkWarrior);

            // 3. REGLA DE DEFENSA: Colocar si el rival es más fuerte que nuestros monstruos en mano
            AddExecutor(ExecutorType.MonsterSet, () => 
                Enemy.GetMonsterCount() > 0 && Util.IsOneEnemyBetterThanValue(1800, false));

            // 4. Invocaciones de Main Deck
            AddExecutor(ExecutorType.Summon, CardId.JunkSynchron, JunkSynchronLogic);
            AddExecutor(ExecutorType.Summon, CardId.Doppelwarrior);
            AddExecutor(ExecutorType.Summon); 
            AddExecutor(ExecutorType.SpSummon, CardId.QuillboltHedgehog);

            // 5. Magias/Trampas y SelectPlace (Inspirado en TimeThief)
            AddExecutor(ExecutorType.Activate, CardId.ScrapIronScarecrow, ScarecrowLogic);
            AddExecutor(ExecutorType.SpellSet, CardId.ScrapIronScarecrow, TrapSetLogic);
            AddExecutor(ExecutorType.SpellSet, CardId.StarlightRoad, TrapSetLogic);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);

            // 6. Reposición
            AddExecutor(ExecutorType.Repos, MonsterReposLogic);
        }

        private bool TuningLogic()
        {
            return Bot.Deck.Count > 3;
        }

        private bool JunkSynchronLogic()
        {
            // Solo invocar si hay un objetivo válido en cementerio para no perder el efecto
            return Bot.Graveyard.Any(c => c.Level <= 2);
        }

        private bool StardustLogic()
        {
            // Evitar invocar si el oponente tiene cartas de negación masiva activas
            return !Enemy.HasInSpellZone(CardId.StarlightRoad); 
        }

        private bool ScarecrowLogic()
        {
            return Duel.Player == 1 && Duel.Phase == DuelPhase.Battle;
        }

        private bool TrapSetLogic()
        {
            if (Bot.GetSpellCountWithoutField() >= 4) return false;
            // Usar zonas seguras para evitar efectos de columna
            AI.SelectPlace(Zones.z0 | Zones.z4 | Zones.z1 | Zones.z3);
            return true;
        }

        private bool MonsterReposLogic()
        {
            if (Card.IsFacedown()) return true;
            // Si estamos en ataque pero el enemigo es más fuerte, pasar a defensa
            if (Card.IsAttack() && Util.IsOneEnemyBetterThanValue(Card.Attack, true))
                return true;
            return DefaultMonsterRepos();
        }

        // CORRECCIÓN DEL ERROR DE COMPILACIÓN:
        // Se cambió la firma para coincidir con el override de la clase base
        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, int hint, bool cancelable)
        {
            // Si el juego nos pide tributar o descartar, intentamos NO elegir a los Ases de Sincronía
            if (cards.Any(c => c.IsCode(CardId.StardustDragon) || c.IsCode(CardId.ShootingStarDragon)))
            {
                var preferred = cards.Where(c => !c.IsCode(CardId.StardustDragon) && !c.IsCode(CardId.ShootingStarDragon)).ToList();
                if (preferred.Count >= min) return base.OnSelectCard(preferred, min, max, hint, cancelable);
            }
            return base.OnSelectCard(cards, min, max, hint, cancelable);
        }

        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            // Lógica de ataque precavido si hay muchas trampas (Escenario A)
            if (Enemy.GetSpellCountWithoutField() >= 2 && attacker.Attack < 2500)
            {
                if (attacker.Attack <= 1500) return null;
            }
            return base.OnSelectAttackTarget(attacker, defenders);
        }
    }
}
