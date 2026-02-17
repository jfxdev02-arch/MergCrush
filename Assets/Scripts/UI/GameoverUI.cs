using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MergCrush.Theme;
using MergCrush.Level;

namespace MergCrush.UI
{
    /// <summary>
    /// Controla a interface de fim de fase (vitoria/derrota)
    /// </summary>
    public class GameoverUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        
        [Header("Victory UI")]
        [SerializeField] private Text victoryTitleText;
        [SerializeField] private Text victoryScoreText;
        [SerializeField] private Text victoryHighScoreText;
        [SerializeField] private Image[] victoryStars;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button replayVictoryButton;
        [SerializeField] private Button menuVictoryButton;
        
        [Header("Defeat UI")]
        [SerializeField] private Text defeatTitleText;
        [SerializeField] private Text defeatScoreText;
        [SerializeField] private Text defeatReasonText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuDefeatButton;
        
        [Header("Shared Elements")]
        [SerializeField] private Sprite starEmptySprite;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private ParticleSystem victoryParticles;
        
        [Header("Animation")]
        [SerializeField] private Animator panelAnimator;
        [SerializeField] private float starAnimationDelay = 0.3f;
        [SerializeField] private float buttonAnimationDelay = 0.5f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip victorySound;
        [SerializeField] private AudioClip defeatSound;
        [SerializeField] private AudioClip starSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioSource audioSource;
        
        // Estado
        private int currentLevel = 0;
        private int currentStars = 0;
        private int currentScore = 0;
        private bool isVictory = false;
        
        private void Awake()
        {
            SetupButtons();
            HideAllPanels();
        }
        
        /// <summary>
        /// Configura os botoes
        /// </summary>
        private void SetupButtons()
        {
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }
            
            if (replayVictoryButton != null)
            {
                replayVictoryButton.onClick.AddListener(OnReplayClicked);
            }
            
