using UnityEngine;
using System.Collections;
using MergCrush.Theme;

namespace MergCrush.Core
{
    /// <summary>
    /// Responsavel por spawnar cubos na grid
    /// </summary>
    public class CubeSpawner : MonoBehaviour
    {
        public static CubeSpawner Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private Configuration config;
        [SerializeField] private GameObject cubePrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool autoSpawn = false;
        [SerializeField] private float initialSpawnDelay = 1f;
        
        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 50;
        [SerializeField] private bool useObjectPool = true;
        
        // Pool de cubos
        private Queue<Cube> cubePool = new Queue<Cube>();
        
        // Estado
        private bool isSpawning = false;
        private int cubesSpawnedThisGame = 0;
        
        // Eventos
        public System.Action<Cube> OnCubeSpawned;
        public System.Action OnSpawnComplete;
        
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
            if (config == null)
            {
                config = Resources.Load<Configuration>("GameConfiguration");
            }
            
            if (useObjectPool)
            {
                InitializePool();
            }
            
            if (autoSpawn)
            {
                StartCoroutine(AutoSpawnRoutine());
            }
        }
        
        /// <summary>
        /// Inicializa o pool de objetos
        /// </summary>
        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject cubeObj = CreateNewCube();
                cubeObj.SetActive(false);
                cubeObj.transform.SetParent(transform);
                
