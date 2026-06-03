# Богатыри MOBA — Unity Project

2D MOBA в стиле Brawl Stars на русском фольклоре. 3v3 матчи, 2–3 минуты за сессию.

## Стек

- **Движок:** Unity 6 (6000.4.9f1)
- **Сеть:** Custom NetworkManager (WebSocket-ready, миграция на Netcode for GameObjects / Photon Fusion планируется)
- **Платформа:** Mobile-first (iOS/Android), PC/Mac/Windows для разработки

## Быстрый старт

### 1. Установка Unity

1. Скачайте [Unity Hub](https://unity.com/download)
2. Установите **Unity 6000.4.x** (LTS) с модулями:
   - Android Build Support (для мобильной сборки)
   - iOS Build Support (Mac, опционально)
3. Убедитесь, что установлен **TextMeshPro** (входит в Unity 6 по умолчанию)

### 2. Открытие проекта

1. В Unity Hub нажмите **Open** → выберите корневую папку `bogatyri-moba`
2. Unity импортирует проект (~1–2 минуты)
3. При запросе импортируйте **TMP Essential Resources** (Window → TextMeshPro → Import TMP Essential Resources)

### 3. Автоматическая настройка

При первом открытии проект автоматически запустит **полную настройку** (ProjectSetupWizard), которая создаст:
- Папки проекта (`Prefabs`, `Scenes`, `ScriptableObjects`, и т.д.)
- ScriptableObject'ы 9 бойцов и 9 ультимейтов
- Префабы (Brawler, Gem, Projectile, HorseProjectile, Wall, Safe)
- Сцены Gameplay (Gem Grab) и Heist
- Canvas с HUD (таймер, счёт, game-over, мобильные контролы)
- Слой Obstacles

Если автозапуск не сработал, выберите в меню: **Bogatyri → Setup Project (Full Setup)**.

### 4. Запуск

1. Откройте сцену `Assets/Scenes/Gameplay.unity`
2. Нажмите **Play ▶** — матч стартует автоматически

Или используйте: **Bogatyri → Play Gameplay (Auto Setup + Play)**.

## Архитектура

### Data-driven дизайн

- **BrawlerData** (ScriptableObject) — статы, роль, ссылки на префабы и **UltimateAbility**
- **UltimateAbility** — абстрактный базовый класс, каждый ультимейт — отдельный SO
- **UITheme** (ScriptableObject) — цвета, шрифты, safe area для UI

### Ключевые системы

| Скрипт | Назначение |
|--------|-----------|
| `GameManager` | Singleton-координатор: запуск матча, камера, UI-ссылки |
| `MatchManager` | Жизненный цикл матча: старт, конец, таймер, счёт |
| `SpawnManager` | Спавн игроков, ботов, гемов с пулингом |
| `EventBus` | Type-safe шина событий для decoupled коммуникации |
| `BrawlerController` | Движение, атака, HP, щит, оглушение, стелс, баффы |
| `PlayerInput` | Ввод (WASD + мышь для ПК, touch через MobileControlsUI) |
| `OptimizedBotAI` | AI ботов: поиск врагов, сбор гемов, обход препятствий |
| `GemGrabMode` | Режим Gem Grab: гемы, счёт, победа при 10 гемах |
| `HeistMode` | Режим Heist: атака/защита сейфов |
| `SpatialHashGrid` | Пространственное хеширование для оптимизации overlap-запросов |
| `ObjectPool` / `PoolManager` | Пулинг объектов для снарядов и эффектов |
| `PerformanceManager` | Мониторинг FPS, адаптивное качество |
| `NetworkManager` | Сетевой менеджер (WebSocket, placeholder для multiplayer) |
| `SaveSystem` | Сохранение прогресса (PlayerPrefs) |

### UI система

| Скрипт | Назначение |
|--------|-----------|
| `MatchHudController` | Контроллер HUD: биндинг к режиму игры |
| `MatchTimerUI` | Отображение таймера матча |
| `TeamScoreUI` | Счёт команд (Gem Grab) |
| `HeistSafeHudUI` | HP сейфов (Heist) |
| `GameOverUI` | Экран конца матча (победа/поражение/ничья) |
| `MobileControlsUI` | Мобильные контролы (джойстики + кнопки) |
| `VirtualJoystick` | Виртуальный джойстик с drag-событиями |
| `UIHealthBar` / `WorldHealthBarFactory` | HP-бары над персонажами |

### Локализация

- `LocaleManager` — управление текущим языком (RU/EN), сохранение в PlayerPrefs
- `LocalizationCatalog` — каталог строк RU/EN для in-match UI
- `LocalizedUI` — хелпер для получения локализованных строк
- Смена языка в редакторе: **Bogatyri → Locale → Use Russian / English**

### Editor Tools

| Инструмент | Меню | Назначение |
|-----------|------|-----------|
| `ProjectSetupWizard` | Bogatyri → Setup Project | Полная настройка проекта (папки, префабы, сцены) |
| `BrawlerDataFactory` | (через wizard) | Генерация 9 бойцов и ультимейтов |
| `UiHudBuilder` | (через wizard) | Создание Canvas с HUD |
| `InMatchUiVerifier` | Bogatyri → Verify In-Match UI | Проверка полноты локализации |
| `LocaleDebugMenu` | Bogatyri → Locale | Переключение языка |

### Уникальные ультимейты (9 штук)

| Боец | Ультимейт | Класс |
|------|-----------|-------|
| Алёша Попович | Конь-огонь | `HorseUltimate` |
| Добрыня Никитич | Медвежий рёв | `RoarUltimate` |
| Илья Муромец | Соколиный взгляд | `SnipeUltimate` |
| Баба Яга | Избушка | `WallUltimate` |
| Змей Горыныч | Огненное дыхание | `FireBreathUltimate` |
| Тугарин Змей | Дымовая завеса | `SmokeUltimate` |
| Варвара Краса | Живая вода | `HealUltimate` |
| Князь Владимир | Золотой стяг | `BuffUltimate` |
| Конюх Сивка | Табун | `StampedeUltimate` |

### Assembly Definitions

| Сборка | Содержимое |
|--------|-----------|
| `BogatyriMoba` | Core, UI, GameModes, Localization, Networking |
| `BogatyriMoba.Editor` | Editor tools (зависит от BogatyriMoba) |

## Roadmap

### MVP (8–10 недель)

- [x] Базовая архитектура (ScriptableObject, EventBus, SpatialHashGrid)
- [x] 9 бойцов с уникальными ультимейтами
- [x] Gem Grab режим
- [x] Heist режим
- [x] AI ботов (OptimizedBotAI)
- [x] In-match HUD (таймер, счёт, game-over, HP-бары)
- [x] Локализация RU/EN (in-match)
- [x] Мобильный UI (джойстики + кнопки, Android/iOS)
- [x] Object pooling + performance manager
- [x] Editor tools (автоматическая настройка проекта)
- [ ] Netcode for GameObjects / Photon Fusion интеграция
- [ ] Серверная валидация
- [ ] Power Level 1–5
- [ ] Trophy Road
- [ ] Звуки и VFX

### Post-MVP

- [ ] Кланы (дружины)
- [ ] Brawl Pass
- [ ] Монетизация (косметика)
- [ ] Ranked mode
- [ ] Клановые войны

## Лицензия

Персонажи фольклора — общественное достояние. Собственный арт и код.
