using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MergCrush.Theme;
using MergCrush.Level;

namespace MergCrush.UI
{
    /// <summary>
    /// Controla a interface do menu principal
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text highScoreText;
        
        [Header("Visual Settings")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private float buttonAnimationDelay = 0.1f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void Start()
        {
            UpdateHighScore();
            ApplyTheme();
            
            // Animacao inicial
            if (menuAnimator != null)
            {
                menuAnimator.SetTrigger("Show");
            }
        }
        
        /// <summary>
        /// Configura os botoes
        /// </summary>
        private void SetupButtons()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayClicked);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }
        
        /// <summary>
        /// Atualiza a exibicao do high score
        /// </summary>
        private void UpdateHighScore()
        {
            if (highScoreText != null)
            {
                int totalScore = 0;
                
                if (LevelManager.Instance != null)
                {
                    int totalLevels = LevelManager.Instance.TotalLevels;
                    for (int i = 0; i < totalLevels; i++)
                    {
                        totalScore += LevelManager.Instance.GetLevelScore(i);
                    }
                }
                
                highScoreText.text = $"Total Score: {FormatScore(totalScore)}";
            }
        }
        
        /// <summary>
        /// Aplica o tema visual
        /// </summary>
        private void ApplyTheme()
        {
            ThemeData theme = ThemeManager.Instance?.CurrentTheme;
            
            if (theme != null)
            {
                if (backgroundImage != null && theme.backgroundSprite != null)
                {
                    backgroundImage.sprite = theme.backgroundSprite;
                }
                
                if (titleText != null)
                {
                    titleText.color = theme.uiTextColor;
                }
            }
        }
        
        /// <summary>
        /// Chamado quando o botao Play e clicado
        /// </summary>
        public void OnPlayClicked()
        {
            PlayButtonSound();
            
            // Animar botao
            AnimateButton(playButton);
            
            // Ir para selecao de nivel
            SceneManager.LoadScene("LevelSelect");
        }
        
        /// <summary>
        /// Chamado quando o botao Settings e clicado
        /// </summary>
        public void OnSettingsClicked()
        {
            PlayButtonSound();
            
            AnimateButton(settingsButton);
            
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// Chamado quando o botao Quit e clicado
        /// </summary>
        public void OnQuitClicked()
        {
            PlayButtonSound();
            
            AnimateButton(quitButton);
            
            // Salvar antes de sair
            PlayerPrefs.Save();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        /// <summary>
        /// Reproduz o som de clique do botao
        /// </summary>
        private void PlayButtonSound()
        {
            if (audioSource != null && buttonClickSound != null)
            {
                audioSource.PlayOneShot(buttonClickSound);
            }
        }
        
        /// <summary>
        /// Anima o botao
        /// </summary>
        private void AnimateButton(Button button)
        {
            if (button != null)
            {
                StartCoroutine(AnimateButtonCoroutine(button));
            }
        }
        
        private System.Collections.IEnumerator AnimateButtonCoroutine(Button button)
        {
            Vector3 originalScale = button.transform.localScale;
            Vector3 pressedScale = originalScale * 0.9f;
            
            float duration = 0.1f;
            float elapsed = 0f;
            
            // Animar para baixo
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                button.transform.localScale = Vector3.Lerp(originalScale, pressedScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            
            // Animar para cima
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                button.transform.localScale = Vector3.Lerp(pressedScale, originalScale, t);
                yield return null;
            }
            
            button.transform.localScale = originalScale;
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
        
        /// <summary>
        /// Fecha o painel de configuracoes
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }
    }
}
