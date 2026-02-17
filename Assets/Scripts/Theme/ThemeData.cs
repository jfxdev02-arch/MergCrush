using UnityEngine;

namespace MergCrush.Theme
{
    /// <summary>
    /// Dados de configuracao de um tema do jogo
    /// Cada tema representa uma fase com itens visuais especificos
    /// </summary>
    [CreateAssetMenu(fileName = "NewTheme", menuName = "MergCrush/Theme Data")]
    public class ThemeData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Nome do tema exibido na UI")]
        public string themeName = "Novo Tema";
        
        [Tooltip("Descricao do tema")]
        [TextArea(2, 4)]
        public string description = "";
        
        [Tooltip("Icone do tema para tela de selecao")]
        public Sprite themeIcon;
        
        [Header("Item Sprites")]
        [Tooltip("Sprites dos 6 itens do tema (equivalente aos valores 2, 4, 8, 16, 32, 64)")]
        public Sprite[] itemSprites = new Sprite[6];
        
        [Tooltip("Nomes dos itens para debug/display")]
        public string[] itemNames = new string[6] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5", "Item 6" };
        
        [Header("3D Materials")]
        [Tooltip("Materials 3D para os cubos (opcional)")]
        public Material[] itemMaterials = new Material[6];
        
        [Tooltip("Material base para cubos sem material especifico")]
        public Material baseCubeMaterial;
        
        [Header("Background")]
        [Tooltip("Sprite de background da fase")]
        public Sprite backgroundSprite;
        
        [Tooltip("Cor de fundo se nao houver sprite")]
        public Color backgroundColor = new Color(0.2f, 0.3f, 0.4f);
        
        [Header("UI Colors")]
        [Tooltip("Cor de destaque para UI do tema")]
        public Color uiAccentColor = new Color(1f, 0.5f, 0.2f);
        
        [Tooltip("Cor secundaria da UI")]
        public Color uiSecondaryColor = new Color(0.9f, 0.9f, 0.9f);
        
        [Tooltip("Cor do texto")]
        public Color uiTextColor = Color.white;
        
        [Header("Level Settings")]
        [Tooltip("Pontuacao necessaria para completar a fase")]
        public int levelTargetScore = 1000;
        
        [Tooltip("Dificuldade do nivel (1-5, afeta spawn de itens)")]
        [Range(1, 5)]
        public int levelDifficulty = 1;
        
        [Tooltip("Numero de estrelas para cada faixa de pontuacao (0-100%, 100-150%, 150%+)")]
        public int[] starThresholds = new int[3] { 100, 150, 200 };
        
        [Header("Audio")]
        [Tooltip("Musica de fundo do tema")]
        public AudioClip bgmTrack;
        
        [Tooltip("Volume da musica (0-1)")]
        [Range(0f, 1f)]
        public float bgmVolume = 0.5f;
        
        [Header("Particles")]
        [Tooltip("Prefab de particula para fusao neste tema")]
        public GameObject mergeParticlePrefab;
        
        [Tooltip("Cor das particulas")]
        public Color particleColor = Color.white;
        
        /// <summary>
        /// Retorna o sprite do item pelo nivel (1-6)
        /// </summary>
        public Sprite GetItemSprite(int level)
        {
            int index = level - 1;
            
            if (itemSprites == null || index < 0 || index >= itemSprites.Length)
            {
                return null;
            }
            
            return itemSprites[index];
        }
        
        /// <summary>
        /// Retorna o material do item pelo nivel (1-6)
        /// </summary>
        public Material GetItemMaterial(int level)
        {
            int index = level - 1;
            
            if (itemMaterials == null || index < 0 || index >= itemMaterials.Length)
            {
                return baseCubeMaterial;
            }
            
            return itemMaterials[index] != null ? itemMaterials[index] : baseCubeMaterial;
        }
        
        /// <summary>
        /// Retorna o nome do item pelo nivel
        /// </summary>
        public string GetItemName(int level)
        {
            int index = level - 1;
            
            if (itemNames == null || index < 0 || index >= itemNames.Length)
            {
                return $"Item {level}";
            }
            
            return itemNames[index];
        }
        
        /// <summary>
        /// Calcula quantas estrelas o jogador ganhou baseado na pontuacao
        /// </summary>
        public int CalculateStars(int score)
        {
            float percentage = (float)score / levelTargetScore * 100f;
            
            if (percentage >= starThresholds[2]) return 3;
            if (percentage >= starThresholds[1]) return 2;
            if (percentage >= starThresholds[0]) return 1;
            
            return 0;
        }
        
        /// <summary>
        /// Valida se o tema tem todos os dados necessarios
        /// </summary>
        public bool IsValid()
        {
            if (itemSprites == null || itemSprites.Length < 6)
            {
                Debug.LogWarning($"Tema {themeName}: Faltam sprites de itens!");
                return false;
            }
            
            int validSprites = 0;
            foreach (var sprite in itemSprites)
            {
                if (sprite != null) validSprites++;
            }
            
            if (validSprites < 6)
            {
                Debug.LogWarning($"Tema {themeName}: Apenas {validSprites}/6 sprites configurados!");
            }
            
            return validSprites >= 6;
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Garantir que os arrays tenham tamanho correto
            if (itemSprites == null || itemSprites.Length != 6)
            {
                itemSprites = new Sprite[6];
            }
            
            if (itemNames == null || itemNames.Length != 6)
            {
                itemNames = new string[6] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5", "Item 6" };
            }
            
            if (itemMaterials == null || itemMaterials.Length != 6)
            {
                itemMaterials = new Material[6];
            }
            
            if (starThresholds == null || starThresholds.Length != 3)
            {
                starThresholds = new int[3] { 100, 150, 200 };
            }
        }
        #endif
    }
}
