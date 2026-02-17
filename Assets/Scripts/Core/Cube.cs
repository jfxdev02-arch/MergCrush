using UnityEngine;
using System.Collections;
using MergCrush.Theme;
using MergCrush.Level;

namespace MergCrush.Core
{
    /// <summary>
    /// Representa um cubo/item tematico na grid
    /// </summary>
    public class Cube : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private SpriteRenderer itemSpriteRenderer;
        [SerializeField] private Collider cubeCollider;
        [SerializeField] private Rigidbody rb;
        
        [Header("Visual Settings")]
        [SerializeField] private float pulseScale = 1.1f;
        [SerializeField] private float pulseDuration = 0.2f;
        [SerializeField] private AnimationCurve mergeCurve;
        
        // Estado do cubo
        private int itemLevel = 1; // 1 = 2, 2 = 4, 3 = 8, etc.
        private Vector2Int gridPosition;
        private bool isMerging = false;
        private bool isMoving = false;
        private bool isSelected = false;
        
        // Referencias
        private Configuration config;
        private Sprite currentItemSprite;
        
        // Eventos
        public System.Action<Cube> OnMergeComplete;
        public System.Action<Cube> OnSelected;
        public System.Action<Cube> OnDeselected;
        
        // Propriedades
        public int ItemLevel => itemLevel;
        public int ItemValue => (int)Mathf.Pow(2, itemLevel);
        public Vector2Int GridPosition
        {
            get => gridPosition;
            set => gridPosition = value;
        }
        public bool IsMerging => isMerging;
        public bool IsMoving => isMoving;
        public bool IsSelected => isSelected;
        
        private void Awake()
        {
            // Garantir componentes
            if (meshRenderer == null)
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (cubeCollider == null)
                cubeCollider = GetComponent<Collider>();
            if (rb == null)
                rb = GetComponent<Rigidbody>();
                
            // Criar curva de animacao se nao existir
            if (mergeCurve == null || mergeCurve.length == 0)
            {
                mergeCurve = new AnimationCurve(
                    new Keyframe(0, 1f),
                    new Keyframe(0.5f, 1.3f),
                    new Keyframe(1, 0f)
                );
            }
        }
        
        /// <summary>
        /// Inicializa o cubo com um nivel de item
        /// </summary>
        public void Initialize(int level, Sprite sprite = null)
        {
            itemLevel = level;
            currentItemSprite = sprite;
            
            if (itemSpriteRenderer != null && sprite != null)
            {
                itemSpriteRenderer.sprite = sprite;
            }
            
            // Atualizar nome para debug
            gameObject.name = $"Cube_{ItemValue}";
        }
        
