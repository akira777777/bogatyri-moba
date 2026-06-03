#!/usr/bin/env bash
set -euo pipefail

# ============================================
#  Богатыри MOBA — Установщик Unity (cross-platform)
# ============================================

UNITY_VERSION="2022.3.45f1"
DRY_RUN=false
SKIP_HUB_CHECK=false

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

usage() {
    cat <<EOF
Usage: $0 [OPTIONS]

Options:
  -v, --version VERSION   Указать версию Unity Editor (default: $UNITY_VERSION)
  -s, --skip-hub          Пропустить проверку Unity Hub
  -d, --dry-run           Только показать, что будет сделано
  -h, --help              Показать эту справку

Examples:
  $0
  $0 --version 2022.3.50f1
  $0 --dry-run
EOF
}

log_info()  { echo -e "${BLUE}[INFO]${NC}  $*"; }
log_ok()    { echo -e "${GREEN}[OK]${NC}    $*"; }
log_warn()  { echo -e "${YELLOW}[WARN]${NC}  $*"; }
log_error() { echo -e "${RED}[ERROR]${NC} $*"; }

# Resolve script directory robustly (works with symlinks)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

detect_os() {
    case "$(uname -s)" in
        Darwin*) echo "macos" ;;
        Linux*)  echo "linux" ;;
        MINGW*|MSYS*|CYGWIN*) echo "windows" ;;
        *)       echo "unknown" ;;
    esac
}

OS="$(detect_os)"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case "$1" in
        -v|--version)
            UNITY_VERSION="$2"
            shift 2
            ;;
        -s|--skip-hub)
            SKIP_HUB_CHECK=true
            shift
            ;;
        -d|--dry-run)
            DRY_RUN=true
            shift
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            log_error "Неизвестный аргумент: $1"
            usage
            exit 1
            ;;
    esac
done

if $DRY_RUN; then
    log_info "DRY RUN: никаких изменений не будет внесено"
fi

echo "=========================================="
echo "  Богатыри MOBA — Установщик Unity"
echo "  OS: $OS | Unity: $UNITY_VERSION"
echo "=========================================="
echo ""

# ── 1. Unity Hub detection ─────────────────
find_unity_hub() {
    local paths=()
    case "$OS" in
        macos)
            paths+=(
                "/Applications/Unity Hub.app/Contents/MacOS/Unity Hub"
                "$HOME/Applications/Unity Hub.app/Contents/MacOS/Unity Hub"
            )
            ;;
        linux)
            paths+=(
                "/usr/bin/unity-hub"
                "/usr/local/bin/unity-hub"
                "/opt/unity-hub/unity-hub"
                "$HOME/Applications/Unity Hub/unity-hub"
            )
            ;;
        windows)
            paths+=(
                "/c/Program Files/Unity Hub/Unity Hub.exe"
                "/c/Program Files (x86)/Unity Hub/Unity Hub.exe"
                "$LOCALAPPDATA/Programs/Unity Hub/Unity Hub.exe"
            )
            ;;
    esac

    for p in "${paths[@]}"; do
        if [[ -f "$p" ]]; then
            echo "$p"
            return 0
        fi
    done

    # Try command -v as fallback
    if command -v unity-hub &>/dev/null; then
        command -v unity-hub
        return 0
    fi

    return 1
}

HUB_PATH=""
if ! $SKIP_HUB_CHECK; then
    echo "[1/5] Поиск Unity Hub..."
    if HUB_PATH="$(find_unity_hub)"; then
        log_ok "Unity Hub найден: $HUB_PATH"
    else
        log_warn "Unity Hub не найден."
        case "$OS" in
            macos)
                echo ""
                echo "Установите Unity Hub через Homebrew:"
                echo "  brew install --cask unity-hub"
                ;;
            linux)
                echo ""
                echo "Скачайте Unity Hub с: https://unity.com/download"
                echo "Или установите через snap:"
                echo "  sudo snap install unity-hub --classic"
                ;;
            windows)
                echo ""
                echo "Скачайте Unity Hub с: https://unity.com/download"
                ;;
        esac
        echo ""
        echo "Или пропустите эту проверку: $0 --skip-hub"
        exit 1
    fi

    if $DRY_RUN; then
        log_info "Было бы проверено наличие Unity Editor $UNITY_VERSION"
    fi