            if (menuVictoryButton != null)
            {
                menuVictoryButton.onClick.AddListener(OnMenuClicked);
            }
            
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnReplayClicked);
            }
            
            if (menuDefeatButton != null)
            {
                menuDefeatButton.onClick.AddListener(OnMenuClicked);
            }
        }
        
        /// <summary>
        /// Esconde todos os paineis
        /// </summary>
        public void HideAllPanels()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }
        
        /// <summary>
        /// Mostra a tela de vitoria
        /// </summary>
        public void ShowVictory(int levelIndex, int score, int stars)
        {
            isVictory = true;
            currentLevel = levelIndex;
            currentScore = score;
            currentStars = stars;
            
            HideAllPanels();
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
            
            // Atualizar textos
            if (victoryTitleText != null)
            {
                victoryTitleText.text = "NIVEL COMPLETO!";
            }
            
            if (victoryScoreText != null)
            {
                victoryScoreText.text = $"Score: {FormatScore(score)}";
            }
            
            if (victoryHighScoreText != null && LevelManager.Instance != null)
            {
                int highScore = LevelManager.Instance.GetLevelScore(levelIndex);
                victoryHighScoreText.text = $"High Score: {FormatScore(highScore)}";
            }
            
            // Atualizar botao de proximo nivel
            if (nextLevelButton != null)
            {
                bool hasNext = LevelManager.Instance != null && LevelManager.Instance.HasNextLevel();
                nextLevelButton.gameObject.SetActive(hasNext);
            }
            
            // Animar estrelas
            StartCoroutine(AnimateStars(stars));
            
            // Tocar som de vitoria
            PlaySound(victorySound);
            
            // Ativar particulas
            if (victoryParticles != null)
            {
                victoryParticles.Play();
            }
            
            // Animar painel
            if (panelAnimator != null)
            {
                panelAnimator.SetTrigger("ShowVictory");
            }
            
            // Aplicar tema
            ApplyThemeToPanel(victoryPanel);
        }
        
        /// <summary>
        /// Mostra a tela de derrota
        /// </summary>
        public void ShowDefeat(int levelIndex, int score, string reason = "")
        {
            isVictory = false;
            currentLevel = levelIndex;
            currentScore = score;
            
            HideAllPanels();
            
            if (defeatPanel != null)
            {
                defeatPanel.SetActive(true);
            }
            
            // Atualizar textos
            if (defeatTitleText != null)
            {
                defeatTitleText.text = "GAME OVER";
            }
            
            if (defeatScoreText != null)
            {
                defeatScoreText.text = $"Score: {FormatScore(score)}";
            }
            
            if (defeatReasonText != null)
            {
                if (string.IsNullOrEmpty(reason))
                {
                    defeatReasonText.text = "A grid esta cheia!";
                }
                else
                {
                    defeatReasonText.text = reason;
                }
            }
            
            // Tocar som de derrota
            PlaySound(defeatSound);
            
            // Animar painel
            if (panelAnimator != null)
            {
                panelAnimator.SetTrigger("ShowDefeat");
            }
            
            // Aplicar tema
            ApplyThemeToPanel(defeatPanel);
        }
        
        /// <summary>
        /// Anima as estrelas aparecendo uma a uma
        /// </summary>
        private System.Collections.IEnumerator AnimateStars(int stars)
        {
            // Resetar estrelas
            foreach (var star in victoryStars)
            {
                if (star != null)
                {
                    star.sprite = starEmptySprite;
                    star.transform.localScale = Vector3.one;
                }
            }
            
            yield return new WaitForSeconds(starAnimationDelay);
            
            // Animar cada estrela
            for (int i = 0; i < victoryStars.Length; i++)
            {
                if (victoryStars[i] != null)
                {
                    if (i < stars)
                    {
                        victoryStars[i].sprite = starFilledSprite;
                        
                        // Animacao de escala
                        StartCoroutine(PunchScale(victoryStars[i].transform));
                        
                        // Som de estrela
                        PlaySound(starSound);
                        
                        yield return new WaitForSeconds(starAnimationDelay);
                    }
                    else
                    {
                        victoryStars[i].sprite = starEmptySprite;
                    }
                }
            }
        }
        
        /// <summary>
        /// Animacao de escala tipo "punch"
        /// </summary>
        private System.Collections.IEnumerator PunchScale(Transform target)
        {
            Vector3 originalScale = Vector3.one;
            Vector3 punchScale = originalScale * 1.3f;
            
            float duration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                float punch = Mathf.Sin(t * Mathf.PI);
                target.localScale = Vector3.Lerp(originalScale, punchScale, punch);
                
                yield return null;
            }
            
            target.localScale = originalScale;
        }
        
        #region Button Callbacks
        
        /// <summary>
        /// Chamado quando o botao Proximo Nivel e clicado
        /// </summary>
        private void OnNextLevelClicked()
        {
            PlaySound(buttonClickSound);
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
        }
        
        /// <summary>
        /// Chamado quando o botao Replay e clicado
        /// </summary>
        private void OnReplayClicked()
        {
            PlaySound(buttonClickSound);
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ReloadLevel();
            }
        }
        
        /// <summary>
        /// Chamado quando o botao Menu e clicado
        /// </summary>
        private void OnMenuClicked()
        {
            PlaySound(buttonClickSound);
            SceneManager.LoadScene("LevelSelect");
        }
        
        #endregion
        
        /// <summary>
        /// Reproduz um som
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
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
            return score.ToString("N0");
        }
        
        /// <summary>
        /// Aplica o tema visual ao painel
        /// </summary>
        private void ApplyThemeToPanel(GameObject panel)
        {
            ThemeData theme = ThemeManager.Instance?.CurrentTheme;
            
            if (theme == null || panel == null) return;
            
            // Aplicar cores de destaque
            Image[] images = panel.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name.Contains("Accent") || img.name.Contains("Background"))
                {
                    img.color = theme.uiAccentColor;
                }
            }
            
            // Aplicar cor de texto
            Text[] texts = panel.GetComponentsInChildren<Text>();
            foreach (var text in texts)
            {
                text.color = theme.uiTextColor;
            }
        }
        
    }
}