                Cube cube = cubeObj.GetComponent<Cube>();
                if (cube != null)
                {
                    cubePool.Enqueue(cube);
                }
            }
            
            Debug.Log($"Pool inicializado com {poolSize} cubos");
        }
        
        /// <summary>
        /// Cria um novo cubo
        /// </summary>
        private GameObject CreateNewCube()
        {
            GameObject cubeObj;
            
            if (cubePrefab != null)
            {
                cubeObj = Instantiate(cubePrefab);
            }
            else
            {
                cubeObj = CreateDefaultCube();
            }
            
            return cubeObj;
        }
        
        /// <summary>
        /// Cria um cubo padrao se nao houver prefab
        /// </summary>
        private GameObject CreateDefaultCube()
        {
            GameObject cubeObj = new GameObject("Cube");
            
            // Adicionar Mesh
            MeshFilter meshFilter = cubeObj.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateBoxMesh();
            
            // Adicionar Renderer
            MeshRenderer renderer = cubeObj.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Adicionar Collider
            BoxCollider collider = cubeObj.AddComponent<BoxCollider>();
            
            // Adicionar Rigidbody
            Rigidbody rb = cubeObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            
            // Criar filho para sprite do item
            GameObject spriteObj = new GameObject("ItemSprite");
            spriteObj.transform.SetParent(cubeObj.transform);
            spriteObj.transform.localPosition = new Vector3(0, 0, -0.51f);
            
            SpriteRenderer spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
            
            // Adicionar componente Cube
            Cube cube = cubeObj.AddComponent<Cube>();
            
            return cubeObj;
        }
        
        /// <summary>
        /// Cria uma mesh de cubo simples
        /// </summary>
        private Mesh CreateBoxMesh()
        {
            Mesh mesh = new Mesh();
            
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f)
            };
            
            int[] triangles = new int[]
            {
                0, 2, 1, 0, 3, 2, // Front
                4, 5, 6, 4, 6, 7, // Back
                0, 1, 5, 0, 5, 4, // Bottom
                2, 3, 7, 2, 7, 6, // Top
                0, 4, 7, 0, 7, 3, // Left
                1, 2, 6, 1, 6, 5  // Right
            };
            
            Vector2[] uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// Obtem um cubo do pool ou cria um novo
        /// </summary>
        private Cube GetCubeFromPool()
        {
            if (useObjectPool && cubePool.Count > 0)
            {
                Cube cube = cubePool.Dequeue();
                cube.gameObject.SetActive(true);
                return cube;
            }
            
            GameObject cubeObj = CreateNewCube();
            return cubeObj.GetComponent<Cube>();
        }
        
        /// <summary>
        /// Devolve um cubo ao pool
        /// </summary>
        public void ReturnCubeToPool(Cube cube)
        {
            if (!useObjectPool)
            {
                Destroy(cube.gameObject);
                return;
            }
            
            cube.gameObject.SetActive(false);
            cube.transform.SetParent(transform);
            cubePool.Enqueue(cube);
        }
        
        /// <summary>
        /// Spawna um cubo em uma posicao especifica
        /// </summary>
        public Cube SpawnCube(Vector2Int gridPos, int? itemLevel = null)
        {
            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager nao encontrado!");
                return null;
            }
            
            if (!GridManager.Instance.IsEmpty(gridPos))
            {
                Debug.LogWarning($"Posicao {gridPos} nao esta vazia!");
                return null;
            }
            
            // Obter cubo
            Cube cube = GetCubeFromPool();
            
            if (cube == null)
            {
                Debug.LogError("Falha ao criar cubo!");
                return null;
            }
            
            // Definir nivel do item
            int level = itemLevel ?? GenerateRandomItemLevel();
            
            // Obter sprite do tema atual
            Sprite itemSprite = GetItemSprite(level);
            Material itemMaterial = GetItemMaterial(level);
            
            // Inicializar cubo
            cube.Initialize(level, itemSprite);
            cube.SetConfig(config);
            
            if (itemMaterial != null)
            {
                cube.SetMaterial(itemMaterial);
            }
            
            // Adicionar a grid
            GridManager.Instance.AddCube(cube, gridPos);
            
            // Animacao de spawn
            cube.PlaySpawnAnimation();
            
            cubesSpawnedThisGame++;
            
            OnCubeSpawned?.Invoke(cube);
            
            return cube;
        }
        
        /// <summary>
        /// Spawna um cubo em uma posicao aleatoria
        /// </summary>
        public Cube SpawnRandomCube()
        {
            Vector2Int? emptyPos = GridManager.Instance?.GetRandomEmptyPosition();
            
            if (!emptyPos.HasValue)
            {
                Debug.LogWarning("Nenhuma posicao vazia disponivel!");
                return null;
            }
            
            return SpawnCube(emptyPos.Value);
        }
        
        /// <summary>
        /// Spawna multiplos cubos aleatorios
        /// </summary>
        public void SpawnMultipleCubes(int count)
        {
            StartCoroutine(SpawnMultipleRoutine(count));
        }
        
        private IEnumerator SpawnMultipleRoutine(int count)
        {
            int spawned = 0;
            float interval = config != null ? config.spawnInterval : 0.5f;
            
            while (spawned < count)
            {
                Cube cube = SpawnRandomCube();
                
                if (cube != null)
                {
                    spawned++;
                    yield return new WaitForSeconds(interval);
                }
                else
                {
                    // Nao ha mais espaco
                    break;
                }
            }
            
            OnSpawnComplete?.Invoke();
        }
        
        /// <summary>
        /// Gera um nivel de item aleatorio baseado na dificuldade
        /// </summary>
        private int GenerateRandomItemLevel()
        {
            int difficulty = 1;
            
            // Obter dificuldade do tema atual
            ThemeManager themeManager = ThemeManager.Instance;
            if (themeManager != null && themeManager.CurrentTheme != null)
            {
                difficulty = themeManager.CurrentTheme.levelDifficulty;
            }
            
            // Maximo de tipos baseado na dificuldade
            int maxTypes = Mathf.Min(difficulty + 2, config?.maxItemTypes ?? 6);
            
            // Probabilidade ponderada (itens de nivel menor sao mais comuns)
            int totalWeight = 0;
            for (int i = 1; i <= maxTypes; i++)
            {
                totalWeight += (maxTypes - i + 1);
            }
            
            int random = Random.Range(0, totalWeight);
            int cumulative = 0;
            
            for (int i = 1; i <= maxTypes; i++)
            {
                cumulative += (maxTypes - i + 1);
                if (random < cumulative)
                {
                    return i;
                }
            }
            
            return 1;
        }
        
        /// <summary>
        /// Obtem o sprite do item para um nivel
        /// </summary>
        private Sprite GetItemSprite(int level)
        {
            ThemeManager themeManager = ThemeManager.Instance;
            
            if (themeManager != null && themeManager.CurrentTheme != null)
            {
                var theme = themeManager.CurrentTheme;
                
                if (theme.itemSprites != null && level >= 1 && level <= theme.itemSprites.Length)
                {
                    return theme.itemSprites[level - 1];
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Obtem o material do item para um nivel
        /// </summary>
        private Material GetItemMaterial(int level)
        {
            ThemeManager themeManager = ThemeManager.Instance;
            
            if (themeManager != null && themeManager.CurrentTheme != null)
            {
                var theme = themeManager.CurrentTheme;
                
                if (theme.itemMaterials != null && level >= 1 && level <= theme.itemMaterials.Length)
                {
                    return theme.itemMaterials[level - 1];
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Inicia o spawn automatico
        /// </summary>
        public void StartAutoSpawn()
        {
            autoSpawn = true;
            StartCoroutine(AutoSpawnRoutine());
        }
        
        /// <summary>
        /// Para o spawn automatico
        /// </summary>
        public void StopAutoSpawn()
        {
            autoSpawn = false;
        }
        
        private IEnumerator AutoSpawnRoutine()
        {
            yield return new WaitForSeconds(initialSpawnDelay);
            
            while (autoSpawn)
            {
                float interval = config != null ? config.spawnInterval : 0.5f;
                int count = config != null ? config.cubesPerSpawn : 1;
                
                for (int i = 0; i < count; i++)
                {
                    SpawnRandomCube();
                }
                
                yield return new WaitForSeconds(interval);
            }
        }
        
        /// <summary>
        /// Spawna cubos iniciais para comecar o jogo
        /// </summary>
        public void SpawnInitialCubes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnRandomCube();
            }
        }
        
        /// <summary>
        /// Reseta o contador de cubos spawned
        /// </summary>
        public void ResetSpawnCount()
        {
            cubesSpawnedThisGame = 0;
        }
        
        /// <summary>
        /// Configura o CubeSpawner
        /// </summary>
        public void SetConfig(Configuration newConfig)
        {
            config = newConfig;
        }
        
        public void SetCubePrefab(GameObject prefab)
        {
            cubePrefab = prefab;
        }
    }
}
