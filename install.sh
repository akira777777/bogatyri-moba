#!/bin/bash
set -e

echo "=========================================="
echo "  Богатыри MOBA — Установщик Unity"
echo "=========================================="
echo ""

# Fix Homebrew permissions (without sudo inside script)
echo "[1/5] Исправление прав Homebrew..."
if [ -d "/opt/homebrew" ]; then
    if [ ! -w "/opt/homebrew" ]; then
        echo "⚠️  /opt/homebrew недоступен для записи."
        echo "Выполните сначала: sudo chown -R \$(whoami) /opt/homebrew"
        exit 1
    fi
fi

# Install Unity Hub via Homebrew
echo "[2/5] Установка Unity Hub..."
if ! command -v unity-hub &> /dev/null; then
    brew install --cask unity-hub
else
    echo "Unity Hub уже установлен."
fi

# Install Unity Editor via Hub CLI (requires login)
echo "[3/5] Проверка Unity Editor..."
UNITY_VERSION="2022.3.45f1"
HUB_PATH="/Applications/Unity Hub.app/Contents/MacOS/Unity Hub"

if [ ! -f "$HUB_PATH" ]; then
    echo "❌ Unity Hub не найден. Пожалуйста, запустите Unity Hub вручную."
    exit 1
fi

echo ""
echo "⚠️  ВАЖНО: Для установки Unity Editor требуется авторизация."
echo ""
echo "Пожалуйста, выполните следующие шаги вручную:"
echo ""
echo "1. Откройте Unity Hub (Приложения → Unity Hub)"
echo "2. Войдите в свой Unity ID (создайте бесплатный, если нет)"
echo "3. Перейдите в Installs → Install Editor"
echo "4. Выберите версию: $UNITY_VERSION"
echo "5. Выберите модуль: Mac Build Support (IL2CPP)"
echo "6. Дождитесь установки (~5-10 GB)"
echo ""
echo "Или выполните в терминале (после входа в Unity Hub):"
echo "  \"$HUB_PATH\" -- --headless install --version $UNITY_VERSION"
echo ""

# Create symlink to project
echo "[4/5] Создание ярлыка проекта..."
PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
echo "Путь к проекту: $PROJECT_DIR"

# Check project structure
echo "[5/5] Проверка структуры проекта..."
if [ -d "$PROJECT_DIR/Assets/Scripts" ]; then
    echo "✅ Скрипты найдены"
else
    echo "❌ Ошибка: скрипты не найдены"
    exit 1
fi

if [ -d "$PROJECT_DIR/Assets/Editor" ]; then
    echo "✅ Editor-скрипты найдены"
else
    echo "❌ Ошибка: Editor-скрипты не найдены"
    exit 1
fi

echo ""
echo "=========================================="
echo "  Проект готов к запуску!"
echo "=========================================="
echo ""
echo "После установки Unity Editor:"
echo "1. Откройте Unity Hub"
echo "2. Нажмите Open → выберите папку:"
echo "   $PROJECT_DIR"
echo "3. Unity импортирует проект (~1-2 минуты)"
echo "4. В меню выберите: Bogatyri → Setup Project (Full Setup)"
echo "5. Это создаст все префабы и сцену автоматически"
echo "6. Откройте сцену Assets/Scenes/Gameplay.unity"
echo "7. Нажмите Play ▶"
echo ""
