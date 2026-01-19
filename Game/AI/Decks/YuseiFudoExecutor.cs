using System;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    [Deck("YuseiFudo", "AI_Yusei_Fixed_Compile")]
    public class YuseiFudoExecutor : DefaultExecutor
    {
        public class CardId
        {
            // Cantantes (Tuners)
            public const int JunkSynchron = 63977008;
            public const int QuickdrawSynchron = 20932152;
            public const int UnknownSynchron = 15310033;
            public const int JetSynchron = 9742784;
            public const int MajesticDragon = 21159309;
            public const int EffectVeiler = 97268402;
            public const int EffectVeiler2 = 97268403;
            public const int FormulaSynchron = 50091196;

            // No-Cantantes (Materiales y Efectos)
            public const int Doppelwarrior = 53855409;
            public const int QuillboltHedgehog = 23571046;
            public const int QuillboltHedgehog2 = 23571046;
            public const int ClearEffector = 58518520;
            public const int RushWarrior = 36736723;
            public const int NecroDefender = 77700347;
            public const int SynchronExplorer = 36643046;
            public const int BrightStarDragon = 16719802;
            public const int JunkAnchor = 25148255;
            public const int ShieldWing = 17201951; 

            // Magias
            public const int Tuning = 96363153; 
            public const int SynchroChase = 23442438;
            public const int OneForOne = 02295440;
            public const int PotOfAvarice = 67169062;
            public const int MonsterReborn = 83764718;
            public const int LevelLifter = 37198732;

            // Trampas
            public const int TimeMachine = 80987696;
            public const int Waboku = 12607053;
            public const int ShootingStar = 47264717;
            public const int SynchroTransmission = 35817848; 
            public const int PhysicalDouble = 63442604;
            public const int BoneTempleBlock = 47778083;
            public const int DeepDarkTrapHole = 28654932;
            public const int StarlightRoad = 58120309;
            public const int ScrapIronScarecrow = 98427577;
            public const int SynchroFellowship = 43834302;

            // Extra Deck
            public const int ShootingQuasarDragon = 35952884;
            public const int ShootingStarDragon = 24696097;
            public const int TGHyperLibrarian = 90953320;
            public const int StardustDragon = 44508094;
            public const int JunkWarrior = 60800381;
            public const int ArcherSynchron = 42810973;
            public const int JetWarrior = 00286392;
        }

        public YuseiFudoExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            AddExecutor(ExecutorType.Activate, CardId.Tuning);
            AddExecutor(ExecutorType.Activate, CardId.OneForOne, OneForOneLogic);
            AddExecutor(ExecutorType.Activate, CardId.MonsterReborn, MonsterRebornLogic);
            AddExecutor(ExecutorType.Activate, CardId.SynchroChase);

            AddExecutor(ExecutorType.SpSummon, CardId.QuickdrawSynchron, QuickdrawLogic);
            AddExecutor(ExecutorType.SpSummon, CardId.UnknownSynchron);
            AddExecutor(ExecutorType.SpSummon, CardId.QuillboltHedgehog);
            AddExecutor(ExecutorType.Activate, CardId.JetSynchron, () => Bot.Hand.Count > 0);

            AddExecutor(ExecutorType.SpSummon, CardId.TGHyperLibrarian, () => Bot.GetMonsterCount() <= 3);
            AddExecutor(ExecutorType.SpSummon, CardId.ShootingQuasarDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.ShootingStarDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.StardustDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.JetWarrior);
            AddExecutor(ExecutorType.SpSummon, CardId.JunkWarrior);
            AddExecutor(ExecutorType.SpSummon, CardId.FormulaSynchron);

            AddExecutor(ExecutorType.Summon, CardId.JunkSynchron, () => Bot.Graveyard.Any(c => c.Level <= 2));
            AddExecutor(ExecutorType.Summon, () => HasTunerInField() && Bot.Hand.Any(c => !c.HasType(CardType.Tuner)));
            AddExecutor(ExecutorType.Summon, () => HasNonTunerInField() && Bot.Hand.Any(c => c.HasType(CardType.Tuner)));

            AddExecutor(ExecutorType.Summon, CardId.ShieldWing, () => Bot.GetMonsterCount() == 0);
            AddExecutor(ExecutorType.Summon, () => Bot.GetMonsterCount() == 0);
            AddExecutor(ExecutorType.MonsterSet, () => Bot.GetMonsterCount() == 0);

            AddExecutor(ExecutorType.Activate, CardId.StarlightRoad, () => Duel.ChainTargets.Count >= 2);
            AddExecutor(ExecutorType.Activate, CardId.Waboku, () => Duel.Player == 1 && Duel.Phase == DuelPhase.Battle);
            AddExecutor(ExecutorType.Activate, CardId.ScrapIronScarecrow, () => Duel.Phase == DuelPhase.Battle);
            AddExecutor(ExecutorType.Activate, CardId.DeepDarkTrapHole);
            
            AddExecutor(ExecutorType.Activate, CardId.PotOfAvarice, () => Bot.Graveyard.Count >= 5);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool QuickdrawLogic()
        {
            ClientCard discard = Bot.Hand.FirstOrDefault(c => c.IsCode(CardId.QuillboltHedgehog, CardId.JetSynchron, CardId.Doppelwarrior, CardId.NecroDefender));
            if (discard != null) { AI.SelectCard(discard); return true; }
            return Bot.GetMonsterCount() == 0;
        }

        private bool OneForOneLogic()
        {
            ClientCard cost = Bot.Hand.FirstOrDefault(c => c.IsMonster());
            if (cost != null) { AI.SelectCard(cost); AI.SelectNextCard(CardId.JetSynchron); return true; }
            return false;
        }

        private bool MonsterRebornLogic()
        {
            if (Bot.HasInGraveyard(CardId.JunkSynchron)) { AI.SelectCard(CardId.JunkSynchron); return true; }
            return false;
        }

        // --- CORRECCIÓN CRÍTICA DE COMPILACIÓN ---
        private bool HasTunerInField() => Bot.MonsterZone.Any(c => c != null && c.IsFaceup() && c.HasType(CardType.Tuner));
        private bool HasNonTunerInField() => Bot.MonsterZone.Any(c => c != null && c.IsFaceup() && !c.HasType(CardType.Tuner));
    }
}
