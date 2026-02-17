using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MergCrush.Theme
{
    /// <summary>
    /// Gerencia a aplicacao de temas visuais no jogo
    /// </summary>
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Canvas mainCanvas;
        
        [Header("UI Elements")]
        [SerializeField] private Image[] accentElements;
        [SerializeField] private Text[] textElements;
        [SerializeField] private Image[] panelElements;
        
        [Header("Current State")]
        [SerializeField] private ThemeData currentTheme;
        [SerializeField] private int currentThemeIndex = 0;
        
        // Estado interno
        private List<SpriteRenderer> backgroundRenderers = new List<SpriteRenderer>();
        private List<Image> dynamicImages = new List<Image>();
        private AudioSource bgmSource;
        
        // Eventos
        public System.Action<ThemeData> OnThemeChanged;
        
        public ThemeData CurrentTheme => currentTheme;
        public int CurrentThemeIndex => currentThemeIndex;
        
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
        }
        
        private void Start()
        {
            // Encontrar camera principal se nao definida
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            
            // Configurar audio source para BGM
            SetupBGMAudioSource();
        }
        
        /// <summary>
        /// Configura o AudioSource para musica de fundo
        /// </summary>
        private void SetupBGMAudioSource()
        {
            bgmSource = GetComponent<AudioSource>();
            
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }
            
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
        
        /// <summary>
        /// Aplica um tema ao jogo
        /// </summary>
        public void ApplyTheme(ThemeData theme, int index = -1)
        {
            if (theme == null)
            {
                Debug.LogError("Tentativa de aplicar tema nulo!");
                return;
            }
            
            currentTheme = theme;
            
            if (index >= 0)
            {
                currentThemeIndex = index;
            }
            
            // Aplicar background
            UpdateBackground(theme);
            
            // Aplicar cores da UI
            UpdateUIColors(theme);
            
            // Atualizar cubos existentes
            UpdateExistingCubes(theme);
            
            // Tocar musica do tema
            PlayThemeBGM(theme);
            
            // Disparar evento
            OnThemeChanged?.Invoke(theme);
            
            Debug.Log($"Tema aplicado: {theme.themeName}");
        }
        
        /// <summary>
        /// Aplica tema por indice (do LevelManager)
        /// </summary>
        public void ApplyThemeByIndex(int index)
        {
            Level.LevelManager levelManager = Level.LevelManager.Instance;
            
            if (levelManager != null)
            {
                ThemeData theme = levelManager.GetTheme(index);
                
                if (theme != null)
                {
                    ApplyTheme(theme, index);
                }
            }
        }
        
        /// <summary>
        /// Atualiza o background do jogo
        /// </summary>
        public void UpdateBackground(ThemeData theme)
        {
            if (theme == null) return;
            
            // Atualizar Image de background
            if (backgroundImage != null)
            {
                if (theme.backgroundSprite != null)
                {
                    backgroundImage.sprite = theme.backgroundSprite;
                    backgroundImage.color = Color.white;
                }
                else
                {
                    backgroundImage.sprite = null;
                    backgroundImage.color = theme.backgroundColor;
                }
            }
            
            // Atualizar cor de fundo da camera
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = theme.backgroundColor;
            }
            
            // Atualizar SpriteRenderers de background
            foreach (var renderer in backgroundRenderers)
            {
                if (renderer != null)
                {
                    if (theme.backgroundSprite != null)
                    {
                        renderer.sprite = theme.backgroundSprite;
                        renderer.color = Color.white;
                    }
                    else
                    {
                        renderer.color = theme.backgroundColor;
                    }
                }
            }
        }
        
        /// <summary>
        /// Atualiza as cores da UI
        /// </summary>
        public void UpdateUIColors(ThemeData theme)
        {
            if (theme == null) return;
            
            // Elementos de destaque
            foreach (var element in accentElements)
            {
                if (element != null)
                {
                    element.color = theme.uiAccentColor;
                }
            }
            
            // Textos
            foreach (var text in textElements)
            {
                if (text != null)
                {
                    text.color = theme.uiTextColor;
                }
            }
            
            // Paineis
            foreach (var panel in panelElements)
            {
                if (panel != null)
                {
                    panel.color = theme.uiSecondaryColor;
                }
            }
        }
        
        /// <summary>
        /// Atualiza os cubos existentes com os sprites do novo tema
        /// </summary>
        private void UpdateExistingCubes(ThemeData theme)
        {
            if (theme == null) return;
            
            Core.GridManager gridManager = Core.GridManager.Instance;
            
            if (gridManager == null) return;
            
            List<Core.Cube> cubes = gridManager.GetAllCubes();
            
            foreach (var cube in cubes)
            {
                if (cube != null)
                {
                    int level = cube.ItemLevel;
                    
                    Sprite sprite = theme.GetItemSprite(level);
                    Material material = theme.GetItemMaterial(level);
                    
                    if (sprite != null)
                    {
                        cube.SetItemSprite(sprite);
                    }
                    
                    if (material != null)
                    {
                        cube.SetMaterial(material);
                    }
                }
            }
        }
        
        /// <summary>
        /// Toca a musica do tema
        /// </summary>
        private void PlayThemeBGM(ThemeData theme)
        {
            if (bgmSource == null || theme == null) return;
            
            if (theme.bgmTrack != null)
            {
                bgmSource.clip = theme.bgmTrack;
                bgmSource.volume = theme.bgmVolume;
                bgmSource.Play();
            }
            else
            {
                bgmSource.Stop();
            }
        }
        
        /// <summary>
        /// Para a musica do tema
        /// </summary>
        public void StopBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }
        
        /// <summary>
        /// Pausa a musica do tema
        /// </summary>
        public void PauseBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Pause();
            }
        }
        
        /// <summary>
        /// Retoma a musica do tema
        /// </summary>
        public void ResumeBGM()
        {
            if (bgmSource != null && bgmSource.clip != null)
            {
                bgmSource.UnPause();
            }
        }
        
        /// <summary>
        /// Registra um Image para ser atualizado com o tema
        /// </summary>
        public void RegisterBackgroundImage(Image image)
        {
            if (backgroundImage == null)
            {
                backgroundImage = image;
            }
        }
        
        /// <summary>
        /// Registra um SpriteRenderer para ser atualizado com o tema
        /// </summary>
        public void RegisterBackgroundRenderer(SpriteRenderer renderer)
        {
            if (!backgroundRenderers.Contains(renderer))
            {
                backgroundRenderers.Add(renderer);
            }
        }
        
        /// <summary>
        /// Registra um elemento de destaque da UI
        /// </summary>
        public void RegisterAccentElement(Image element)
        {
            if (element != null)
            {
                var list = new List<Image>(accentElements);
                if (!list.Contains(element))
                {
                    list.Add(element);
                    accentElements = list.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Registra um elemento de texto
        /// </summary>
        public void RegisterTextElement(Text text)
        {
            if (text != null)
            {
                var list = new List<Text>(textElements);
                if (!list.Contains(text))
                {
                    list.Add(text);
                    textElements = list.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Obtém o prefab de particula de fusao do tema atual
        /// </summary>
        public GameObject GetMergeParticle()
        {
            if (currentTheme != null)
            {
                return currentTheme.mergeParticlePrefab;
            }
            
            return null;
        }
        
        /// <summary>
        /// Obtém a cor de particula do tema atual
        /// </summary>
        public Color GetParticleColor()
        {
            if (currentTheme != null)
            {
                return currentTheme.particleColor;
            }
            
            return Color.white;
        }
        
        /// <summary>
        /// Define a referencia do background Image
        /// </summary>
        public void SetBackgroundImage(Image image)
        {
            backgroundImage = image;
        }
        
        /// <summary>
        /// Define a camera principal
        /// </summary>
        public void SetMainCamera(Camera camera)
        {
            mainCamera = camera;
        }
        
        /// <summary>
        /// Limpa as referencias registradas
        /// </summary>
        public void ClearReferences()
        {
            backgroundRenderers.Clear();
            accentElements = new Image[0];
            textElements = new Text[0];
            panelElements = new Image[0];
        }
    }
}
