using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using MergCrush.Core;
using MergCrush.Theme;
using MergCrush.Level;
using MergCrush.UI;

namespace MergCrush.Editor
{
    /// <summary>
    /// Configurador de cenas do jogo MergCrush
    /// Execute via menu: Tools > MergCrush > Setup Scenes
    /// </summary>
    public static class SceneSetup
    {
        [MenuItem("Tools/MergCrush/Setup All Scenes")]
        public static void SetupAllScenes()
        {
            SetupMainMenuScene();
            SetupLevelSelectScene();
            SetupGameScene();
            CreateThemeAssets();
            CreateConfigurationAsset();
            
            Debug.Log("Todas as cenas configuradas!");
        }
        
        [MenuItem("Tools/MergCrush/Setup Main Menu Scene")]
        public static void SetupMainMenuScene()
        {
            // Criar nova cena
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Adicionar Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera mainCamera = camObj.AddComponent<Camera>();
            mainCamera.backgroundColor = new Color(0.2f, 0.3f, 0.4f);
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5.4f;
            camObj.transform.position = new Vector3(0, 0, -10);
            camObj.tag = "MainCamera";
            
            // Criar Canvas
            GameObject canvasObj = CreateCanvas("MainCanvas");
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Criar Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.rectTransform.anchorMin = Vector2.zero;
            bgImage.rectTransform.anchorMax = Vector2.one;
            bgImage.rectTransform.offsetMin = Vector2.zero;
            bgImage.rectTransform.offsetMax = Vector2.zero;
            bgImage.color = new Color(0.95f, 0.95f, 0.95f);
            
            // Criar painel do menu
            GameObject panelObj = new GameObject("MenuPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400, 500);
            panelRect.anchoredPosition = Vector2.zero;
            
            // Titulo
            GameObject titleObj = CreateUIText("Title", "MERGCRUSH", panelObj.transform);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 150);
            titleRect.sizeDelta = new Vector2(350, 80);
            Text titleText = titleObj.GetComponent<Text>();
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // Botao Play
            GameObject playBtn = CreateUIButton("PlayButton", "JOGAR", panelObj.transform);
            RectTransform playRect = playBtn.GetComponent<RectTransform>();
            playRect.anchoredPosition = new Vector2(0, 20);
            playRect.sizeDelta = new Vector2(250, 60);
            
            // Botao Settings
            GameObject settingsBtn = CreateUIButton("SettingsButton", "Configuracoes", panelObj.transform);
            RectTransform settingsRect = settingsBtn.GetComponent<RectTransform>();
            settingsRect.anchoredPosition = new Vector2(0, -60);
            settingsRect.sizeDelta = new Vector2(250, 60);
            
            // Botao Quit
            GameObject quitBtn = CreateUIButton("QuitButton", "Sair", panelObj.transform);
            RectTransform quitRect = quitBtn.GetComponent<RectTransform>();
            quitRect.anchoredPosition = new Vector2(0, -140);
            quitRect.sizeDelta = new Vector2(250, 60);
            
            // Adicionar MainMenuUI
            MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();
            
            // Criar EventSystem
            CreateEventSystem();
            
            // Salvar cena
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("Cena MainMenu criada!");
        }
        
        [MenuItem("Tools/MergCrush/Setup Level Select Scene")]
        public static void SetupLevelSelectScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Adicionar Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera mainCamera = camObj.AddComponent<Camera>();
            mainCamera.backgroundColor = new Color(0.15f, 0.2f, 0.3f);
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5.4f;
            camObj.transform.position = new Vector3(0, 0, -10);
            camObj.tag = "MainCamera";
            
            // Criar Canvas
            GameObject canvasObj = CreateCanvas("MainCanvas");
            
            // Criar Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.rectTransform.anchorMin = Vector2.zero;
            bgImage.rectTransform.anchorMax = Vector2.one;
            bgImage.rectTransform.offsetMin = Vector2.zero;
            bgImage.rectTransform.offsetMax = Vector2.zero;
            bgImage.color = new Color(0.9f, 0.9f, 0.95f);
            
            // Titulo
            GameObject titleObj = CreateUIText("Title", "Selecione a Fase", canvasObj.transform);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -50);
            titleRect.sizeDelta = new Vector2(400, 60);
            Text titleText = titleObj.GetComponent<Text>();
            titleText.fontSize = 36;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // Container de botoes de nivel
            GameObject containerObj = new GameObject("LevelButtonsContainer");
            containerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.offsetMin = new Vector2(50, 100);
            containerRect.offsetMax = new Vector2(-50, -100);
            
