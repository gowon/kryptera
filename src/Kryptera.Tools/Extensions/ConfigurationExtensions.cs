namespace Kryptera.Tools.Extensions
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        public static bool TryBind<TOption>(this IConfiguration configuration, TOption options,
            out IConfiguration foundSection)
            where TOption : class
        {
            const string suffix = "Options";
            foundSection = null;

            // We look for the config sections with and without the Options prefix
            var name = typeof(TOption).Name;
            var keys = new List<string>
            {
                name
            };

            if (name.EndsWith(suffix))
            {
                keys.Add(name.Remove(name.Length - suffix.Length));
            }

            foreach (var key in keys)
            {
                var section = configuration.GetSection(key);
                if (section.Exists())
                {
                    foundSection = section;
                    configuration.Bind(key, options);
                    return true;
                }
            }

            return false;
        }
    }
}