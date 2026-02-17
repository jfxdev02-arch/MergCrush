using UnityEngine;
using MergCrush.Theme;

namespace MergCrush.Core
{
    /// <summary>
    /// Configuracoes globais do jogo MergCrush
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "MergCrush/Configuration")]
    public class Configuration : ScriptableObject
    {
        [Header("Grid Settings")]
        [Tooltip("Largura da grid de cubos")]
        public int gridWidth = 9;
        
        [Tooltip("Altura da grid de cubos")]
        public int gridHeight = 9;
        
        [Tooltip("Espacamento entre cubos")]
        public float cubeSpacing = 1.1f;
        
        [Header("Cube Settings")]
        [Tooltip("Tamanho do cubo")]
        public float cubeSize = 1f;
        
        [Tooltip("Velocidade de queda dos cubos")]
        public float fallSpeed = 8f;
        
        [Tooltip("Velocidade de movimento horizontal")]
        public float moveSpeed = 10f;
        
        [Tooltip("Tempo de animacao de fusao")]
        public float mergeAnimationDuration = 0.3f;
        
        [Header("Gameplay Settings")]
        [Tooltip("Numero maximo de itens diferentes (equivalente a 2,4,8,16,32,64)")]
        public int maxItemTypes = 6;
        
        [Tooltip("Pontos base por fusao")]
        public int baseMergePoints = 10;
        
        [Tooltip("Multiplicador de pontos por nivel do item")]
        public float pointMultiplier = 1.5f;
        
        [Header("Spawn Settings")]
        [Tooltip("Intervalo entre spawns de novos cubos")]
        public float spawnInterval = 0.5f;
        
        [Tooltip("Numero de cubos spawnados por vez")]
        public int cubesPerSpawn = 1;
        
        [Header("Visual Settings")]
        [Tooltip("Prefab do cubo 3D")]
        public GameObject cubePrefab;
        
        [Tooltip("Material base para os cubos")]
        public Material baseCubeMaterial;
        
        [Header("Level System")]
        [Tooltip("Lista de temas disponiveis")]
        public ThemeData[] themes;
        
        [Tooltip("Nivel inicial (indice do tema)")]
        public int startingLevel = 0;
        
        /// <summary>
        /// Retorna o tema pelo indice
        /// </summary>
        public ThemeData GetTheme(int index)
        {
            if (themes == null || themes.Length == 0)
            {
                Debug.LogError("Nenhum tema configurado!");
                return null;
            }
            
            if (index < 0 || index >= themes.Length)
            {
                Debug.LogWarning($"Indice de tema invalido: {index}. Usando tema 0.");
                return themes[0];
            }
            
            return themes[index];
        }
        
        /// <summary>
        /// Retorna o numero total de niveis/temas
        /// </summary>
        public int GetTotalLevels()
        {
            return themes != null ? themes.Length : 0;
        }
    }
}