            // Adicionar GridLayoutGroup
            GridLayoutGroup grid = containerObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 120);
            grid.spacing = new Vector2(20, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.MiddleCenter;
            
            // Adicionar LevelSelectUI
            LevelSelectUI levelSelectUI = canvasObj.AddComponent<LevelSelectUI>();
            
            // Botao Voltar
            GameObject backBtn = CreateUIButton("BackButton", "Voltar", canvasObj.transform);
            RectTransform backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0);
            backRect.anchorMax = new Vector2(0, 0);
            backRect.anchoredPosition = new Vector2(100, 50);
            backRect.sizeDelta = new Vector2(150, 50);
            
            // Criar EventSystem
            CreateEventSystem();
            
            // Salvar cena
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/LevelSelect.unity");
            Debug.Log("Cena LevelSelect criada!");
        }
        
        [MenuItem("Tools/MergCrush/Setup Game Scene")]
        public static void SetupGameScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Adicionar Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera mainCamera = camObj.AddComponent<Camera>();
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 6f;
            camObj.transform.position = new Vector3(0, 0, -10);
            camObj.tag = "MainCamera";
            
            // Criar managers
            GameObject managersObj = new GameObject("GameManagers");
            
            // GridManager
            GameObject gridManagerObj = new GameObject("GridManager");
            gridManagerObj.transform.SetParent(managersObj.transform);
            GridManager gridManager = gridManagerObj.AddComponent<GridManager>();
            
            // CubeSpawner
            GameObject spawnerObj = new GameObject("CubeSpawner");
            spawnerObj.transform.SetParent(managersObj.transform);
            CubeSpawner spawner = spawnerObj.AddComponent<CubeSpawner>();
            
            // CubeCollision
            GameObject collisionObj = new GameObject("CubeCollision");
            collisionObj.transform.SetParent(managersObj.transform);
            CubeCollision collision = collisionObj.AddComponent<CubeCollision>();
            
            // ScoreManager
            GameObject scoreObj = new GameObject("ScoreManager");
            scoreObj.transform.SetParent(managersObj.transform);
            ScoreManager scoreManager = scoreObj.AddComponent<ScoreManager>();
            
            // ThemeManager
            GameObject themeObj = new GameObject("ThemeManager");
            themeObj.transform.SetParent(managersObj.transform);
            ThemeManager themeManager = themeObj.AddComponent<ThemeManager>();
            
            // LevelManager
            GameObject levelObj = new GameObject("LevelManager");
            levelObj.transform.SetParent(managersObj.transform);
            LevelManager levelManager = levelObj.AddComponent<LevelManager>();
            
            // Criar Canvas de UI do jogo
            GameObject canvasObj = CreateCanvas("GameCanvas");
            
            // Background do jogo
            GameObject bgObj = new GameObject("GameBackground");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.rectTransform.anchorMin = Vector2.zero;
            bgImage.rectTransform.anchorMax = Vector2.one;
            bgImage.rectTransform.offsetMin = Vector2.zero;
            bgImage.rectTransform.offsetMax = Vector2.zero;
            bgImage.color = new Color(0.2f, 0.25f, 0.3f);
            
            // Painel de score (topo)
            GameObject scorePanel = new GameObject("ScorePanel");
            scorePanel.transform.SetParent(canvasObj.transform, false);
            RectTransform scoreRect = scorePanel.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(1, 1);
            scoreRect.sizeDelta = new Vector2(0, 80);
            scoreRect.anchoredPosition = new Vector2(0, -40);
            
            Image scoreBg = scorePanel.AddComponent<Image>();
            scoreBg.color = new Color(0, 0, 0, 0.5f);
            
            // Score text
            GameObject scoreTextObj = CreateUIText("ScoreText", "Score: 0", scorePanel.transform);
            RectTransform scoreTextRect = scoreTextObj.GetComponent<RectTransform>();
            scoreTextRect.anchorMin = new Vector2(0, 0.5f);
            scoreTextRect.anchorMax = new Vector2(0, 0.5f);
            scoreTextRect.anchoredPosition = new Vector2(100, 0);
            scoreTextRect.sizeDelta = new Vector2(200, 50);
            
            // Target text
            GameObject targetTextObj = CreateUIText("TargetText", "Target: 1000", scorePanel.transform);
            RectTransform targetRect = targetTextObj.GetComponent<RectTransform>();
            targetRect.anchorMin = new Vector2(1, 0.5f);
            targetRect.anchorMax = new Vector2(1, 0.5f);
            targetRect.anchoredPosition = new Vector2(-100, 0);
            targetRect.sizeDelta = new Vector2(200, 50);
            
            // Barra de progresso
            GameObject progressObj = new GameObject("ProgressBar");
            progressObj.transform.SetParent(scorePanel.transform, false);
            RectTransform progressRect = progressObj.AddComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.5f, 0.5f);
            progressRect.anchorMax = new Vector2(0.5f, 0.5f);
            progressRect.anchoredPosition = Vector2.zero;
            progressRect.sizeDelta = new Vector2(300, 20);
            
            Slider progressSlider = progressObj.AddComponent<Slider>();
            
            // Estrelas
            for (int i = 0; i < 3; i++)
            {
                GameObject starObj = new GameObject($"Star_{i + 1}");
                starObj.transform.SetParent(scorePanel.transform, false);
                Image starImage = starObj.AddComponent<Image>();
                RectTransform starRect = starObj.GetComponent<RectTransform>();
                starRect.anchorMin = new Vector2(0.5f, 0.5f);
                starRect.anchorMax = new Vector2(0.5f, 0.5f);
                starRect.anchoredPosition = new Vector2(-40 + i * 40, 0);
                starRect.sizeDelta = new Vector2(30, 30);
            }
            
            // Painel de pause
            GameObject pausePanel = CreatePanel("PausePanel", canvasObj.transform, false);
            CreateUIText("PauseTitle", "PAUSADO", pausePanel.transform);
            CreateUIButton("ResumeButton", "Continuar", pausePanel.transform);
            CreateUIButton("MenuButton", "Menu", pausePanel.transform);
            
            // Painel de game over
            GameObject gameOverPanel = CreatePanel("GameOverPanel", canvasObj.transform, false);
            CreateUIText("GameOverTitle", "GAME OVER", gameOverPanel.transform);
            CreateUIText("FinalScore", "Score: 0", gameOverPanel.transform);
            CreateUIButton("RetryButton", "Tentar Novamente", gameOverPanel.transform);
            CreateUIButton("MenuButton", "Menu", gameOverPanel.transform);
            
            // Painel de vitoria
            GameObject victoryPanel = CreatePanel("VictoryPanel", canvasObj.transform, false);
            CreateUIText("VictoryTitle", "NIVEL COMPLETO!", victoryPanel.transform);
            
            // Estrelas de vitoria
            for (int i = 0; i < 3; i++)
            {
                GameObject starObj = new GameObject($"Star_{i + 1}");
                starObj.transform.SetParent(victoryPanel.transform, false);
                Image starImage = starObj.AddComponent<Image>();
                RectTransform starRect = starObj.GetComponent<RectTransform>();
                starRect.anchoredPosition = new Vector2(-50 + i * 50, 50);
                starRect.sizeDelta = new Vector2(40, 40);
            }
            
            CreateUIText("VictoryScore", "Score: 0", victoryPanel.transform);
            CreateUIButton("NextLevelButton", "Proximo Nivel", victoryPanel.transform);
            CreateUIButton("ReplayButton", "Jogar Novamente", victoryPanel.transform);
            
            // Adicionar UIManager
            UIManager uiManager = canvasObj.AddComponent<UIManager>();
            
            // Adicionar GameoverUI
            GameoverUI gameOverUI = canvasObj.AddComponent<GameoverUI>();
            
            // Botao de pause (canto superior direito)
            GameObject pauseBtn = CreateUIButton("PauseButton", "II", canvasObj.transform);
            RectTransform pauseRect = pauseBtn.GetComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(1, 1);
            pauseRect.anchorMax = new Vector2(1, 1);
            pauseRect.anchoredPosition = new Vector2(-50, -50);
            pauseRect.sizeDelta = new Vector2(60, 60);
            
            // Criar EventSystem
            CreateEventSystem();
            
            // Salvar cena
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
            Debug.Log("Cena GameScene criada!");
        }
        
        [MenuItem("Tools/MergCrush/Create Theme Assets")]
        public static void CreateThemeAssets()
        {
            string[] themeNames = { "Sushi", "Cafeteria", "Praia", "Shopping", "Carros" };
            int[] targetScores = { 1000, 1500, 2000, 2500, 3000 };
            int[] difficulties = { 1, 2, 3, 4, 5 };
            
            for (int i = 0; i < themeNames.Length; i++)
            {
                CreateThemeAsset(themeNames[i], i + 1, targetScores[i], difficulties[i]);
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log("Assets de tema criados!");
        }
        
        private static void CreateThemeAsset(string themeName, int levelNumber, int targetScore, int difficulty)
        {
            ThemeData theme = ScriptableObject.CreateInstance<ThemeData>();
            theme.themeName = themeName;
            theme.description = $"Fase {levelNumber} - Tema {themeName}";
            theme.levelTargetScore = targetScore;
            theme.levelDifficulty = difficulty;
            
            // Carregar sprites
            string itemsPath = $"Assets/Resources/Themes/{themeName}/Items";
            string bgPath = $"Assets/Resources/Themes/{themeName}/BG";
            
            // Tentar carregar sprites dos itens
            theme.itemSprites = new Sprite[6];
            theme.itemNames = new string[6];
            
            string[] itemFiles = System.IO.Directory.Exists(itemsPath) 
                ? System.IO.Directory.GetFiles(itemsPath, "*.png") 
                : new string[0];
            
            for (int i = 0; i < Mathf.Min(itemFiles.Length, 6); i++)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(itemFiles[i]);
                if (sprite != null)
                {
                    theme.itemSprites[i] = sprite;
                    theme.itemNames[i] = System.IO.Path.GetFileNameWithoutExtension(itemFiles[i]);
                }
            }
            
            // Carregar background
            string[] bgFiles = System.IO.Directory.Exists(bgPath) 
                ? System.IO.Directory.GetFiles(bgPath, "*.png") 
                : new string[0];
            
            if (bgFiles.Length > 0)
            {
                theme.backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgFiles[0]);
            }
            
            // Definir cores baseado no tema
            switch (themeName)
            {
                case "Sushi":
                    theme.uiAccentColor = new Color(1f, 0.4f, 0.3f);
                    theme.backgroundColor = new Color(0.95f, 0.9f, 0.85f);
                    break;
                case "Cafeteria":
                    theme.uiAccentColor = new Color(0.6f, 0.4f, 0.2f);
                    theme.backgroundColor = new Color(0.95f, 0.9f, 0.85f);
                    break;
                case "Praia":
                    theme.uiAccentColor = new Color(0.2f, 0.6f, 0.9f);
                    theme.backgroundColor = new Color(0.9f, 0.95f, 1f);
                    break;
                case "Shopping":
                    theme.uiAccentColor = new Color(1f, 0.4f, 0.7f);
                    theme.backgroundColor = new Color(0.98f, 0.95f, 0.95f);
                    break;
                case "Carros":
                    theme.uiAccentColor = new Color(0.9f, 0.2f, 0.2f);
                    theme.backgroundColor = new Color(0.9f, 0.92f, 0.95f);
                    break;
            }
            
            theme.uiTextColor = Color.white;
            
            // Salvar asset
            string assetPath = $"Assets/Resources/Themes/{themeName}Data.asset";
            AssetDatabase.CreateAsset(theme, assetPath);
        }
        
        [MenuItem("Tools/MergCrush/Create Configuration Asset")]
        public static void CreateConfigurationAsset()
        {
            Configuration config = ScriptableObject.CreateInstance<Configuration>();
            
            // Carregar temas
            string[] themeNames = { "Sushi", "Cafeteria", "Praia", "Shopping", "Carros" };
            config.themes = new ThemeData[themeNames.Length];
            
            for (int i = 0; i < themeNames.Length; i++)
            {
                ThemeData theme = AssetDatabase.LoadAssetAtPath<ThemeData>(
                    $"Assets/Resources/Themes/{themeNames[i]}Data.asset");
                config.themes[i] = theme;
            }
            
            // Salvar asset
            AssetDatabase.CreateAsset(config, "Assets/Resources/GameConfiguration.asset");
            AssetDatabase.SaveAssets();
            
            Debug.Log("Configuration asset criado!");
        }
        
        #region Helper Methods
        
        private static GameObject CreateCanvas(string name)
        {
            GameObject canvasObj = new GameObject(name);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            return canvasObj;
        }
        
        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }
        
        private static GameObject CreateUIText(string name, string text, Transform parent)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(200, 50);
            
            Text uiText = textObj.AddComponent<Text>();
            uiText.text = text;
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontSize = 24;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.color = Color.black;
            
            return textObj;
        }
        
        private static GameObject CreateUIButton(string name, string text, Transform parent)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(200, 50);
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.6f, 0.9f);
            
            Button button = buttonObj.AddComponent<Button>();
            
            // Texto do botao
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text uiText = textObj.AddComponent<Text>();
            uiText.text = text;
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontSize = 20;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.color = Color.white;
            
            return buttonObj;
        }
        
        private static GameObject CreatePanel(string name, Transform parent, bool active)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);
            panelObj.SetActive(active);
            
            RectTransform rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image image = panelObj.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.8f);
            
            return panelObj;
        }
        
        #endregion
    }
}
