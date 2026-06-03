# UI style guide (in-match)

Lightweight tokens for Bogatyri MOBA HUD. Folklore-inspired readability; do not copy third-party art wholesale (see `Docs/NextSteps.md`).

## Color tokens

| Token | Hex | RGBA | Use |
|-------|-----|------|-----|
| TeamBlue | `#4DABF7` | (0.30, 0.67, 0.97) | Blue score, win title |
| TeamRed | `#FF6B6B` | (1.0, 0.42, 0.42) | Red score, loss title |
| AccentGold | `#FFD54F` | (1.0, 0.84, 0.31) | Match timer |
| HudPanel | `#00000099` | black ~60% alpha | Score panels |
| NeutralGray | `#9E9E9E` | gray | Draw state |
| SafeBlue | `#5C6BC0` | tint | Heist safe (blue) |
| SafeRed | `#EF5350` | tint | Heist safe (red) |

## Typography (TMP)

| Role | Size (1080×1920 ref) | Weight |
|------|----------------------|--------|
| Timer | 32 | Bold |
| Team score | 28 | Bold |
| Game-over title | 36 | Bold |
| Game-over body | 22 | Regular |
| Control labels | 18 | Regular |

## Layout

- Reference resolution: **1080 × 1920** portrait (mobile-first).
- `CanvasScaler`: scale with screen size, match **0.5** width/height.
- Safe area inset: **48 px** on sides and bottom (notch / home indicator).
- Timer: top center, below safe top.
- Scores: blue left, red right, vertically centered in top band.
- Mobile controls: move stick bottom-left, aim stick bottom-right, action buttons above aim stick.

## Placeholders

Until art pipeline delivers sprites, use semi-transparent panels and TMP. Team tint on health bar fill.
