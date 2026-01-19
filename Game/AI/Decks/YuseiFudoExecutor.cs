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

            // Magias
            public const int Tuning = 96363153; // ID proporcionado por usuario
            public const int Tuning_Alt = 96363153;
            public const int SynchroChase = 23442438;
            public const int OneForOne = 02295440;
            public const int OneForOne_Alt = 02295440;
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
            // --- PROTOCOLOS DE ACTIVACIÓN (ORDE PRIORITARIO) ---
            AddExecutor(ExecutorType.Activate, CardId.Tuning, TuningLogic);
            AddExecutor(ExecutorType.Activate, CardId.Tuning_Alt, TuningLogic);
            AddExecutor(ExecutorType.Activate, CardId.SynchroChase, () => Bot.Hand.Any(c => c.HasType(CardType.Tuner)));
            AddExecutor(ExecutorType.Activate, CardId.OneForOne, OneForOneLogic);
            AddExecutor(ExecutorType.Activate, CardId.OneForOne_Alt, OneForOneLogic);
            AddExecutor(ExecutorType.Activate, CardId.MonsterReborn, MonsterRebornLogic);
            AddExecutor(ExecutorType.Activate, CardId.LevelLifter, LevelLifterLogic);

            // --- REACCIONES DE TRAMPAS ---
            AddExecutor(ExecutorType.Activate, CardId.StarlightRoad, StarlightRoadLogic);
            AddExecutor(ExecutorType.Activate, CardId.TimeMachine, TimeMachineLogic);
            AddExecutor(ExecutorType.Activate, CardId.Waboku, WabokuLogic);
            AddExecutor(ExecutorType.Activate, CardId.ShootingStar, ShootingStarLogic);
            AddExecutor(ExecutorType.Activate, CardId.SynchroTransmission, SynchroTransmissionLogic);
            AddExecutor(ExecutorType.Activate, CardId.PhysicalDouble, PhysicalDoubleLogic);
            AddExecutor(ExecutorType.Activate, CardId.BoneTempleBlock, BoneTempleBlockLogic);
            AddExecutor(ExecutorType.Activate, CardId.DeepDarkTrapHole, DeepDarkTrapHoleLogic);
            AddExecutor(ExecutorType.Activate, CardId.ScrapIronScarecrow, () => Duel.Phase == DuelPhase.Battle);
            AddExecutor(ExecutorType.Activate, CardId.SynchroFellowship, SynchroFellowshipLogic);

            // --- INVOCACIONES ESPECIALES Y CEBOS ---
            AddExecutor(ExecutorType.SpSummon, CardId.QuickdrawSynchron, QuickdrawLogic);
            AddExecutor(ExecutorType.SpSummon, CardId.UnknownSynchron, () => Bot.GetMonsterCount() == 0);
            AddExecutor(ExecutorType.SpSummon, CardId.QuillboltHedgehog, () => HasTunerInField());
            AddExecutor(ExecutorType.SpSummon, CardId.QuillboltHedgehog2, () => HasTunerInField());
            AddExecutor(ExecutorType.Activate, CardId.JetSynchron, JetSynchronGraveLogic);

            // --- EXTRA DECK (CADENA DE SINCRONÍA) ---
            AddExecutor(ExecutorType.SpSummon, CardId.ShootingQuasarDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.ShootingStarDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.TGHyperLibrarian, LibrarianComboLogic);
            AddExecutor(ExecutorType.SpSummon, CardId.StardustDragon, StardustSummonLogic);
            AddExecutor(ExecutorType.SpSummon, CardId.JunkWarrior);
            AddExecutor(ExecutorType.SpSummon, CardId.ArcherSynchron);
            AddExecutor(ExecutorType.SpSummon, CardId.FormulaSynchron);

            // --- INVOCACIÓN NORMAL Y DEFENSA PASIVA ---
            AddExecutor(ExecutorType.Summon, CardId.SynchronExplorer, SynchronExplorerLogic);
            AddExecutor(ExecutorType.Summon, CardId.JunkSynchron, JunkSynchronNormalLogic);
            AddExecutor(ExecutorType.Summon, CardId.Doppelwarrior, () => Bot.HasInMonstersZone(CardId.JunkSynchron));
            
            // REGLA DEFENSA PASIVA: Colocar si no hay Tuners
            AddExecutor(ExecutorType.MonsterSet, PassiveDefenseLogic);
            
            // Restricciones "Qué NO Hacer"
            AddExecutor(ExecutorType.Summon, CardId.EffectVeiler, () => false);
            AddExecutor(ExecutorType.Summon, CardId.EffectVeiler2, () => false);
            AddExecutor(ExecutorType.Summon, CardId.MajesticDragon, () => Bot.HasInMonstersZone(CardId.StardustDragon));
            AddExecutor(ExecutorType.Summon, CardId.BrightStarDragon, () => HasTunerInField());

            // --- RECICLAJE ---
            AddExecutor(ExecutorType.Activate, CardId.RushWarrior, () => Bot.Hand.Count == 0 && Bot.HasInGraveyard(CardId.JunkSynchron));
            AddExecutor(ExecutorType.Activate, CardId.PotOfAvarice, PotOfAvariceLogic);
            AddExecutor(ExecutorType.Activate, CardId.EffectVeiler, DefaultEffectVeiler);
            AddExecutor(ExecutorType.Activate, CardId.EffectVeiler2, DefaultEffectVeiler);

            AddExecutor(ExecutorType.Repos, MonsterReposLogic);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        // --- LÓGICA DE MAGIAS ---

        private bool TuningLogic()
        {
            // Prioridad: Activar primero para ver qué cae al cementerio
            if (Card.Location == CardLocation.Hand)
            {
                AI.SelectCard(CardId.JunkSynchron);
                return true;
            }
            return false;
        }

        private bool OneForOneLogic()
        {
            ClientCard cost = Bot.Hand.FirstOrDefault(c => c.IsCode(CardId.NecroDefender, CardId.RushWarrior, CardId.JetSynchron, CardId.QuillboltHedgehog));
            if (cost == null) cost = Bot.Hand.FirstOrDefault(c => c.IsMonster());
            if (cost != null)
            {
                AI.SelectCard(cost);
                AI.SelectNextCard(CardId.JetSynchron);
                return true;
            }
            return false;
        }

        private bool MonsterRebornLogic()
        {
            if (Bot.HasInGraveyard(CardId.JunkSynchron)) { AI.SelectCard(CardId.JunkSynchron); return true; }
            return false;
        }

        private bool LevelLifterLogic()
        {
            if (Bot.Hand.Any(c => c.IsCode(CardId.QuickdrawSynchron)) && Bot.MonsterZone.Any(c => c != null && c.HasType(CardType.Token)))
            {
                AI.SelectCard(CardId.QuickdrawSynchron); // Descartar nivel 5
                return true; // Convertir Token a nivel 5
            }
            return false;
        }

        // --- LÓGICA DE TRAMPAS ---

        private bool TimeMachineLogic()
        {
            if (Duel.Phase != DuelPhase.Battle) return false;
            // No usar en turno oponente excepto si el atacante es 0 ATK
            if (Duel.Player == 1)
            {
                ClientCard attacker = Enemy.BattlingMonster;
                return attacker != null && attacker.Attack == 0;
            }
            return true;
        }

        private bool WabokuLogic()
        {
            // Puramente defensivo para proteger materiales
            bool hasMaterials = Bot.MonsterZone.Any(c => c != null && (c.Level <= 3 || c.HasType(CardType.Token)));
            return Duel.Player == 1 && Duel.Phase == DuelPhase.Battle && hasMaterials;
        }

        private bool ShootingStarLogic()
        {
            return Bot.MonsterZone.Any(c => c != null && c.IsCode(CardId.StardustDragon));
        }

        private bool SynchroTransmissionLogic()
        {
            // Sincronía sorpresa en turno del oponente
            return Duel.Player == 1 && (Duel.Phase == DuelPhase.Main1 || Duel.Phase == DuelPhase.Main2);
        }

        private bool PhysicalDoubleLogic()
        {
            if (Duel.Player == 1 && Bot.GetMonsterCount() < 5)
            {
                ClientCard attacker = Enemy.BattlingMonster;
                return attacker != null && attacker.Attack >= 2000;
            }
            return false;
        }

        private bool BoneTempleBlockLogic()
        {
            bool hasDiscard = Bot.Hand.Any(c => c.IsCode(CardId.QuillboltHedgehog, CardId.QuillboltHedgehog2));
            bool enemyHasLevel4 = Enemy.Graveyard.Any(c => c.Level <= 4) || Enemy.MonsterZone.Any(c => c != null && c.Level <= 4);
            return hasDiscard && enemyHasLevel4;
        }

        private bool DeepDarkTrapHoleLogic()
        {
            foreach (ClientCard card in Duel.SummoningCards)
            {
                if (card.Level >= 5 && card.HasType(CardType.Effect)) return true;
            }
            return false;
        }

        private bool StarlightRoadLogic()
        {
            return Duel.ChainTargets.Count >= 2;
        }

        private bool SynchroFellowshipLogic()
        {
            return Bot.Graveyard.Count(c => c.HasSetcode(0x1017)) >= 2; // 0x1017 = Synchron
        }

        // --- LÓGICA DE MONSTRUOS ---

        private bool QuickdrawLogic()
        {
            // Cebo: Descartar útil para guardar Invocación Normal
            ClientCard discard = Bot.Hand.FirstOrDefault(c => c.IsCode(CardId.JetSynchron, CardId.QuillboltHedgehog, CardId.NecroDefender));
            if (discard != null)
            {
                AI.SelectCard(discard);
                AI.SelectPosition(CardPosition.FaceUpDefence);
                return true;
            }
            return false;
        }

        private bool LibrarianComboLogic()
        {
            // Gestión de Espacio: Necesita 2 zonas para tokens de Doppelwarrior
            return Bot.GetMonsterCount() <= 3;
        }

        private bool StardustSummonLogic()
        {
            // Protección: Priorizar Clear Effector como material
            if (Bot.HasInMonstersZone(CardId.ClearEffector))
            {
                AI.SelectMaterials(CardId.ClearEffector);
            }
            return true;
        }

        private bool JunkSynchronNormalLogic()
        {
            return Bot.Graveyard.Any(c => c.Level <= 2);
        }

        private bool SynchronExplorerLogic()
        {
            return Bot.HasInGraveyard(CardId.JunkSynchron);
        }

        private bool JetSynchronGraveLogic()
        {
            // No usar si se va a desterrar sin plan de sincronía
            return HasTunerInField() && Bot.Hand.Count > 0;
        }

        private bool PotOfAvariceLogic()
        {
            // Reciclar Extra Deck, pero no quitar materiales de Junk Synchron
            if (Bot.Graveyard.Count(c => c.Level <= 2) < 2) return false;
            return Bot.Graveyard.Count >= 5;
        }

        private bool PassiveDefenseLogic()
        {
            // SI No-Tuner y NO hay Tuners disponibles -> Colocar boca abajo
            bool hasTuner = Bot.Hand.Concat(Bot.MonsterZone.Where(c => c != null)).Any(c => c.HasType(CardType.Tuner));
            if (!hasTuner && !Card.HasType(CardType.Tuner))
            {
                AI.SelectPosition(CardPosition.FaceDownDefence);
                return true;
            }
            return false;
        }

        private bool MonsterReposLogic()
        {
            if (Card.IsFacedown())
            {
                bool hasTuner = Bot.Hand.Concat(Bot.MonsterZone.Where(c => c != null)).Any(c => c.HasType(CardType.Tuner));
                return Enemy.GetMonsterCount() == 0 || hasTuner;
            }
            return DefaultMonsterRepos();
        }

        private bool HasTunerInField() => Bot.MonsterZone.Any(c => c != null && c.HasType(CardType.Tuner));
        
        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            // Lógica Junk Warrior: Solo atacar si puede superar al enemigo
            if (attacker.IsCode(CardId.JunkWarrior))
            {
                ClientCard bestEnemy = Enemy.MonsterZone.GetHighestAttackMonster();
                if (bestEnemy != null && attacker.Attack > bestEnemy.Attack) return AI.Attack(attacker, bestEnemy);
            }
            return base.OnSelectAttackTarget(attacker, defenders);
        }
    }
}
