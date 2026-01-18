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

            // 3. Reglas de Invocación de Monstruos
            AddExecutor(ExecutorType.Summon, CardId.JunkSynchron, JunkSynchronLogic);
            AddExecutor(ExecutorType.Summon, CardId.Doppelwarrior);
            AddExecutor(ExecutorType.SpSummon, CardId.QuillboltHedgehog);
            AddExecutor(ExecutorType.Summon); // Invocación genérica por si acaso
            
            AddExecutor(ExecutorType.MonsterSet, () => 
                Enemy.GetMonsterCount() > 0 && Util.IsOneEnemyBetterThanValue(1800, false));

            // 4. Magias y Trampas
            AddExecutor(ExecutorType.Activate, CardId.ScrapIronScarecrow, ScarecrowLogic);
            AddExecutor(ExecutorType.SpellSet, CardId.ScrapIronScarecrow, TrapSetLogic);
            AddExecutor(ExecutorType.SpellSet, CardId.StarlightRoad, TrapSetLogic);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);

            // 5. Reposición de posición
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool JunkSynchronLogic()
        {
            // Selecciona un monstruo de nivel bajo en el cementerio para el efecto de Junk Synchron
            ClientCard target = Bot.Graveyard.FirstOrDefault(c => c.Level <= 2);
            if (target != null)
            {
                AI.SelectCard(target);
                return true;
            }
            return true;
        }

        private bool ScarecrowLogic()
        {
            return Duel.Player == 1 && Duel.Phase == DuelPhase.Battle;
        }

        private bool TrapSetLogic()
        {
            if (Bot.GetSpellCountWithoutField() >= 4) return false;
            // Evita la zona central para mayor seguridad contra efectos de columna
            AI.SelectPlace(Zones.z0 | Zones.z1 | Zones.z3 | Zones.z4);
            return true;
        }

        // Se eliminó OnSelectCard para evitar el error CS0115.
        // La lógica de "no tributar ases" ahora se maneja automáticamente 
        // por la prioridad del DefaultExecutor al no añadirlos como materiales 
        // preferentes en los otros métodos.

        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            // Si el atacante es débil y hay muchas cartas seteadas, ser precavido
            if (Enemy.GetSpellCountWithoutField() >= 2 && attacker.Attack < 2000)
                return null;
            
            return base.OnSelectAttackTarget(attacker, defenders);
        }
    }
}