        /// <summary>
        /// Define o sprite do item tematico
        /// </summary>
        public void SetItemSprite(Sprite sprite)
        {
            currentItemSprite = sprite;
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.sprite = sprite;
            }
        }
        
        /// <summary>
        /// Define o material do cubo 3D
        /// </summary>
        public void SetMaterial(Material material)
        {
            if (meshRenderer != null && material != null)
            {
                meshRenderer.material = material;
            }
        }
        
        /// <summary>
        /// Define a cor do cubo
        /// </summary>
        public void SetColor(Color color)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = color;
            }
        }
        
        /// <summary>
        /// Tenta fundir este cubo com outro
        /// </summary>
        public bool CanMergeWith(Cube other)
        {
            if (other == null) return false;
            if (isMerging || other.isMerging) return false;
            if (itemLevel != other.itemLevel) return false;
            
            // Verificar nivel maximo
            if (config == null)
            {
                config = Resources.Load<Configuration>("GameConfiguration");
            }
            
            int maxLevel = config != null ? config.maxItemTypes : 6;
            if (itemLevel >= maxLevel) return false;
            
            return true;
        }
        
        /// <summary>
        /// Executa a fusao com outro cubo
        /// </summary>
        public void MergeWith(Cube other)
        {
            if (!CanMergeWith(other)) return;
            
            StartCoroutine(MergeCoroutine(other));
        }
        
        private IEnumerator MergeCoroutine(Cube other)
        {
            isMerging = true;
            other.isMerging = true;
            
            // Desativar colisores durante fusao
            if (cubeCollider != null) cubeCollider.enabled = false;
            if (other.cubeCollider != null) other.cubeCollider.enabled = false;
            
            // Animacao de fusao
            float duration = config != null ? config.mergeAnimationDuration : 0.3f;
            float elapsed = 0f;
            
            Vector3 startPos = transform.position;
            Vector3 targetPos = other.transform.position;
            Vector3 startScale = transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curveValue = mergeCurve.Evaluate(t);
                
                // Mover em direcao ao outro cubo
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                
                // Efeito de escala
                transform.localScale = startScale * curveValue;
                
                // O outro cubo tambem diminui
                if (other != null)
                {
                    other.transform.localScale = startScale * (1f - t);
                }
                
                yield return null;
            }
            
            // Destruir o outro cubo
            if (other != null)
            {
                GridManager.Instance?.RemoveCube(other.GridPosition);
                Destroy(other.gameObject);
            }
            
            // Aumentar nivel deste cubo
            itemLevel++;
            
            // Atualizar sprite para o novo nivel
            UpdateItemAfterMerge();
            
            // Restaurar escala
            transform.localScale = startScale;
            
            // Reativar colisor
            if (cubeCollider != null) cubeCollider.enabled = true;
            
            isMerging = false;
            
            // Notificar conclusao
            OnMergeComplete?.Invoke(this);
            
            // Adicionar pontos
            ScoreManager.Instance?.AddMergeScore(this);
            
            Debug.Log($"Fusao completa! Novo valor: {ItemValue}");
        }
        
        /// <summary>
        /// Atualiza o item apos fusao (busca novo sprite do tema)
        /// </summary>
        private void UpdateItemAfterMerge()
        {
            ThemeManager themeManager = ThemeManager.Instance;
            if (themeManager != null && themeManager.CurrentTheme != null)
            {
                var theme = themeManager.CurrentTheme;
                if (theme.itemSprites != null && itemLevel <= theme.itemSprites.Length)
                {
                    SetItemSprite(theme.itemSprites[itemLevel - 1]);
                }
                
                if (theme.itemMaterials != null && itemLevel <= theme.itemMaterials.Length)
                {
                    SetMaterial(theme.itemMaterials[itemLevel - 1]);
                }
            }
            
            // Atualizar nome
            gameObject.name = $"Cube_{ItemValue}";
        }
        
        /// <summary>
        /// Move o cubo para uma posicao da grid
        /// </summary>
        public void MoveToGridPosition(Vector2Int newPos, bool animate = true)
        {
            if (GridManager.Instance == null) return;
            
            Vector3 targetWorldPos = GridManager.Instance.GridToWorldPosition(newPos);
            
            if (animate)
            {
                StartCoroutine(MoveCoroutine(targetWorldPos, newPos));
            }
            else
            {
                transform.localPosition = targetWorldPos;
                gridPosition = newPos;
            }
        }
        
        private IEnumerator MoveCoroutine(Vector3 targetPos, Vector2Int newGridPos)
        {
            isMoving = true;
            
            float speed = config != null ? config.moveSpeed : 10f;
            Vector3 startPos = transform.localPosition;
            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / speed;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            
            transform.localPosition = targetPos;
            gridPosition = newGridPos;
            isMoving = false;
        }
        
        /// <summary>
        /// Faz o cubo cair ate encontrar outro cubo ou o chao
        /// </summary>
        public void Fall()
        {
            StartCoroutine(FallCoroutine());
        }
        
        private IEnumerator FallCoroutine()
        {
            isMoving = true;
            
            float speed = config != null ? config.fallSpeed : 8f;
            
            while (true)
            {
                Vector2Int belowPos = new Vector2Int(gridPosition.x, gridPosition.y - 1);
                
                // Verificar se pode cair
                if (belowPos.y < 0)
                {
                    // Chegou no chao
                    break;
                }
                
                if (GridManager.Instance != null && !GridManager.Instance.IsEmpty(belowPos))
                {
                    // Ha um cubo abaixo
                    Cube belowCube = GridManager.Instance.GetCube(belowPos);
                    
                    // Verificar se pode fundir
                    if (CanMergeWith(belowCube))
                    {
                        GridManager.Instance.RemoveCube(gridPosition);
                        MergeWith(belowCube);
                        break;
                    }
                    else
                    {
                        // Nao pode fundir, parar de cair
                        break;
                    }
                }
                
                // Mover para baixo
                GridManager.Instance?.MoveCube(gridPosition, belowPos);
                gridPosition = belowPos;
                
                // Animar movimento
                Vector3 targetPos = GridManager.Instance.GridToWorldPosition(gridPosition);
                float step = speed * Time.deltaTime;
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, step);
                
                yield return null;
            }
            
            isMoving = false;
        }
        
        /// <summary>
        /// Seleciona o cubo
        /// </summary>
        public void Select()
        {
            if (isSelected) return;
            
            isSelected = true;
            StartCoroutine(PulseAnimation());
            OnSelected?.Invoke(this);
        }
        
        /// <summary>
        /// Deseleciona o cubo
        /// </summary>
        public void Deselect()
        {
            if (!isSelected) return;
            
            isSelected = false;
            transform.localScale = Vector3.one;
            OnDeselected?.Invoke(this);
        }
        
        private IEnumerator PulseAnimation()
        {
            Vector3 originalScale = Vector3.one;
            Vector3 targetScale = Vector3.one * pulseScale;
            
            float elapsed = 0f;
            while (elapsed < pulseDuration && isSelected)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                float pingPong = Mathf.PingPong(t * 2, 1);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, pingPong);
                yield return null;
            }
            
            if (isSelected)
            {
                // Loop da animacao de selecao
                while (isSelected)
                {
                    elapsed = 0f;
                    while (elapsed < pulseDuration)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / pulseDuration;
                        float pingPong = Mathf.PingPong(t * 2, 1);
                        transform.localScale = Vector3.Lerp(originalScale, targetScale, pingPong);
                        yield return null;
                    }
                }
            }
        }
        
        /// <summary>
        /// Animacao de spawn
        /// </summary>
        public void PlaySpawnAnimation()
        {
            StartCoroutine(SpawnAnimationCoroutine());
        }
        
        private IEnumerator SpawnAnimationCoroutine()
        {
            transform.localScale = Vector3.zero;
            
            float duration = 0.2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float bounce = 1 + Mathf.Sin(t * Mathf.PI) * 0.2f;
                transform.localScale = Vector3.one * bounce * t;
                yield return null;
            }
            
            transform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Animacao de destruicao
        /// </summary>
        public void PlayDestroyAnimation()
        {
            StartCoroutine(DestroyAnimationCoroutine());
        }
        
        private IEnumerator DestroyAnimationCoroutine()
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                
                // Efeito de rotacao
                transform.Rotate(Vector3.forward, 360 * Time.deltaTime);
                
                yield return null;
            }
            
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Configura o cubo
        /// </summary>
        public void SetConfig(Configuration newConfig)
        {
            config = newConfig;
        }
        
        private void OnMouseDown()
        {
            if (!isMerging && !isMoving)
            {
                Select();
            }
        }
    }
}
