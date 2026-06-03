using System.Collections.Generic;

namespace BogatyriMoba.Localization
{
    /// <summary>
    /// Embedded RU/EN strings for in-match UI. Works without Unity Localization assets in repo.
    /// </summary>
    public static class LocalizationCatalog
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Tables =
            new Dictionary<string, Dictionary<string, string>>
            {
                [LocaleManager.Russian] = new Dictionary<string, string>
                {
                    ["gameover.title_win"] = "Победа!",
                    ["gameover.title_loss"] = "Поражение!",
                    ["gameover.title_draw"] = "Ничья!",
                    ["gameover.body_win"] = "Синяя команда победила! {blue} vs {red}",
                    ["gameover.body_loss"] = "Красная команда победила! {red} vs {blue}",
                    ["gameover.body_draw"] = "Ничья! {blue} vs {red}",
                    ["gameover.play_again"] = "Ещё раз",
                    ["match.gems_label"] = "Кристаллы",
                    ["controls.attack"] = "Атака",
                    ["controls.super"] = "Супер",
                    ["controls.gadget"] = "Гаджет",
                    ["heist.safe_hp"] = "Сейф {0}%",
                },
                [LocaleManager.English] = new Dictionary<string, string>
                {
                    ["gameover.title_win"] = "Victory!",
                    ["gameover.title_loss"] = "Defeat!",
                    ["gameover.title_draw"] = "Draw!",
                    ["gameover.body_win"] = "Blue team wins! {blue} vs {red}",
                    ["gameover.body_loss"] = "Red team wins! {red} vs {blue}",
                    ["gameover.body_draw"] = "Draw! {blue} vs {red}",
                    ["gameover.play_again"] = "Play again",
                    ["match.gems_label"] = "Gems",
                    ["controls.attack"] = "Attack",
                    ["controls.super"] = "Super",
                    ["controls.gadget"] = "Gadget",
                    ["heist.safe_hp"] = "Safe {0}%",
                },
            };

        public static string Get(string key)
        {
            if (!Tables.TryGetValue(LocaleManager.CurrentLocale, out var localeTable))
                localeTable = Tables[LocaleManager.Russian];

            if (localeTable.TryGetValue(key, out var value))
                return value;

            if (LocaleManager.CurrentLocale != LocaleManager.Russian &&
                Tables[LocaleManager.Russian].TryGetValue(key, out var fallback))
                return fallback;

            return key;
        }

        public static string Format(string key, params object[] args)
        {
            var template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }
    }
}
