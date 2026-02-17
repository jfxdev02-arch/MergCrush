using UnityEngine;
using System.Collections.Generic;
using MergCrush.Core;

namespace MergCrush.Level
{
    /// <summary>
    /// Gerencia o sistema de pontuacao do jogo
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private Configuration config;
        
        [Header("Current Game State")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int comboCount = 0;
        [SerializeField] private float comboTimer = 0f;
        [SerializeField] private int totalMerges = 0;
        [SerializeField] private int highestItemLevel = 1;
        
        [Header("Combo Settings")]
        [SerializeField] private float comboTimeWindow = 2f;
        [SerializeField] private float comboMultiplierIncrement = 0.5f;
        [SerializeField] private int maxComboMultiplier = 10;
        
        // Estatisticas da sessao
        private Dictionary<int, int> mergesByLevel = new Dictionary<int, int>();
        private int totalCubesSpawned = 0;
        
        // Eventos
        public System.Action<int> OnScoreChanged;
        public System.Action<int, int> OnMergeScored; // points, itemLevel
        public System.Action<int> OnComboIncreased; // comboCount
        public System.Action OnComboEnded;
        
        public int CurrentScore => currentScore;
        public int ComboCount => comboCount;
        public int TotalMerges => totalMerges;
        public int HighestItemLevel => highestItemLevel;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            if (config == null)
            {
                config = Resources.Load<Configuration>("GameConfiguration");
            }
        }
        
        private void Update()
        {
            // Atualizar timer do combo
            if (comboCount > 0)
            {
                comboTimer -= Time.deltaTime;
                
                if (comboTimer <= 0)
                {
                    EndCombo();
                }
            }
        }
        
        /// <summary>
        /// Adiciona pontos por uma fusao
        /// </summary>
        public void AddMergeScore(Cube cube)
        {
            if (cube == null) return;
            
            int itemLevel = cube.ItemLevel;
            int itemValue = cube.ItemValue;
            
            // Calcular pontos base
            int basePoints = config != null ? config.baseMergePoints : 10;
            float multiplier = config != null ? config.pointMultiplier : 1.5f;
            
            // Pontos = base * valor do item
            int points = Mathf.RoundToInt(basePoints * itemValue * multiplier);
            
            // Aplicar multiplicador de combo
            float comboMultiplier = 1f + (comboCount * comboMultiplierIncrement);
            comboMultiplier = Mathf.Min(comboMultiplier, maxComboMultiplier);
            
            points = Mathf.RoundToInt(points * comboMultiplier);
            
            // Incrementar combo
            IncreaseCombo();
            
            // Adicionar ao score
            currentScore += points;
            totalMerges++;
            
            // Atualizar estatisticas
            if (itemLevel > highestItemLevel)
            {
                highestItemLevel = itemLevel;
            }
            
            if (!mergesByLevel.ContainsKey(itemLevel))
            {
                mergesByLevel[itemLevel] = 0;
            }
            mergesByLevel[itemLevel]++;
            
            // Notificar LevelManager
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.AddScore(points);
            }
            
            // Disparar eventos
            OnScoreChanged?.Invoke(currentScore);
            OnMergeScored?.Invoke(points, itemLevel);
            
            Debug.Log($"Fusao! Nivel: {itemLevel}, Valor: {itemValue}, Pontos: {points}, Combo: x{comboMultiplier:F1}");
        }
        
        /// <summary>
        /// Adiciona pontos diretamente
        /// </summary>
        public void AddScore(int points)
        {
            currentScore += points;
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.AddScore(points);
            }
            
            OnScoreChanged?.Invoke(currentScore);
        }
        
        /// <summary>
        /// Incrementa o contador de combo
        /// </summary>
        private void IncreaseCombo()
        {
            comboCount++;
            comboTimer = comboTimeWindow;
            
            OnComboIncreased?.Invoke(comboCount);
        }
        
        /// <summary>
        /// Finaliza o combo atual
        /// </summary>
        private void EndCombo()
        {
            if (comboCount > 1)
            {
                Debug.Log($"Combo finalizado: {comboCount} fusoes!");
            }
            
            comboCount = 0;
            comboTimer = 0f;
            
            OnComboEnded?.Invoke();
        }
        
        /// <summary>
        /// Retorna o multiplicador de combo atual
        /// </summary>
        public float GetComboMultiplier()
        {
            if (comboCount <= 0) return 1f;
            
            float multiplier = 1f + (comboCount * comboMultiplierIncrement);
            return Mathf.Min(multiplier, maxComboMultiplier);
        }
        
        /// <summary>
        /// Registra um cubo spawnado
        /// </summary>
        public void RegisterCubeSpawned()
        {
            totalCubesSpawned++;
        }
        
        /// <summary>
        /// Reseta o score para uma nova partida
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            comboCount = 0;
            comboTimer = 0f;
            totalMerges = 0;
            highestItemLevel = 1;
            totalCubesSpawned = 0;
            mergesByLevel.Clear();
            
            OnScoreChanged?.Invoke(currentScore);
        }
        
        /// <summary>
        /// Obtem estatisticas da partida
        /// </summary>
        public GameStatistics GetStatistics()
        {
            return new GameStatistics
            {
                finalScore = currentScore,
                totalMerges = totalMerges,
                highestItemLevel = highestItemLevel,
                maxCombo = comboCount,
                totalCubesSpawned = totalCubesSpawned,
                mergesByLevel = new Dictionary<int, int>(mergesByLevel)
            };
        }
        
        /// <summary>
        /// Calcula pontos bonus por completar o nivel
        /// </summary>
        public int CalculateBonusScore()
        {
            int bonus = 0;
            
            // Bonus por item mais alto
            bonus += highestItemLevel * 100;
            
            // Bonus por combos
            bonus += totalMerges * 10;
            
            return bonus;
        }
        
        /// <summary>
        /// Define o score atual (para carregar jogos salvos)
        /// </summary>
        public void SetCurrentScore(int score)
        {
            currentScore = score;
            OnScoreChanged?.Invoke(currentScore);
        }
        
        /// <summary>
        /// Configura o ScoreManager
        /// </summary>
        public void SetConfig(Configuration newConfig)
        {
            config = newConfig;
        }
    }
    
    /// <summary>
    /// Estatisticas de uma partida
    /// </summary>
    [System.Serializable]
    public class GameStatistics
    {
        public int finalScore;
        public int totalMerges;
        public int highestItemLevel;
        public int maxCombo;
        public int totalCubesSpawned;
        public Dictionary<int, int> mergesByLevel;
        
        public override string ToString()
        {
            return $"Score: {finalScore}, Merges: {totalMerges}, Max Level: {highestItemLevel}, Combo: {maxCombo}";
        }
    }
}
