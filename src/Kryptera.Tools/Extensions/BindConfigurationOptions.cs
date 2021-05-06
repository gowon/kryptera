namespace Kryptera.Tools.Extensions
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    public class BindConfigurationOptions<TOptions> : IConfigureOptions<TOptions> where TOptions : class
    {
        private readonly IConfiguration _configuration;

        public BindConfigurationOptions(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Configure(TOptions options)
        {
            // only works with root-level options by default
            // would need to subclass to specify a nested configuration section
            GetSection(_configuration).TryBind(options, out _);
        }

        protected virtual IConfiguration GetSection(IConfiguration configuration)
        {
            return configuration;
        }
    }
}