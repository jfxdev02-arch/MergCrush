using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using MergCrush.Theme;
using MergCrush.Level;

namespace MergCrush.UI
{
    /// <summary>
    /// Controla a interface de selecao de niveis/fases
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("Level Buttons")]
        [SerializeField] private Transform buttonsContainer;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private int buttonsPerRow = 3;
        [SerializeField] private float buttonSpacing = 20f;
        
        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private Text titleText;
        [SerializeField] private ScrollRect scrollRect;
        
        [Header("Level Info Panel")]
        [SerializeField] private GameObject levelInfoPanel;
        [SerializeField] private Image levelPreviewImage;
        [SerializeField] private Text levelNameText;
        [SerializeField] private Text levelDescriptionText;
        [SerializeField] private Text targetScoreText;
        [SerializeField] private Button playLevelButton;
        [SerializeField] private Image[] infoStars;
        
        [Header("Visual Settings")]
        [SerializeField] private Sprite lockedLevelSprite;
        [SerializeField] private Sprite starEmptySprite;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color unlockedColor = Color.white;
        
        [Header("Audio")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip levelSelectSound;
        [SerializeField] private AudioSource audioSource;
        
        // Estado
        private List<LevelButton> levelButtons = new List<LevelButton>();
        private int selectedLevel = -1;
        
        private void Awake()
        {
            SetupBackButton();
        }
        
        private void Start()
        {
            GenerateLevelButtons();
            ApplyTheme();
        }
        
        /// <summary>
        /// Configura o botao de voltar
        /// </summary>
        private void SetupBackButton()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
            }
        }
        
        /// <summary>
        /// Gera os botoes de nivel dinamicamente
        /// </summary>
        private void GenerateLevelButtons()
        {
            // Limpar botoes existentes
            foreach (var btn in levelButtons)
            {
                if (btn != null && btn.gameObject != null)
                {
                    Destroy(btn.gameObject);
                }
            }
            levelButtons.Clear();
            
            if (LevelManager.Instance == null) return;
            
            int totalLevels = LevelManager.Instance.TotalLevels;
            
            // Criar grid layout se nao existir
            if (buttonsContainer != null)
            {
                GridLayoutGroup gridLayout = buttonsContainer.GetComponent<GridLayoutGroup>();
                if (gridLayout == null)
                {
                    gridLayout = buttonsContainer.gameObject.AddComponent<GridLayoutGroup>();
                }
                
                float buttonSize = 150f;
                gridLayout.cellSize = new Vector2(buttonSize, buttonSize);
                gridLayout.spacing = new Vector2(buttonSpacing, buttonSpacing);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = buttonsPerRow;
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
            }
            
            // Criar botoes para cada nivel
            for (int i = 0; i < totalLevels; i++)
            {
                CreateLevelButton(i);
            }
            
            // Selecionar primeiro nivel disponivel
            if (levelButtons.Count > 0)
            {
                SelectLevel(0);
            }
        }
        
        /// <summary>
        /// Cria um botao de nivel
        /// </summary>
        private void CreateLevelButton(int levelIndex)
        {
            GameObject buttonObj;
            
            if (levelButtonPrefab != null)
            {
                buttonObj = Instantiate(levelButtonPrefab, buttonsContainer);
            }
            else
            {
                buttonObj = CreateDefaultLevelButton();
            }
            
            buttonObj.transform.SetParent(buttonsContainer, false);
            buttonObj.name = $"Level_{levelIndex + 1}";
            
            LevelButton levelButton = buttonObj.GetComponent<LevelButton>();
            if (levelButton == null)
            {
                levelButton = buttonObj.AddComponent<LevelButton>();
            }
            
            // Configurar botao
            ThemeData theme = LevelManager.Instance.GetTheme(levelIndex);
            bool isUnlocked = LevelManager.Instance.IsLevelUnlocked(levelIndex);
            int stars = LevelManager.Instance.GetLevelStars(levelIndex);
            int highScore = LevelManager.Instance.GetLevelScore(levelIndex);
            
            levelButton.Setup(levelIndex, theme, isUnlocked, stars, highScore);
            levelButton.OnClicked += OnLevelButtonClicked;
            
            levelButtons.Add(levelButton);
        }
        
        /// <summary>
        /// Cria um botao de nivel padrao
        /// </summary>
        private GameObject CreateDefaultLevelButton()
        {
            GameObject buttonObj = new GameObject("LevelButton");
            
            // Adicionar RectTransform
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            
            // Adicionar Image
            Image image = buttonObj.AddComponent<Image>();
            image.color = Color.white;
            
            // Adicionar Button
            Button button = buttonObj.AddComponent<Button>();
            
            // Criar texto do numero do nivel
            GameObject textObj = new GameObject("Number");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            Text text = textObj.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return buttonObj;
        }
        
        /// <summary>
        /// Chamado quando um botao de nivel e clicado
        /// </summary>
        private void OnLevelButtonClicked(int levelIndex)
        {
            PlaySound(levelSelectSound);
            SelectLevel(levelIndex);
        }
        
        /// <summary>
        /// Seleciona um nivel e mostra informacoes
        /// </summary>
        private void SelectLevel(int levelIndex)
        {
            selectedLevel = levelIndex;
            
            // Atualizar visual dos botoes
            foreach (var btn in levelButtons)
            {
                btn.SetSelected(btn.LevelIndex == levelIndex);
            }
            
            // Mostrar painel de informacoes
            UpdateLevelInfo(levelIndex);
        }
        
        /// <summary>
        /// Atualiza o painel de informacoes do nivel
        /// </summary>
        private void UpdateLevelInfo(int levelIndex)
        {
            if (levelInfoPanel == null) return;
            
            ThemeData theme = LevelManager.Instance?.GetTheme(levelIndex);
            bool isUnlocked = LevelManager.Instance?.IsLevelUnlocked(levelIndex) ?? false;
            int stars = LevelManager.Instance?.GetLevelStars(levelIndex) ?? 0;
            int highScore = LevelManager.Instance?.GetLevelScore(levelIndex) ?? 0;
            
            levelInfoPanel.SetActive(true);
            
            // Atualizar preview
            if (levelPreviewImage != null)
            {
                if (isUnlocked && theme != null && theme.themeIcon != null)
                {
                    levelPreviewImage.sprite = theme.themeIcon;
                    levelPreviewImage.color = Color.white;
                }
                else if (lockedLevelSprite != null)
                {
                    levelPreviewImage.sprite = lockedLevelSprite;
                    levelPreviewImage.color = lockedColor;
                }
            }
            
            // Atualizar textos
            if (levelNameText != null)
            {
                levelNameText.text = isUnlocked && theme != null 
                    ? $"{levelIndex + 1}. {theme.themeName}" 
                    : $"{levelIndex + 1}. ???";
            }
            
            if (levelDescriptionText != null)
            {
                levelDescriptionText.text = isUnlocked && theme != null 
                    ? theme.description 
                    : "Complete o nivel anterior para desbloquear!";
            }
            
            if (targetScoreText != null)
            {
                targetScoreText.text = isUnlocked && theme != null 
                    ? $"Target: {theme.levelTargetScore:N0}\nHigh Score: {highScore:N0}"
                    : "Target: ???";
            }
            
            // Atualizar estrelas
            for (int i = 0; i < infoStars.Length; i++)
            {
                if (infoStars[i] != null)
                {
                    if (isUnlocked)
                    {
                        infoStars[i].sprite = i < stars ? starFilledSprite : starEmptySprite;
                        infoStars[i].color = Color.white;
                    }
                    else
                    {
                        infoStars[i].sprite = starEmptySprite;
                        infoStars[i].color = lockedColor;
                    }
                }
            }
            
            // Atualizar botao de play
            if (playLevelButton != null)
            {
                playLevelButton.interactable = isUnlocked;
                playLevelButton.onClick.RemoveAllListeners();
                
                if (isUnlocked)
                {
                    playLevelButton.onClick.AddListener(() => OnPlayLevelClicked(levelIndex));
                }
            }
        }
        
        /// <summary>
        /// Chamado quando o botao Play e clicado
        /// </summary>
        private void OnPlayLevelClicked(int levelIndex)
        {
            PlaySound(buttonClickSound);
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(levelIndex);
            }
        }
        
        /// <summary>
        /// Chamado quando o botao Back e clicado
        /// </summary>
        private void OnBackClicked()
        {
            PlaySound(buttonClickSound);
            SceneManager.LoadScene("MainMenu");
        }
        
        /// <summary>
        /// Aplica o tema visual
        /// </summary>
        private void ApplyTheme()
        {
            ThemeData theme = ThemeManager.Instance?.CurrentTheme;
            
            if (theme != null)
            {
                if (titleText != null)
                {
                    titleText.color = theme.uiAccentColor;
                }
            }
        }
        
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
    }
    
    /// <summary>
    /// Componente de botao de nivel
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text levelNumberText;
        [SerializeField] private Image[] starImages;
        
        [Header("Visual Settings")]
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite starEmptySprite;
        [SerializeField] private Sprite starFilledSprite;
        
        // Estado
        private int levelIndex;
        private bool isUnlocked;
        private bool isSelected;
        private ThemeData theme;
        
        // Evento
        public System.Action<int> OnClicked;
        
        public int LevelIndex => levelIndex;
        
        private void Awake()
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
        }
        
        /// <summary>
        /// Configura o botao de nivel
        /// </summary>
        public void Setup(int index, ThemeData themeData, bool unlocked, int stars, int highScore)
        {
            levelIndex = index;
            theme = themeData;
            isUnlocked = unlocked;
            
            // Atualizar numero
            if (levelNumberText != null)
            {
                levelNumberText.text = $"{index + 1}";
            }
            
            // Atualizar icone
            if (iconImage != null)
            {
                if (isUnlocked && theme != null && theme.themeIcon != null)
                {
                    iconImage.sprite = theme.themeIcon;
                    iconImage.color = Color.white;
                }
                else if (lockedSprite != null)
                {
                    iconImage.sprite = lockedSprite;
                    iconImage.color = Color.gray;
                }
            }
            
            // Atualizar background
            if (backgroundImage != null)
            {
                backgroundImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
            }
            
            // Atualizar estrelas
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    bool hasStar = isUnlocked && i < stars;
                    starImages[i].sprite = hasStar ? starFilledSprite : starEmptySprite;
                    starImages[i].color = isUnlocked ? Color.white : Color.gray;
                }
            }
        }
        
        /// <summary>
        /// Define se o botao esta selecionado
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (backgroundImage != null && selectedSprite != null && selected && isUnlocked)
            {
                backgroundImage.sprite = selectedSprite;
            }
            else if (backgroundImage != null)
            {
                backgroundImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
            }
        }
        
        private void OnClick()
        {
            OnClicked?.Invoke(levelIndex);
        }
    }
}
