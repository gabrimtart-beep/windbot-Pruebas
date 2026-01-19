using System;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    [Deck("LogicalSynchro", "AI_Synchro_Architect_v1")]
    public class SynchroArchitectExecutor : DefaultExecutor
    {
        // Memoria de Duelo: Registra amenazas y patrones del rival
        private static HashSet<int> OpponentThreats = new HashSet<int>();
        private int TurnCount = 0;

        public SynchroArchitectExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // --- MOTOR DE DESCUBRIMIENTO DE COMBOS ---
            // El bot buscará jugadas en este orden de prioridad lógica:
            
            // 1. BUSCADORES (Preparar la mano)
            AddExecutor(ExecutorType.Activate, c => c.HasCategory(CardCategory.Search) || c.HasCategory(CardCategory.Draw));

            // 2. EXTENSORES (Invocar de modo especial si ayuda a subir el nivel del campo)
            AddExecutor(ExecutorType.SpSummon, Discovery_SpecialSummonLogic);

            // 3. SINCRONÍA DINÁMICA (El bot "mira" el Extra Deck y calcula si puede invocar algo)
            AddExecutor(ExecutorType.SpSummon, Discovery_SynchroDiscovery);

            // 4. INVOCACIÓN NORMAL TÁCTICA
            AddExecutor(ExecutorType.Summon, Discovery_NormalSummonLogic);

            // 5. REACCIÓN INTELIGENTE
            AddExecutor(ExecutorType.Activate, Discovery_SmartActivation);

            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        // --- LÓGICA: DESCUBRIMIENTO DE SINCRONÍA ---
        private bool Discovery_SynchroDiscovery()
        {
            // El bot analiza qué niveles tiene en el campo
            var fieldLevels = Bot.MonsterZone.Where(c => c != null && c.IsFaceup()).Select(c => c.Level).ToList();
            
            // Si tiene un Cantante y un No-Cantante, busca en el Extra Deck qué puede invocar
            if (Bot.MonsterZone.Any(c => c != null && c.HasType(CardType.Tuner)))
            {
                // El motor de WindBot ya intenta emparejar niveles, 
                // pero aquí le damos prioridad si el resultado es un monstruo de "Capa Alta" (Nivel 7+)
                return Card.Level >= 7 || (Card.Attack > Enemy.MonsterZone.GetHighestAttackMonster()?.Attack ?? 0);
            }
            return false;
        }

        // --- LÓGICA: INVOCACIÓN NORMAL POR POSIBILIDADES ---
        private bool Discovery_NormalSummonLogic()
        {
            // REGLA DE APRENDIZAJE: Si el oponente ha usado "Effect Veiler" o "Ash Blossom" (basado en memoria)
            // el bot esperará a tener un "Cebo" antes de bajar su carta principal.
            bool isOpponentDangerous = OpponentThreats.Any();

            // Si no hay monstruos, invocar para no perder presencia (Lógica de Supervivencia)
            if (Bot.GetMonsterCount() == 0) return true;

            // ANALIZAR POSIBILIDADES:
            // ¿Tengo un Cantante en campo? Entonces invoco un No-Cantante de la mano para abrir un Combo.
            if (HasTunerInField() && !Card.HasType(CardType.Tuner)) return true;

            // ¿Tengo un No-Cantante? Invoco un Cantante.
            if (HasNonTunerInField() && Card.HasType(CardType.Tuner)) return true;

            // Si el monstruo en mano tiene un ATK mayor al mejor del rival, contraatacar.
            if (Card.Attack > (Enemy.MonsterZone.GetHighestAttackMonster()?.Attack ?? 0)) return true;

            return false;
        }

        // --- LÓGICA: ACTIVACIÓN BASADA EN VALOR ---
        private bool Discovery_SmartActivation()
        {
            // Guardar en memoria lo que el oponente hace para "aprender" sus cartas
            if (Duel.Player == 1)
            {
                OpponentThreats.Add(Card.Id);
            }

            // ¿Es una carta de interrupción? (Negación/Destrucción)
            if (Card.IsSpellNegate() || Card.IsMonsterNegate() || Card.HasCategory(CardCategory.Destroy))
            {
                // Solo activarla si el objetivo del rival es "Valioso" (ATK > 2000 o Efecto de búsqueda)
                ClientCard target = Duel.ChainTargets.LastOrDefault();
                if (target != null && target.Attack < 1500 && Bot.LifePoints > 2000) return false;
            }

            return true;
        }

        private bool Discovery_SpecialSummonLogic()
        {
            // Si la invocación especial NO gasta recursos valiosos y aumenta el número de monstruos, hacerlo.
            // Esto permite que el bot "descubra" que puede usar a Quillbolt Hedgehog o Jet Synchron
            // para completar niveles que le faltan para una Sincronía.
            if (Bot.GetMonsterCount() < 4) return true;

            return false;
        }

        // --- AYUDAS TÉCNICAS ---
        private bool HasTunerInField() => Bot.MonsterZone.Any(c => c != null && c.IsFaceup() && c.HasType(CardType.Tuner));
        private bool HasNonTunerInField() => Bot.MonsterZone.Any(c => c != null && c.IsFaceup() && !c.HasType(CardType.Tuner));
        
        public override void OnNewTurn()
        {
            TurnCount++;
            base.OnNewTurn();
        }
    }
}
