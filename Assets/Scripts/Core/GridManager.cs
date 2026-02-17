using UnityEngine;
using System.Collections.Generic;

namespace MergCrush.Core
{
    /// <summary>
    /// Gerencia a grid de cubos do jogo
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private Configuration config;
        
        [Header("Grid State")]
        [SerializeField] private Transform gridContainer;
        
        // Matriz de cubos [x, y]
        private Cube[,] cubes;
        
        // Lista de posicoes vazias
        private List<Vector2Int> emptyPositions = new List<Vector2Int>();
        
        // Eventos
        public System.Action<Vector2Int> OnCubeAdded;
        public System.Action<Vector2Int> OnCubeRemoved;
        public System.Action OnGridFull;
        
        public int GridWidth => config != null ? config.gridWidth : 9;
        public int GridHeight => config != null ? config.gridHeight : 9;
        public float CubeSpacing => config != null ? config.cubeSpacing : 1.1f;
        
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
            InitializeGrid();
        }
        
        /// <summary>
        /// Inicializa a grid de cubos
        /// </summary>
        public void InitializeGrid()
        {
            if (config == null)
            {
                config = Resources.Load<Configuration>("GameConfiguration");
                if (config == null)
                {
                    Debug.LogError("Configuration nao encontrado!");
                    return;
                }
            }
            
            cubes = new Cube[GridWidth, GridHeight];
            emptyPositions.Clear();
            
            // Criar container se nao existir
            if (gridContainer == null)
            {
                GameObject container = new GameObject("GridContainer");
                container.transform.SetParent(transform);
                gridContainer = container.transform;
            }
            
            // Inicializar lista de posicoes vazias
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    emptyPositions.Add(new Vector2Int(x, y));
                }
            }
            
            // Centralizar grid na tela
            CenterGrid();
            
            Debug.Log($"Grid inicializada: {GridWidth}x{GridHeight}");
        }
        
        /// <summary>
        /// Centraliza a grid na posicao do mundo
        /// </summary>
        private void CenterGrid()
        {
            float offsetX = (GridWidth - 1) * CubeSpacing / 2f;
            float offsetY = (GridHeight - 1) * CubeSpacing / 2f;
            
            gridContainer.localPosition = new Vector3(-offsetX, -offsetY, 0);
        }
        
        /// <summary>
        /// Converte posicao da grid para posicao do mundo
        /// </summary>
        public Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(
                gridPos.x * CubeSpacing,
                gridPos.y * CubeSpacing,
                0
            );
        }
        
        /// <summary>
        /// Converte posicao do mundo para posicao da grid
        /// </summary>
        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - gridContainer.position;
            
            int x = Mathf.RoundToInt(localPos.x / CubeSpacing);
            int y = Mathf.RoundToInt(localPos.y / CubeSpacing);
            
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Verifica se a posicao esta dentro da grid
        /// </summary>
        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < GridWidth && pos.y >= 0 && pos.y < GridHeight;
        }
        
        /// <summary>
        /// Verifica se a posicao esta vazia
        /// </summary>
        public bool IsEmpty(Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return false;
            return cubes[pos.x, pos.y] == null;
        }
        
        /// <summary>
        /// Obtem o cubo em uma posicao
        /// </summary>
        public Cube GetCube(Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return null;
            return cubes[pos.x, pos.y];
        }
        
        /// <summary>
        /// Adiciona um cubo na grid
        /// </summary>
        public bool AddCube(Cube cube, Vector2Int pos)
        {
            if (!IsEmpty(pos))
            {
                Debug.LogWarning($"Posicao {pos} nao esta vazia!");
                return false;
            }
            
            cubes[pos.x, pos.y] = cube;
            cube.GridPosition = pos;
            emptyPositions.Remove(pos);
            
            // Atualizar posicao visual
            cube.transform.SetParent(gridContainer);
            cube.transform.localPosition = GridToWorldPosition(pos);
            
            OnCubeAdded?.Invoke(pos);
            
            // Verificar se grid esta cheia
            if (emptyPositions.Count == 0)
            {
                OnGridFull?.Invoke();
            }
            
            return true;
        }
        
        /// <summary>
        /// Remove um cubo da grid
        /// </summary>
        public void RemoveCube(Vector2Int pos)
        {
            if (!IsValidPosition(pos)) return;
            
            cubes[pos.x, pos.y] = null;
            
            if (!emptyPositions.Contains(pos))
            {
                emptyPositions.Add(pos);
            }
            
            OnCubeRemoved?.Invoke(pos);
        }
        
        /// <summary>
        /// Move um cubo para uma nova posicao
        /// </summary>
        public bool MoveCube(Vector2Int from, Vector2Int to)
        {
            if (!IsValidPosition(from) || !IsValidPosition(to)) return false;
            if (IsEmpty(from)) return false;
            if (!IsEmpty(to)) return false;
            
            Cube cube = cubes[from.x, from.y];
            cubes[from.x, from.y] = null;
            cubes[to.x, to.y] = cube;
            
            cube.GridPosition = to;
            
            emptyPositions.Add(from);
            emptyPositions.Remove(to);
            
            return true;
        }
        
        /// <summary>
        /// Retorna uma posicao vazia aleatoria
        /// </summary>
        public Vector2Int? GetRandomEmptyPosition()
        {
            if (emptyPositions.Count == 0) return null;
            
            int index = Random.Range(0, emptyPositions.Count);
            return emptyPositions[index];
        }
        
        /// <summary>
        /// Retorna o numero de posicoes vazias
        /// </summary>
        public int GetEmptyCount()
        {
            return emptyPositions.Count;
        }
        
        /// <summary>
        /// Retorna todos os cubos da grid
        /// </summary>
        public List<Cube> GetAllCubes()
        {
            List<Cube> result = new List<Cube>();
            
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (cubes[x, y] != null)
                    {
                        result.Add(cubes[x, y]);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Retorna cubos adjacentes a uma posicao
        /// </summary>
        public List<Cube> GetAdjacentCubes(Vector2Int pos)
        {
            List<Cube> result = new List<Cube>();
            
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int adjacentPos = pos + dir;
                Cube cube = GetCube(adjacentPos);
                if (cube != null)
                {
                    result.Add(cube);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Limpa toda a grid
        /// </summary>
        public void ClearGrid()
        {
            if (cubes == null) return;
            
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (cubes[x, y] != null)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(cubes[x, y].gameObject);
                        }
                        else
                        {
                            DestroyImmediate(cubes[x, y].gameObject);
                        }
                        cubes[x, y] = null;
                    }
                }
            }
            
            emptyPositions.Clear();
            
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    emptyPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        /// <summary>
        /// Configura o GridManager
        /// </summary>
        public void SetConfig(Configuration newConfig)
        {
            config = newConfig;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (config == null) return;
            
            Gizmos.color = Color.green * 0.3f;
            
            Vector3 center = transform.position;
            if (gridContainer != null)
            {
                center = gridContainer.position;
            }
            
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    Vector3 pos = center + new Vector3(x * CubeSpacing, y * CubeSpacing, 0);
                    Gizmos.DrawWireCube(pos, Vector3.one * config.cubeSize * 0.9f);
                }
            }
        }
        #endif
    }
}
