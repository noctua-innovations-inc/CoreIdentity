using Microsoft.Extensions.Configuration;
using System;

namespace AspNetIdentity.Data
{
    public class AspNetStoreBase
    {
        public const string ConfigAppIdKey = "ApplicationId";

        public AspNetStoreBase(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected IConfiguration Configuration { get; }

        protected Guid ApplicationId
        {
            get
            {
                return new Guid(Configuration[ConfigAppIdKey]);
            }
        }
    }
}