using System.Collections.ObjectModel;
using System.IO;

namespace Anything_UI_WPF.Services;

public class LanguageService
{
    private Dictionary<string, string> _strings = DefaultRu();
    public string CurrentCode { get; private set; } = "ru";

    public LanguageService()
    {
        SetLanguage(CurrentCode);
    }

    public static ObservableCollection<LanguageInfo> AvailableLanguages { get; } = new()
    {
        new("ru", "Русский"),
        new("en", "English"),
    };

    public string this[string key] => _strings.GetValueOrDefault(key, key);

    public string Format(string key, params (string, string)[] args)
    {
        var s = this[key];
        foreach (var (k, v) in args)
            s = s.Replace($"{{{k}}}", v);
        return s;
    }

    public void SetLanguage(string code)
    {
        CurrentCode = code;
        _strings = DefaultRu();
        if (code != "ru")
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages", $"LANG.{code}.yaml");
            if (File.Exists(path))
            {
                try
                {
                    var loaded = ParseSimpleYaml(File.ReadAllText(path));
                    foreach (var (k, v) in loaded)
                        if (v != null)
                            _strings[k] = v;
                }
                catch { }
            }
        }
    }

    private static Dictionary<string, string?> ParseSimpleYaml(string yaml)
    {
        var result = new Dictionary<string, string?>();
        foreach (var line in yaml.Split('\n', '\r'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
            var colonPos = trimmed.IndexOf(':');
            if (colonPos < 0) continue;
            var key = trimmed[..colonPos].Trim();
            var value = trimmed[(colonPos + 1)..].Trim().Trim('"').Trim('\'');
            result[key] = value;
        }
        return result;
    }

    private static Dictionary<string, string> DefaultRu() => new()
    {
        ["window_title"] = "Anything — быстрый поиск файлов",
        ["settings"] = "Настройки",
        ["bg_indexing"] = "Индексация в фоне",
        ["rebuild_index"] = "Перестроить индекс",
        ["add_dir"] = "Добавить",
        ["remove"] = "Удалить",
        ["light_theme"] = "Светлая",
        ["dark_theme"] = "Тёмная",
        ["about"] = "О программе",
        ["about_comments"] = "Быстрый поиск файлов\n\nДвижок: Rust (LibAnything + SearchEngine)\nGUI: WPF-UI",
        ["search_placeholder"] = "Введите запрос (например: !tmp ext:pdf \"отчёт\")",
        ["status_init"] = "Инициализация...",
        ["status_ready"] = "Готов к поиску (индекс: {count} записей)",
        ["status_indexing"] = "Индексация...",
        ["status_indexing_progress"] = "Индексация... {count} файлов",
        ["status_building"] = "Индекс ещё строится...",
        ["status_loading_index"] = "Загрузка индекса...",
        ["status_load_error"] = "Ошибка загрузки индекса: {error}",
        ["status_error"] = "Ошибка: {error}",
        ["status_failed"] = "Ошибка индексации",
        ["status_no_results"] = "Совпадений не найдено",
        ["status_results"] = "Найдено: {count} совпадений",
        ["settings_title"] = "Настройки Anything",
        ["language"] = "Язык",
        ["general"] = "Основные",
        ["excluded_dirs"] = "Исключённые каталоги",
        ["dir_placeholder"] = "/путь/к/каталогу",
    };
}

public record LanguageInfo(string Code, string Label);
