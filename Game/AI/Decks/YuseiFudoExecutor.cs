using YGOSharp.OCGWrapper.Enums;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("YuseiFudo", "AI_Yusei")] // "YuseiFudo" debe ser el nombre de tu archivo .ydk
    public class YuseiFudoExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int Tuning = 63977008;
            public const int EffectVeiler = 97268402;
            public const int StardustDragon = 44508094;
            public const int JunkSynchron = 63977008; // Ejemplo
        }

        public YuseiFudoExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // 1. REGLAS DE ACTIVACIÓN (Magias y efectos)
            AddProcessor(CardId.Tuning, DefaultSpellActivate);
            AddProcessor(CardId.EffectVeiler, DefaultTrapActivate); // Se usa en el turno del oponente

            // 2. REGLAS DE INVOCACIÓN (Monstruos)
            AddProcessor(CardId.StardustDragon, DefaultSynchroSummon);
            
            // 3. REGLA POR DEFECTO: Invocar el monstruo más fuerte que pueda
            AddProcessor(ExecutorType.Summon, DefaultSummon);
            AddProcessor(ExecutorType.SpSummon, DefaultSpSummon);
            
            // 4. REGLA DE ATAQUE: Atacar con todo
            AddProcessor(ExecutorType.MonsterExtractSummon, DefaultMonsterExtractSummon);
        }
    }
}