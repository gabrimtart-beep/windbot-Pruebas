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
            AddExecutor(ExecutorType.Activate, CardId.Tuning);

            // 2. Extra Deck - Sincronía
            AddExecutor(ExecutorType.SpSummon, CardId.ShootingStarDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.StardustDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.JunkWarrior);

            // 3. REGLA DE DEFENSA (Basada en Blue-Eyes)
            AddExecutor(ExecutorType.MonsterSet, () => 
                Enemy.GetMonsterCount() > 0 && Util.IsOneEnemyBetterThanValue(1800, false));

            // 4. Invocaciones de Main Deck
            AddExecutor(ExecutorType.Summon, CardId.JunkSynchron, JunkSynchronLogic);
            AddExecutor(ExecutorType.Summon, CardId.Doppelwarrior);
            AddExecutor(ExecutorType.Summon); 
            AddExecutor(ExecutorType.SpSummon, CardId.QuillboltHedgehog);

            // 5. Magias/Trampas y SelectPlace (Basada en TimeThief)
            AddExecutor(ExecutorType.Activate, CardId.ScrapIronScarecrow, ScarecrowLogic);
            AddExecutor(ExecutorType.SpellSet, CardId.ScrapIronScarecrow, TrapSetLogic);
            AddExecutor(ExecutorType.SpellSet, CardId.StarlightRoad, TrapSetLogic);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);

            // 6. Reposición
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool JunkSynchronLogic()
        {
            return Bot.Graveyard.Any(c => c.Level <= 2);
        }

        private bool ScarecrowLogic()
        {
            return Duel.Player == 1 && Duel.Phase == DuelPhase.Battle;
        }

        private bool TrapSetLogic()
        {
            if (Bot.GetSpellCountWithoutField() >= 4) return false;
            // Evita la columna central (Zonas 0, 1, 3, 4 son seguras)
            AI.SelectPlace(Zones.z0 | Zones.z1 | Zones.z3 | Zones.z4);
            return true;
        }

        // --- SOLUCIÓN AL ERROR CS0115 ---
        // Se ajustó la firma eliminando 'int hint' o usando la firma universal de WindBot
        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, bool cancelable)
        {
            // Priorizar no descartar o tributar a los monstruos de Sincronía
            if (cards.Any(c => c.IsCode(CardId.StardustDragon, CardId.ShootingStarDragon)))
            {
                var filter = cards.Where(c => !c.IsCode(CardId.StardustDragon, CardId.ShootingStarDragon)).ToList();
                if (filter.Count >= min) return base.OnSelectCard(filter, min, max, cancelable);
            }
            return base.OnSelectCard(cards, min, max, cancelable);
        }

        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            // Lógica de ataque precavido (Inspirado en Level8Executor)
            if (Enemy.GetSpellCountWithoutField() >= 2 && attacker.Attack < 2000)
                return null;
            
            return base.OnSelectAttackTarget(attacker, defenders);
        }
    }
}
