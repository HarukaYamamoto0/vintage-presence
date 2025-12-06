namespace VintagePresence.PresenceTemplate;

public class PresenceTemplateEngine
{
    private readonly Dictionary<string, Func<PresenceContext, string?>> _tokenResolvers;

    public PresenceTemplateEngine()
    {
        _tokenResolvers = new Dictionary<string, Func<PresenceContext, string?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["gamemode"] = ctx => ctx.GameMode,
            ["day"] = ctx => ctx.Day?.ToString(),
            ["timeofday"] = ctx => ctx.TimeOfDay,
            ["playername"] = ctx => ctx.PlayerName,
            ["health"] = ctx => ctx.Health is null || ctx.MaxHealth is null
                ? null
                : $"{ctx.Health:0.#} / {ctx.MaxHealth:0.#}",
            ["healthpercent"] = ctx => ctx.Health is null || ctx.MaxHealth is null || ctx.MaxHealth == 0
                ? null
                : $"{ctx.Health.Value / ctx.MaxHealth.Value * 100:0}%",
            ["deaths"] = ctx => ctx.Deaths?.ToString(),
            ["players"] = ctx => ctx.OnlinePlayers?.ToString(),
            ["coords"] = ctx => ctx.Coords,
            ["temperature"] = ctx => ctx.Temperature?.ToString("0.#"),
            ["weather"] = ctx => ctx.Weather,
            ["season"] = ctx => ctx.Season,
            ["modversion"] = ctx => ctx.ModVersion,
            ["gameversion"] = ctx => ctx.GameVersion,
            ["nl"] = _ => "\n"
        };
    }

    public string Render(string template, PresenceContext? context)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;
        if (context == null) return template;

        var sb = new System.Text.StringBuilder(template.Length);
        for (var i = 0; i < template.Length; i++)
        {
            var c = template[i];

            switch (c)
            {
                case '{':
                {
                    var end = template.IndexOf('}', i + 1);
                    if (end == -1)
                    {
                        sb.Append(c);
                        continue;
                    }

                    var rawToken = template.Substring(i + 1, end - i - 1).Trim();

                    // supports {token|fallback text}
                    var tokenName = rawToken;
                    string? fallback = null;

                    var pipeIndex = rawToken.IndexOf('|');
                    if (pipeIndex >= 0)
                    {
                        tokenName = rawToken[..pipeIndex].Trim();
                        fallback = rawToken[(pipeIndex + 1)..].Trim();
                    }

                    var value = ResolveToken(tokenName, context);

                    if (string.IsNullOrEmpty(value))
                    {
                        if (!string.IsNullOrEmpty(fallback))
                        {
                            sb.Append(fallback);
                        }
                        // If there is no fallback, simply delete the token.
                    }
                    else
                    {
                        sb.Append(value);
                    }

                    i = end;
                    break;
                }
                case '}' when i + 1 < template.Length && template[i + 1] == '}':
                    // Simple escape: "}}" => "}"
                    sb.Append('}');
                    i++;
                    break;
                default:
                {
                    if (c == '{' && i + 1 < template.Length && template[i + 1] == '{')
                    {
                        // Simple escape: "{{" => "{"
                        sb.Append('{');
                        i++;
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
                }
            }
        }

        return sb.ToString();
    }

    private string? ResolveToken(string name, PresenceContext ctx)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        if (!_tokenResolvers.TryGetValue(name, out var resolver)) return null;
        try
        {
            return resolver(ctx);
        }
        catch
        {
            return null;
        }

        // Unknown token => keep literal or return null if you want it to disappear.
    }
}