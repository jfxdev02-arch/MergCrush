using UnityEngine;

namespace MergCrush.Level
{
    /// <summary>
    /// Dados de configuracao de um nivel especifico
    /// Usado para configuracoes avancadas por nivel
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "MergCrush/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Numero do nivel")]
        public int levelNumber = 1;
        
        [Tooltip("Nome do nivel")]
        public string levelName = "Level 1";
        
        [Tooltip("Descricao do nivel")]
        [TextArea(2, 4)]
        public string description = "";
        
        [Header("Gameplay Settings")]
        [Tooltip("Tema associado a este nivel")]
        public Theme.ThemeData theme;
        
        [Tooltip("Pontuacao alvo para completar")]
        public int targetScore = 1000;
        
        [Tooltip("Dificuldade do nivel (1-5)")]
        [Range(1, 5)]
        public int difficulty = 1;
        
        [Tooltip("Numero de cubos iniciais")]
        public int initialCubes = 5;
        
        [Tooltip("Intervalo de spawn (segundos)")]
        public float spawnInterval = 2f;
        
        [Tooltip("Maximo de itens diferentes que podem spawnar")]
        [Range(1, 6)]
        public int maxSpawnItemLevel = 3;
        
        [Header("Grid Settings Override")]
        [Tooltip("Usar configuracoes de grid customizadas")]
        public bool useCustomGrid = false;
        
        [Tooltip("Largura customizada da grid")]
        public int customGridWidth = 9;
        
        [Tooltip("Altura customizada da grid")]
        public int customGridHeight = 9;
        
        [Header("Stars")]
        [Tooltip("Porcentagem para 1 estrela")]
        public int star1Threshold = 100;
        
        [Tooltip("Porcentagem para 2 estrelas")]
        public int star2Threshold = 150;
        
        [Tooltip("Porcentagem para 3 estrelas")]
        public int star3Threshold = 200;
        
        [Header("Time Limit")]
        [Tooltip("Usar limite de tempo")]
        public bool hasTimeLimit = false;
        
        [Tooltip("Tempo limite em segundos")]
        public int timeLimitSeconds = 120;
        
        [Header("Moves Limit")]
        [Tooltip("Usar limite de movimentos")]
        public bool hasMoveLimit = false;
        
        [Tooltip("Numero maximo de movimentos")]
        public int maxMoves = 50;
        
        [Header("Special Rules")]
        [Tooltip("Permite combos em cascata")]
        public bool allowCascadeCombo = true;
        
        [Tooltip("Multiplicador de pontos por combo")]
        public float comboMultiplier = 1.5f;
        
        [Tooltip("Bonus por completar rapidamente")]
        public int timeBonus = 100;
        
        /// <summary>
        /// Calcula quantas estrelas o jogador ganhou
        /// </summary>
        public int CalculateStars(int score)
        {
            float percentage = (float)score / targetScore * 100f;
            
            if (percentage >= star3Threshold) return 3;
            if (percentage >= star2Threshold) return 2;
            if (percentage >= star1Threshold) return 1;
            
            return 0;
        }
        
        /// <summary>
        /// Verifica se os dados sao validos
        /// </summary>
        public bool IsValid()
        {
            if (theme == null)
            {
                Debug.LogWarning($"LevelData {levelName}: Nenhum tema associado!");
                return false;
            }
            
            return true;
        }
    }
}
