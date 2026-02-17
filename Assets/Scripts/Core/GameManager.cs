using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using MergCrush.Theme;
using MergCrush.Level;
using MergCrush.UI;

namespace MergCrush.Core
{
    /// <summary>
    /// Gerenciador principal do jogo - orquestra todos os sistemas
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private Configuration config;
        
        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Menu;
        [SerializeField] private bool isPaused = false;
        
        // Referencias dos managers
        private GridManager gridManager;
        private CubeSpawner cubeSpawner;
        private CubeCollision cubeCollision;
        private ScoreManager scoreManager;
        private ThemeManager themeManager;
        private LevelManager levelManager;
        
        // Estado
        private int initialCubes = 5;
        
        public GameState CurrentState => currentState;
        public bool IsPaused => isPaused;
        
        // Eventos
        public System.Action<GameState> OnStateChanged;
        public System.Action OnGameStart;
        public System.Action OnGameEnd;
        
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
            
            // Carregar configuracao
            if (config == null)
            {
                config = Resources.Load<Configuration>("GameConfiguration");
            }
        }
        
        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Se estiver na cena de jogo, inicializar
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                InitializeGame();
            }
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        /// <summary>
        /// Chamado quando uma cena e carregada
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "GameScene")
            {
                InitializeGame();
            }
            else if (scene.name == "MainMenu" || scene.name == "LevelSelect")
            {
                currentState = GameState.Menu;
            }
        }
        
        /// <summary>
        /// Inicializa o jogo
        /// </summary>
        public void InitializeGame()
        {
            // Obter referencias dos managers
            gridManager = GridManager.Instance;
            cubeSpawner = CubeSpawner.Instance;
            cubeCollision = CubeCollision.Instance;
            scoreManager = ScoreManager.Instance;
            themeManager = ThemeManager.Instance;
            levelManager = LevelManager.Instance;
            
            // Configurar managers com a configuration
            if (config != null)
            {
                if (gridManager != null) gridManager.SetConfig(config);
                if (cubeSpawner != null) cubeSpawner.SetConfig(config);
                if (cubeCollision != null) cubeCollision.SetConfig(config);
                if (scoreManager != null) scoreManager.SetConfig(config);
            }
            
            // Inicializar grid
            if (gridManager != null)
            {
                gridManager.InitializeGrid();
            }
            
            // Aplicar tema do nivel atual
            if (levelManager != null && themeManager != null)
            {
                ThemeData theme = levelManager.CurrentTheme;
                if (theme != null)
                {
                    themeManager.ApplyTheme(theme, levelManager.CurrentLevel);
                    initialCubes = theme.levelDifficulty + 4;
                }
            }
            
            // Resetar score
            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }
            
            // Iniciar o jogo
            StartCoroutine(StartGameRoutine());
        }
        
        /// <summary>
        /// Rotina de inicio do jogo
        /// </summary>
        private IEnumerator StartGameRoutine()
        {
            currentState = GameState.Playing;
            OnStateChanged?.Invoke(currentState);
            
            yield return new WaitForSeconds(0.5f);
            
            // Spawnar cubos iniciais
            if (cubeSpawner != null)
            {
                cubeSpawner.SpawnInitialCubes(initialCubes);
            }
            
            OnGameStart?.Invoke();
            
            Debug.Log("Jogo iniciado!");
        }
        
        /// <summary>
        /// Pausa o jogo
        /// </summary>
        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            
            isPaused = true;
            Time.timeScale = 0f;
            currentState = GameState.Paused;
            OnStateChanged?.Invoke(currentState);
        }
        
        /// <summary>
        /// Retoma o jogo
        /// </summary>
        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            
            isPaused = false;
            Time.timeScale = 1f;
            currentState = GameState.Playing;
            OnStateChanged?.Invoke(currentState);
        }
        
        /// <summary>
        /// Verifica condicoes de fim de jogo
        /// </summary>
        public void CheckGameOver()
        {
            if (currentState != GameState.Playing) return;
            
            // Verificar se a grid esta bloqueada
            if (cubeCollision != null && cubeCollision.IsGridBlocked())
            {
                EndGame(false);
            }
        }
        
        /// <summary>
        /// Finaliza o jogo
        /// </summary>
        public void EndGame(bool victory)
        {
            currentState = victory ? GameState.Victory : GameState.GameOver;
            OnStateChanged?.Invoke(currentState);
            OnGameEnd?.Invoke();
            
            if (victory)
            {
                if (levelManager != null)
                {
                    levelManager.CompleteLevel();
                }
                
                Debug.Log("Nivel completo!");
            }
            else
            {
                Debug.Log("Game Over!");
            }
        }
        
        /// <summary>
        /// Reinicia o nivel atual
        /// </summary>
        public void RestartLevel()
        {
            Time.timeScale = 1f;
            isPaused = false;
            
            if (levelManager != null)
            {
                levelManager.ReloadLevel();
            }
            else
            {
                SceneManager.LoadScene("GameScene");
            }
        }
        
        /// <summary>
        /// Vai para o proximo nivel
        /// </summary>
        public void NextLevel()
        {
            Time.timeScale = 1f;
            isPaused = false;
            
            if (levelManager != null)
            {
                levelManager.LoadNextLevel();
            }
        }
        
        /// <summary>
        /// Volta para o menu
        /// </summary>
        public void GoToMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;
            currentState = GameState.Menu;
            
            SceneManager.LoadScene("MainMenu");
        }
        
        /// <summary>
        /// Vai para selecao de nivel
        /// </summary>
        public void GoToLevelSelect()
        {
            Time.timeScale = 1f;
            isPaused = false;
            currentState = GameState.Menu;
            
            SceneManager.LoadScene("LevelSelect");
        }
        
        /// <summary>
        /// Sai do jogo
        /// </summary>
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
    
    /// <summary>
    /// Estados do jogo
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Victory,
        GameOver
    }
}
