# Богатыри MOBA — Unity Project

2D MOBA в стиле Brawl Stars на русском фольклоре. 3v3 онлайн-матчи, 2–3 минуты за сессию.

## Стек

- **Движок:** Unity 2022.3 LTS (2D URP рекомендуется)
- **Сеть:** Photon PUN 2 (устанавливается через Package Manager / Asset Store)
- **Платформа:** Mobile-first (iOS/Android), PC/Mac для разработки

## Быстрый старт

### 1. Установка Unity

1. Скачайте [Unity Hub](https://unity.com/download)
2. Установите **Unity 2022.3 LTS** с модулями:
   - Android Build Support
   - iOS Build Support (для Mac)
   - Visual Studio Editor (опционально)

### 2. Создание проекта

1. В Unity Hub нажмите **New Project**
2. Выберите шаблон **2D (URP)** или **2D Built-in**
3. Назовите проект `BogatyriMoba`
4. Скопируйте содержимое папки `BogatyriMoba/Assets/` из этого репозитория в папку `Assets/` вашего проекта

### 3. Установка Photon PUN 2

1. Откройте **Window → Package Manager**
2. Нажмите **+ → Add package from git URL...**
3. Введите: `https://github.com/Unity-Technologies/com.unity.multiplayer.mlapi.git` (или установите через Asset Store)
4. **Важно:** для простоты MVP используйте **Photon PUN 2 Free** из Asset Store
5. Создайте App ID на [photonengine.com](https://dashboard.photonengine.com)
6. Вставьте App ID в `PhotonServerSettings` (Window → Photon → Highlight Server Settings)

### 4. Генерация данных бойцов

1. В Unity меню выберите **Bogatyri → Generate All Brawler Data**
2. Это создаст:
   - 9 ScriptableObject'ов бойцов в `Assets/ScriptableObjects/Brawlers/`
   - 9 ScriptableObject'ов ультимейтов в `Assets/ScriptableObjects/Ultimates/`

### 5. Настройка сцены

1. Создайте сцену `Gameplay`
2. Добавьте пустой объект `GameManager` с компонентом `GameManager`
3. Добавьте пустой объект `GameMode` с компонентом `GemGrabMode`
4. Настройте Spawn Points (минимум 3 на команду)
5. Создайте префаб `Brawler`:
   - Добавьте `SpriteRenderer`, `Rigidbody2D`, `CircleCollider2D`
   - Добавьте `BrawlerController`, `PlayerInput`
   - Добавьте `ProjectileSpawnPoint` (пустой объект-потомок)
   - Сохраните как префаб в `Assets/Prefabs/`
6. Создайте префаб `Gem`:
   - Добавьте `SpriteRenderer`, `CircleCollider2D` (isTrigger)
   - Добавьте `Gem`
   - Сохраните как префаб
7. Создайте префабы для ультимейтов (конь, стена, эффекты) или используйте placeholder'ы
8. Назначьте все префабы в `GameManager`

### 6. Настройка слоёв (Layers)

1. Создайте слой **Obstacles** в Edit → Project Settings → Tags and Layers
2. Назначьте все стены и препятствия на слой Obstacles
3. Убедитесь, что `Projectile` проверяет столкновения с этим слоем

## Архитектура

### ScriptableObject архитектура

- **BrawlerData** — хранит статы, роли, ссылки на префабы и **UltimateAbility**
- **UltimateAbility** — абстрактный базовый класс для всех ультимейтов
- Каждый боец имеет уникальный ультимейт, создаваемый через `CreateAssetMenu`

### Ключевые системы

| Скрипт | Назначение |
|--------|-----------|
| `BrawlerController` | Движение, атака, получение урона, щит, оглушение, стелс, баффы |
| `PlayerInput` | Ввод с клавиатуры/джойстика (WASD + мышь для ПК) |
| `SimpleBotAI` | AI ботов: поиск врагов, сбор гемов, обход препятствий |
| `GameManager` | Спавн игроков, управление матчем, камера |
| `GemGrabMode` | Логика Gem Grab: гемы, счёт, победа при 10 гемах |
| `Projectile` | Снаряды с поддержкой piercing (пробивание) |
| `HorseProjectile` | Логика коня-ультимейта |
| `WallObject` | Временная стена с коллайдером |
| `StealthComponent` | Невидимость + скорость + крит |
| `BuffComponent` | Временный бафф скорости/урона |
| `FireBreathZone` | Зона огненного дыхания |

### Уникальные ультимейты (9 штук)

| Боец | Ультимейт | Класс |
|------|-----------|-------|
| Алёша | Конь-огонь | `HorseUltimate` |
| Добрыня | Медвежий рёв | `RoarUltimate` |
| Илья | Соколиный взгляд | `SnipeUltimate` |
| Баба Яга | Избушка | `WallUltimate` |
| Змей Горыныч | Огненное дыхание | `FireBreathUltimate` |
| Тугарин | Дымовая завеса | `SmokeUltimate` |
| Варвара | Живая вода | `HealUltimate` |
| Князь | Золотой стяг | `BuffUltimate` |
| Конюх | Табун | `StampedeUltimate` |

## Roadmap

### MVP (8–10 недель)

- [x] Базовая архитектура (ScriptableObject, State Machine)
- [x] 9 бойцов с уникальными ультимейтами
- [x] Gem Grab режим
- [x] Heist режим (базовая реализация)
- [x] AI ботов
- [ ] Photon PUN 2 интеграция
- [ ] Серверная валидация
- [ ] Мобильный UI (джойстик)
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
