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
            // Añade aquí más IDs si lo necesitas
        }

        public YuseiFudoExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Siguiendo el ejemplo de ABC/Dragun:
            AddExecutor(ExecutorType.Activate, CardId.Tuning);
            AddExecutor(ExecutorType.SpSummon, CardId.StardustDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.JunkWarrior);
            
            AddExecutor(ExecutorType.Activate, DefaultDontChainMyself);
            AddExecutor(ExecutorType.Summon, CardId.JunkSynchron);
            AddExecutor(ExecutorType.Summon);
            AddExecutor(ExecutorType.SpSummon);

            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        // Basado en los ejemplos, la lógica de batalla por defecto
        public override bool OnPreBattleBetween(ClientCard attacker, ClientCard defender)
        {
            return base.OnPreBattleBetween(attacker, defender);
        }
    }
}
