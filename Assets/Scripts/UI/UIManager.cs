using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using MergCrush.Theme;
using MergCrush.Level;
using MergCrush.Core;

namespace MergCrush.UI
{
    /// <summary>
    /// Gerencia todas as telas de interface do jogo
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private GameObject settingsPanel;
        
        [Header("Game UI References")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text targetScoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starEmpty;
        [SerializeField] private Sprite starFilled;
        
        [Header("Animation Settings")]
        [SerializeField] private float panelFadeDuration = 0.3f;
        [SerializeField] private AnimationCurve panelAnimationCurve;
        
        // Estado atual
        private GameObject currentPanel;
        private Stack<GameObject> panelHistory = new Stack<GameObject>();
        private bool isTransitioning = false;
        
        // Eventos
        public System.Action OnPauseGame;
        public System.Action OnResumeGame;
        public System.Action OnBackToMenu;
        
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
            // Esconder todos os paineis
            HideAllPanels();
            
            // Mostrar painel apropriado baseado na cena
            string sceneName = SceneManager.GetActiveScene().name;
            
            if (sceneName == "MainMenu" || sceneName == "SampleScene")
            {
                ShowPanel(mainMenuPanel);
            }
            else if (sceneName == "LevelSelect")
            {
                ShowPanel(levelSelectPanel);
            }
            else if (sceneName == "GameScene")
            {
                ShowPanel(gamePanel);
            }
            
            // Registrar eventos
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
                ScoreManager.Instance.OnComboIncreased += UpdateComboUI;
                ScoreManager.Instance.OnComboEnded += HideComboUI;
            }
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCompleted += ShowLevelComplete;
            }
        }
        
        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
                ScoreManager.Instance.OnComboIncreased -= UpdateComboUI;
                ScoreManager.Instance.OnComboEnded -= HideComboUI;
            }
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCompleted -= ShowLevelComplete;
            }
        }
        
        /// <summary>
        /// Mostra um painel especifico
        /// </summary>
        public void ShowPanel(GameObject panel)
        {
            if (panel == null || isTransitioning) return;
            
            if (currentPanel != null)
            {
                panelHistory.Push(currentPanel);
                currentPanel.SetActive(false);
            }
            
            panel.SetActive(true);
            currentPanel = panel;
            
            // Animar entrada
            StartCoroutine(AnimatePanelIn(panel));
        }
        
        /// <summary>
        /// Volta para o painel anterior
        /// </summary>
        public void GoBack()
        {
            if (panelHistory.Count > 0)
            {
                if (currentPanel != null)
                {
                    currentPanel.SetActive(false);
                }
                
                GameObject previousPanel = panelHistory.Pop();
                previousPanel.SetActive(true);
                currentPanel = previousPanel;
            }
        }
        
        /// <summary>
        /// Esconde todos os paineis
        /// </summary>
        public void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (gamePanel != null) gamePanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            
            panelHistory.Clear();
            currentPanel = null;
        }
        
        /// <summary>
        /// Animacao de entrada do painel
        /// </summary>
        private System.Collections.IEnumerator AnimatePanelIn(GameObject panel)
        {
            isTransitioning = true;
            
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            
            float elapsed = 0f;
            
            while (elapsed < panelFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / panelFadeDuration;
                
                if (panelAnimationCurve != null && panelAnimationCurve.length > 0)
                {
                    t = panelAnimationCurve.Evaluate(t);
                }
                
                canvasGroup.alpha = t;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            isTransitioning = false;
        }
        
        #region Game UI Updates
        
        /// <summary>
        /// Atualiza a UI de score
        /// </summary>
        private void UpdateScoreUI(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = FormatScore(score);
            }
            
            // Atualizar progresso
            if (progressSlider != null && LevelManager.Instance != null)
            {
                int target = LevelManager.Instance.GetTargetScore();
                progressSlider.value = (float)score / target;
                
                // Atualizar estrelas
                UpdateStars(score, target);
            }
            
            // Atualizar target
            if (targetScoreText != null && LevelManager.Instance != null)
            {
                targetScoreText.text = $"Target: {FormatScore(LevelManager.Instance.GetTargetScore())}";
            }
        }
        
        /// <summary>
        /// Atualiza as estrelas baseado no score
        /// </summary>
        private void UpdateStars(int score, int target)
        {
            float percentage = (float)score / target * 100f;
            
            ThemeData theme = ThemeManager.Instance?.CurrentTheme;
            
            if (theme != null)
            {
                int stars = theme.CalculateStars(score);
                
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] != null)
                    {
                        bool filled = i < stars;
                        starImages[i].sprite = filled ? starFilled : starEmpty;
                    }
                }
            }
        }
        
        /// <summary>
        /// Atualiza a UI de combo
        /// </summary>
        private void UpdateComboUI(int comboCount)
        {
            if (comboText != null)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"COMBO x{comboCount}!";
            }
        }
        
        /// <summary>
        /// Esconde a UI de combo
        /// </summary>
        private void HideComboUI()
        {
            if (comboText != null)
            {
                comboText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Formata o score para exibicao
        /// </summary>
        private string FormatScore(int score)
        {
            if (score >= 1000000)
            {
                return $"{score / 1000000f:F1}M";
            }
            else if (score >= 1000)
            {
                return $"{score / 1000f:F1}K";
            }
            return score.ToString();
        }
        
        #endregion
        
        #region Scene Navigation
        
        /// <summary>
        /// Vai para o menu principal
        /// </summary>
        public void GoToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        /// <summary>
        /// Vai para a tela de selecao de niveis
        /// </summary>
        public void GoToLevelSelect()
        {
            SceneManager.LoadScene("LevelSelect");
        }
        
        /// <summary>
        /// Inicia o jogo no nivel especificado
        /// </summary>
        public void StartLevel(int levelIndex)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(levelIndex);
            }
        }
        
        /// <summary>
        /// Reinicia o nivel atual
        /// </summary>
        public void RestartLevel()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ReloadLevel();
            }
        }
        
        /// <summary>
        /// Vai para o proximo nivel
        /// </summary>
        public void GoToNextLevel()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
        }
        
        #endregion
        
        #region Game State
        
        /// <summary>
        /// Pausa o jogo
        /// </summary>
        public void PauseGame()
        {
            Time.timeScale = 0f;
            ShowPanel(pausePanel);
            OnPauseGame?.Invoke();
        }
        
        /// <summary>
        /// Retoma o jogo
        /// </summary>
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            
            if (gamePanel != null)
            {
                currentPanel = gamePanel;
            }
            
            OnResumeGame?.Invoke();
        }
        
        /// <summary>
        /// Mostra tela de game over
        /// </summary>
        public void ShowGameOver()
        {
            ShowPanel(gameOverPanel);
        }
        
        /// <summary>
        /// Mostra tela de nivel completo
        /// </summary>
        public void ShowLevelComplete(int levelIndex, int stars)
        {
            // Atualizar informacoes no painel
            if (levelCompletePanel != null)
            {
                // Atualizar estrelas
                Image[] panelStars = levelCompletePanel.GetComponentsInChildren<Image>();
                // Implementar logica de estrelas no painel
                
                // Atualizar score
                Text[] texts = levelCompletePanel.GetComponentsInChildren<Text>();
                foreach (var text in texts)
                {
                    if (text.name.Contains("Score"))
                    {
                        text.text = $"Score: {FormatScore(ScoreManager.Instance.CurrentScore)}";
                    }
                }
            }
            
            ShowPanel(levelCompletePanel);
        }
        
        #endregion
        
        #region Settings
        
        /// <summary>
        /// Abre o painel de configuracoes
        /// </summary>
        public void OpenSettings()
        {
            ShowPanel(settingsPanel);
        }
        
        /// <summary>
        /// Fecha o painel de configuracoes
        /// </summary>
        public void CloseSettings()
        {
            GoBack();
        }
        
        #endregion
        
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
}