fi

# ── 2. Unity Editor hints ──────────────────
if ! $SKIP_HUB_CHECK && [[ -n "$HUB_PATH" ]]; then
    echo ""
    echo "[2/5] Проверка Unity Editor..."
    echo ""
    echo "⚠️  Для автоматической установки Unity Editor требуется авторизация."
    echo ""
    echo "Рекомендуемые шаги:"
    echo "  1. Откройте Unity Hub"
    echo "  2. Войдите в Unity ID"
    echo "  3. Installs → Install Editor → $UNITY_VERSION"
    echo ""
    if [[ "$OS" != "windows" ]]; then
        echo "Или выполните в терминале (после входа в Unity Hub):"
        echo "  \"$HUB_PATH\" -- --headless install --version $UNITY_VERSION"
    fi
    echo ""
fi

# ── 3. Project structure validation ────────
echo "[3/5] Проверка структуры проекта..."
echo "  Путь: $SCRIPT_DIR"

check_dir() {
    local label="$1"
    local path="$2"
    if [[ -d "$path" ]]; then
        log_ok "$label"
        return 0
    else
        log_error "$label не найдено: $path"
        return 1
    fi
}

check_file() {
    local label="$1"
    local path="$2"
    if [[ -f "$path" ]]; then
        log_ok "$label"
        return 0
    else
        log_warn "$label не найдено: $path"
        return 1
    fi
}

ERRORS=0
check_dir "Скрипты"        "$SCRIPT_DIR/Assets/Scripts"        || ((ERRORS++))
check_dir "Editor-скрипты" "$SCRIPT_DIR/Assets/Editor"         || ((ERRORS++))
check_dir "Сцены"          "$SCRIPT_DIR/Assets/Scenes"         || true
check_dir "Префабы"        "$SCRIPT_DIR/Assets/Prefabs"        || true
check_dir "Resources"      "$SCRIPT_DIR/Assets/Resources"      || true

check_file "Packages/manifest.json"   "$SCRIPT_DIR/Packages/manifest.json"   || ((ERRORS++))
check_file "ProjectSettings"          "$SCRIPT_DIR/ProjectSettings/ProjectVersion.txt" || ((ERRORS++))

if [[ $ERRORS -gt 0 ]]; then
    echo ""
    log_error "Обнаружено $ERRORS критических проблем. Убедитесь, что запускаете скрипт из корня проекта."
    exit 1
fi

# ── 4. Library folder check ────────────────
echo ""
echo "[4/5] Проверка состояния проекта..."
if [[ -d "$SCRIPT_DIR/Library" ]]; then
    log_ok "Проект уже импортировался ранее (Library найден)"
else
    log_warn "Папка Library отсутствует — при первом открытии Unity импортирует проект (~1-2 мин)"
fi

# ── 5. Git LFS check (Unity projects often use it) ──
echo ""
echo "[5/5] Проверка окружения..."
if command -v git &>/dev/null; then
    if git -C "$SCRIPT_DIR" lfs status &>/dev/null 2>&1 || [[ -f "$SCRIPT_DIR/.gitattributes" ]]; then
        if git lfs &>/dev/null; then
            log_ok "Git LFS доступен"
        else
            log_warn "Git LFS не установлен, но проект может использовать LFS-файлы"
        fi
    fi
else
    log_warn "Git не найден в PATH"
fi

echo ""
echo "=========================================="
echo "  Проект готов к запуску!"
echo "=========================================="
echo ""
echo "Следующие шаги:"
echo "  1. Убедитесь, что Unity Editor $UNITY_VERSION установлен"
echo "  2. Откройте Unity Hub → Open → выберите:"
echo "     $SCRIPT_DIR"
echo "  3. Дождитесь импорта (~1-2 минуты)"
echo "  4. В меню: Bogatyri → Setup Project (Full Setup)"
echo "  5. Откройте сцену Assets/Scenes/Gameplay.unity"
echo "  6. Нажмите Play ▶"
echo ""
