using System.IO;
using Microsoft.AspNetCore.Hosting;
using Optional;

namespace Vltava.Web
{
    public class SystemFolders
    {
        IHostingEnvironment _env;

        public SystemFolders(IHostingEnvironment environment)
        {
            _env = environment;
        }

        public Option<string> SubscriptionsFile(string filename)
        {
            var fileLocation = Path.Combine(_env.ContentRootPath, "Subscriptions", filename);

            if (File.Exists(fileLocation))
                return Option.Some(fileLocation);
            else
                return Option.None<string>();
        }

        public Option<string> TemplateFile(string filename)
        {
            var fileLocation = Path.Combine(_env.ContentRootPath, "Templates", filename);

            if (File.Exists(fileLocation))
                return Option.Some(fileLocation);
            else
                return Option.None<string>();
        }
    }

}