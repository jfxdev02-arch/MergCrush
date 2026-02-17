# MergCrush - Jogo Candy Crush 2048

## Visao Geral

Jogo de puzzle inspirado no Candy Crush com mecanica hibrida 3D estilo 2048, com 5 temas tematicos.

## Temas Disponiveis

1. **Sushi** - Nivel 1 (Target: 1000 pontos)
2. **Cafeteria** - Nivel 2 (Target: 1500 pontos)
3. **Praia** - Nivel 3 (Target: 2000 pontos)
4. **Shopping** - Nivel 4 (Target: 2500 pontos)
5. **Carros** - Nivel 5 (Target: 3000 pontos)

## Configuracao Inicial

### Passo 1: Configurar Assets no Unity

1. Abra o projeto no Unity
2. Va em `Tools > MergCrush > Setup All Scenes`
3. Isso ira criar automaticamente:
   - Cenas: MainMenu, LevelSelect, GameScene
   - Assets de tema (ThemeData)
   - Configuracao do jogo (GameConfiguration)

### Passo 2: Configurar Sprites

Os sprites estao em `Assets/Resources/Themes/[Tema]/Items/`. No Unity:

1. Selecione cada sprite PNG
2. No Inspector, configure:
   - Texture Type: Sprite (2D and UI)
   - Pixels Per Unit: 512
   - Filter Mode: Bilinear
   - Compression: None (para melhor qualidade)

### Passo 3: Configurar Cenas no Build

1. Va em `File > Build Settings`
2. Adicione as cenas na ordem:
   - `Assets/Scenes/MainMenu.unity` (Index 0)
   - `Assets/Scenes/LevelSelect.unity` (Index 1)
   - `Assets/Scenes/GameScene.unity` (Index 2)

## Estrutura de Arquivos

```
Assets/
├── Scripts/
│   ├── Core/           # Scripts principais do jogo
│   │   ├── Configuration.cs
│   │   ├── GameManager.cs
│   │   ├── GridManager.cs
│   │   ├── Cube.cs
│   │   ├── CubeSpawner.cs
│   │   └── CubeCollision.cs
│   ├── Theme/          # Sistema de temas
│   │   ├── ThemeData.cs
│   │   └── ThemeManager.cs
│   ├── Level/          # Sistema de niveis
│   │   ├── LevelManager.cs
│   │   ├── LevelData.cs
│   │   └── ScoreManager.cs
│   ├── UI/             # Interface de usuario
│   │   ├── UIManager.cs
│   │   ├── MainMenuUI.cs
│   │   ├── LevelSelectUI.cs
│   │   └── GameoverUI.cs
│   └── Editor/         # Ferramentas do editor
│       └── SceneSetup.cs
├── Resources/
│   ├── Themes/         # Assets de cada tema
│   │   ├── Sushi/
│   │   ├── Cafeteria/
│   │   ├── Praia/
│   │   ├── Shopping/
│   │   └── Carros/
│   ├── UI/             # Assets de UI
│   └── GameConfiguration.asset
└── Scenes/
    ├── MainMenu.unity
    ├── LevelSelect.unity
    └── GameScene.unity
```

## Como Jogar

1. **Objetivo**: Alcancar a pontuacao alvo antes que a grid fique cheia
2. **Mecanica**: Quando dois itens iguais se tocam, eles se fundem em um item de nivel superior
3. **Pontuacao**: Cada fusao adiciona pontos. Combos consecutivos dao multiplicadores

## Controles

- **Clique/Toque**: Seleciona e move cubos
- **Arrastar**: Move cubos para posicoes adjacentes
- **Pause**: Botao no canto superior direito

## Criar Novo Tema

1. Crie uma pasta em `Assets/Resources/Themes/[NomeDoTema]/Items/`
2. Adicione 6 sprites de itens (512x512px)
3. Adicione um background (1920x1080px)
4. Va em `Assets > Create > MergCrush > Theme Data`
5. Configure o novo ThemeData
6. Adicione o tema ao GameConfiguration

## Comandos do Menu

- `Tools > MergCrush > Setup All Scenes` - Configura todas as cenas
- `Tools > MergCrush > Setup Main Menu Scene` - Cria cena de menu
- `Tools > MergCrush > Setup Level Select Scene` - Cria cena de selecao
- `Tools > MergCrush > Setup Game Scene` - Cria cena de gameplay
- `Tools > MergCrush > Create Theme Assets` - Cria assets de tema
- `Tools > MergCrush > Create Configuration Asset` - Cria configuracao

## Requisitos

- Unity 2022.3 ou superior (testado com Unity 6)
- URP (Universal Render Pipeline)

## Creditos

Desenvolvido com Unity e AI
