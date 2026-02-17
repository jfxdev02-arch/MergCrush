using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using MergCrush.Theme;
using MergCrush.Core;

namespace MergCrush.Level
{
    /// <summary>
    /// Gerencia a progressao de fases do jogo
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        
        [Header("Level Configuration")]
        [SerializeField] private ThemeData[] themes;
        [SerializeField] private int startingLevel = 0;
        
        [Header("Current State")]
        [SerializeField] private int currentLevel = 0;
        [SerializeField] private int unlockedLevel = 0;
        [SerializeField] private bool isPlaying = false;
        
        // Keys para PlayerPrefs
        private const string UNLOCKED_LEVEL_KEY = "MergCrush_UnlockedLevel";
        private const string LEVEL_SCORE_PREFIX = "MergCrush_LevelScore_";
        private const string LEVEL_STARS_PREFIX = "MergCrush_LevelStars_";
        
        // Estado do nivel atual
        private int currentScore = 0;
        private bool levelCompleted = false;
        
        // Eventos
        public System.Action<int> OnLevelLoaded;
        public System.Action<int, int> OnLevelCompleted; // levelIndex, stars
        public System.Action<int> OnLevelUnlocked;
        public System.Action<int> OnScoreChanged;
        
        public int CurrentLevel => currentLevel;
        public int UnlockedLevel => unlockedLevel;
        public int TotalLevels => themes != null ? themes.Length : 0;
        public bool IsPlaying => isPlaying;
        public int CurrentScore => currentScore;
        public ThemeData CurrentTheme => GetTheme(currentLevel);
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            LoadProgress();
        }
        
        private void Start()
        {
            // Se nao ha temas configurados, tentar carregar da Configuration
            if (themes == null || themes.Length == 0)
            {
                Configuration config = Resources.Load<Configuration>("GameConfiguration");
                if (config != null && config.themes != null)
                {
                    themes = config.themes;
                }
            }
        }
        
        /// <summary>
        /// Carrega o progresso salvo
        /// </summary>
        private void LoadProgress()
        {
            unlockedLevel = PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 0);
            
            // Garantir que pelo menos o primeiro nivel esteja desbloqueado
            if (unlockedLevel < 0) unlockedLevel = 0;
        }
        
        /// <summary>
        /// Salva o progresso
        /// </summary>
        private void SaveProgress()
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, unlockedLevel);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Carrega um nivel especifico
        /// </summary>
        public void LoadLevel(int levelIndex)
        {
            if (!IsLevelUnlocked(levelIndex))
            {
                Debug.LogWarning($"Nivel {levelIndex} esta bloqueado!");
                return;
            }
            
            if (levelIndex < 0 || levelIndex >= TotalLevels)
            {
                Debug.LogError($"Indice de nivel invalido: {levelIndex}");
                return;
            }
            
            currentLevel = levelIndex;
            currentScore = 0;
            levelCompleted = false;
            isPlaying = true;
            
            // Aplicar tema
            ThemeData theme = GetTheme(levelIndex);
            if (ThemeManager.Instance != null && theme != null)
            {
                ThemeManager.Instance.ApplyTheme(theme, levelIndex);
            }
            
            // Carregar cena de jogo se necessario
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                SceneManager.LoadScene("GameScene");
            }
            
            OnLevelLoaded?.Invoke(levelIndex);
            
            Debug.Log($"Nivel {levelIndex} carregado: {theme?.themeName}");
        }
        
        /// <summary>
        /// Carrega o proximo nivel
        /// </summary>
        public void LoadNextLevel()
        {
            LoadLevel(currentLevel + 1);
        }
        
        /// <summary>
        /// Recarrega o nivel atual
        /// </summary>
        public void ReloadLevel()
        {
            LoadLevel(currentLevel);
        }
        
        /// <summary>
        /// Completa o nivel atual
        /// </summary>
        public void CompleteLevel()
        {
            if (levelCompleted) return;
            
            levelCompleted = true;
            isPlaying = false;
            
            ThemeData theme = CurrentTheme;
            int stars = theme != null ? theme.CalculateStars(currentScore) : 0;
            
            // Salvar pontuacao se for maior
            SetLevelScore(currentLevel, currentScore);
            SetLevelStars(currentLevel, stars);
            
            // Desbloquear proximo nivel
            if (currentLevel + 1 < TotalLevels)
            {
                if (currentLevel + 1 > unlockedLevel)
                {
                    unlockedLevel = currentLevel + 1;
                    SaveProgress();
                    OnLevelUnlocked?.Invoke(unlockedLevel);
                }
            }
            
            OnLevelCompleted?.Invoke(currentLevel, stars);
            
            Debug.Log($"Nivel {currentLevel} completo! Estrelas: {stars}, Score: {currentScore}");
        }
        
        /// <summary>
        /// Verifica se o nivel esta desbloqueado
        /// </summary>
        public bool IsLevelUnlocked(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex <= unlockedLevel;
        }
        
        /// <summary>
        /// Verifica se o nivel foi completado
        /// </summary>
        public bool IsLevelCompleted(int levelIndex)
        {
            return GetLevelStars(levelIndex) > 0;
        }
        
        /// <summary>
        /// Obtem o tema de um nivel
        /// </summary>
        public ThemeData GetTheme(int levelIndex)
        {
            if (themes == null || levelIndex < 0 || levelIndex >= themes.Length)
            {
                return null;
            }
            
            return themes[levelIndex];
        }
        
        /// <summary>
        /// Obtem a pontuacao de um nivel
        /// </summary>
        public int GetLevelScore(int levelIndex)
        {
            return PlayerPrefs.GetInt($"{LEVEL_SCORE_PREFIX}{levelIndex}", 0);
        }
        
        /// <summary>
        /// Define a pontuacao de um nivel
        /// </summary>
        public void SetLevelScore(int levelIndex, int score)
        {
            int currentHigh = GetLevelScore(levelIndex);
            
            if (score > currentHigh)
            {
                PlayerPrefs.SetInt($"{LEVEL_SCORE_PREFIX}{levelIndex}", score);
                PlayerPrefs.Save();
            }
        }
        
        /// <summary>
        /// Obtem as estrelas de um nivel
        /// </summary>
        public int GetLevelStars(int levelIndex)
        {
            return PlayerPrefs.GetInt($"{LEVEL_STARS_PREFIX}{levelIndex}", 0);
        }
        
        /// <summary>
        /// Define as estrelas de um nivel
        /// </summary>
        public void SetLevelStars(int levelIndex, int stars)
        {
            int currentStars = GetLevelStars(levelIndex);
            
            if (stars > currentStars)
            {
                PlayerPrefs.SetInt($"{LEVEL_STARS_PREFIX}{levelIndex}", stars);
                PlayerPrefs.Save();
            }
        }
        
        /// <summary>
        /// Adiciona pontos ao score atual
        /// </summary>
        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
            
            // Verificar se atingiu o target
            ThemeData theme = CurrentTheme;
            if (theme != null && currentScore >= theme.levelTargetScore && !levelCompleted)
            {
                CompleteLevel();
            }
        }
        
        /// <summary>
        /// Define o score atual
        /// </summary>
        public void SetCurrentScore(int score)
        {
            currentScore = score;
            OnScoreChanged?.Invoke(currentScore);
        }
        
        /// <summary>
        /// Obtem o score target do nivel atual
        /// </summary>
        public int GetTargetScore()
        {
            ThemeData theme = CurrentTheme;
            return theme != null ? theme.levelTargetScore : 1000;
        }
        
        /// <summary>
        /// Verifica se ha proximo nivel
        /// </summary>
        public bool HasNextLevel()
        {
            return currentLevel + 1 < TotalLevels;
        }
        
        /// <summary>
        /// Verifica se o nivel atual esta completo
        /// </summary>
        public bool IsCurrentLevelCompleted()
        {
            return levelCompleted;
        }
        
        /// <summary>
        /// Reseta todo o progresso
        /// </summary>
        public void ResetAllProgress()
        {
            unlockedLevel = 0;
            PlayerPrefs.DeleteKey(UNLOCKED_LEVEL_KEY);
            
            // Deletar scores e estrelas de todos os niveis
            for (int i = 0; i < TotalLevels; i++)
            {
                PlayerPrefs.DeleteKey($"{LEVEL_SCORE_PREFIX}{i}");
                PlayerPrefs.DeleteKey($"{LEVEL_STARS_PREFIX}{i}");
            }
            
            PlayerPrefs.Save();
            
            Debug.Log("Todo o progresso foi resetado!");
        }
        
        /// <summary>
        /// Define os temas disponiveis
        /// </summary>
        public void SetThemes(ThemeData[] newThemes)
        {
            themes = newThemes;
        }
        
        /// <summary>
        /// Desbloqueia todos os niveis (para teste)
        /// </summary>
        public void UnlockAllLevels()
        {
            unlockedLevel = TotalLevels - 1;
            SaveProgress();
            
            Debug.Log("Todos os niveis desbloqueados!");
        }
    }
}
