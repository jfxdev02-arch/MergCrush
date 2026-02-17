using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MergCrush.Core
{
    /// <summary>
    /// Gerencia colisoes e deteccao de fusao entre cubos
    /// </summary>
    public class CubeCollision : MonoBehaviour
    {
        public static CubeCollision Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private Configuration config;
        
        [Header("Collision Settings")]
        [SerializeField] private LayerMask cubeLayer;
        [SerializeField] private float collisionCheckRadius = 0.5f;
        [SerializeField] private float mergeCheckDelay = 0.1f;
        
        // Estado
        private Cube selectedCube = null;
        private List<Cube> cubesToCheck = new List<Cube>();
        private bool isProcessingMerge = false;
        
        // Eventos
        public System.Action<Cube, Cube> OnMergeDetected;
        public System.Action<Cube> OnCubeLanded;
        
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
            
            // Registrar eventos
            if (GridManager.Instance != null)
            {
                GridManager.Instance.OnCubeAdded += OnCubeAdded;
            }
        }
        
        private void OnDestroy()
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.OnCubeAdded -= OnCubeAdded;
            }
        }
        
        /// <summary>
        /// Chamado quando um cubo e adicionado a grid
        /// </summary>
        private void OnCubeAdded(Vector2Int pos)
        {
            Cube cube = GridManager.Instance?.GetCube(pos);
            if (cube != null)
            {
                // Verificar fusoes possiveis apos o cubo pousar
                StartCoroutine(CheckMergeAfterLanding(cube));
            }
        }
        
        /// <summary>
        /// Verifica fusoes apos um cubo pousar
        /// </summary>
        private IEnumerator CheckMergeAfterLanding(Cube cube)
        {
            yield return new WaitForSeconds(mergeCheckDelay);
            
            if (cube == null || cube.IsMerging) yield break;
            
            CheckAndMergeAdjacent(cube);
            
            OnCubeLanded?.Invoke(cube);
        }
        
        /// <summary>
        /// Verifica e executa fusao com cubos adjacentes
        /// </summary>
        public void CheckAndMergeAdjacent(Cube cube)
        {
            if (cube == null || cube.IsMerging || isProcessingMerge) return;
            
            if (GridManager.Instance == null) return;
            
            List<Cube> adjacentCubes = GridManager.Instance.GetAdjacentCubes(cube.GridPosition);
            
            foreach (Cube adjacent in adjacentCubes)
            {
                if (adjacent != null && !adjacent.IsMerging)
                {
                    if (cube.CanMergeWith(adjacent))
                    {
                        // Detectado fusao possivel
                        OnMergeDetected?.Invoke(cube, adjacent);
                        
                        // Executar fusao
                        isProcessingMerge = true;
                        cube.MergeWith(adjacent);
                        
                        // Aguardar fusao completar
                        StartCoroutine(WaitForMergeComplete(cube));
                        return;
                    }
                }
            }
        }
        
        private IEnumerator WaitForMergeComplete(Cube cube)
        {
            while (cube != null && cube.IsMerging)
            {
                yield return null;
            }
            
            isProcessingMerge = false;
            
            // Verificar cascata de fusoes
            if (cube != null)
            {
                yield return new WaitForSeconds(mergeCheckDelay);
                CheckAndMergeAdjacent(cube);
            }
        }
        
        /// <summary>
        /// Verifica se existe movimento possivel na grid
        /// </summary>
        public bool HasValidMoves()
        {
            if (GridManager.Instance == null) return false;
            
            List<Cube> allCubes = GridManager.Instance.GetAllCubes();
            
            foreach (Cube cube in allCubes)
            {
                if (cube == null || cube.IsMerging) continue;
                
                List<Cube> adjacent = GridManager.Instance.GetAdjacentCubes(cube.GridPosition);
                
                foreach (Cube adj in adjacent)
                {
                    if (adj != null && cube.CanMergeWith(adj))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Verifica se a grid esta bloqueada (game over)
        /// </summary>
        public bool IsGridBlocked()
        {
            // Se ha espacos vazios, nao esta bloqueado
            if (GridManager.Instance != null && GridManager.Instance.GetEmptyCount() > 0)
            {
                return false;
            }
            
            // Se nao ha movimentos validos, esta bloqueado
            return !HasValidMoves();
        }
        
        /// <summary>
        /// Seleciona um cubo
        /// </summary>
        public void SelectCube(Cube cube)
        {
            if (selectedCube != null)
            {
                selectedCube.Deselect();
            }
            
            selectedCube = cube;
            
            if (cube != null)
            {
                cube.Select();
            }
        }
        
        /// <summary>
        /// Tenta mover o cubo selecionado para uma posicao
        /// </summary>
        public bool TryMoveSelectedCube(Vector2Int targetPos)
        {
            if (selectedCube == null) return false;
            if (GridManager.Instance == null) return false;
            
            // Verificar se a posicao e valida
            if (!GridManager.Instance.IsValidPosition(targetPos)) return false;
            
            // Verificar se ha cubo na posicao alvo
            Cube targetCube = GridManager.Instance.GetCube(targetPos);
            
            if (targetCube != null)
            {
                // Tentar fundir
                if (selectedCube.CanMergeWith(targetCube))
                {
                    GridManager.Instance.RemoveCube(selectedCube.GridPosition);
                    selectedCube.MergeWith(targetCube);
                    SelectCube(null);
                    return true;
                }
                else
                {
                    // Nao pode mover para la
                    return false;
                }
            }
            else
            {
                // Mover para posicao vazia
                GridManager.Instance.MoveCube(selectedCube.GridPosition, targetPos);
                selectedCube.MoveToGridPosition(targetPos);
                SelectCube(null);
                return true;
            }
        }
        
        /// <summary>
        /// Encontra todos os cubos que podem ser fundidos com um cubo especifico
        /// </summary>
        public List<Cube> FindMergeableCubes(Cube cube)
        {
            List<Cube> result = new List<Cube>();
            
            if (cube == null || GridManager.Instance == null) return result;
            
            List<Cube> adjacent = GridManager.Instance.GetAdjacentCubes(cube.GridPosition);
            
            foreach (Cube adj in adjacent)
            {
                if (adj != null && cube.CanMergeWith(adj))
                {
                    result.Add(adj);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Executa fusao em cadeia (combo)
        /// </summary>
        public IEnumerator ExecuteComboMerge(Cube startCube)
        {
            List<Cube> toMerge = FindMergeableCubes(startCube);
            
            if (toMerge.Count > 0)
            {
                // Fundir com o primeiro disponivel
                Cube target = toMerge[0];
                
                GridManager.Instance.RemoveCube(startCube.GridPosition);
                startCube.MergeWith(target);
                
                while (startCube != null && startCube.IsMerging)
                {
                    yield return null;
                }
                
                // Verificar cascata
                if (startCube != null)
                {
                    yield return new WaitForSeconds(mergeCheckDelay);
                    yield return ExecuteComboMerge(startCube);
                }
            }
        }
        
        /// <summary>
        /// Verifica colisao por trigger
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            CheckCollision(other.gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            CheckCollision(other.gameObject);
        }
        
        private void CheckCollision(GameObject otherObj)
        {
            Cube otherCube = otherObj.GetComponent<Cube>();
            
            if (otherCube != null && !otherCube.IsMerging)
            {
                cubesToCheck.Add(otherCube);
            }
        }
        
        /// <summary>
        /// Configura o CubeCollision
        /// </summary>
        public void SetConfig(Configuration newConfig)
        {
            config = newConfig;
        }
    }
}
