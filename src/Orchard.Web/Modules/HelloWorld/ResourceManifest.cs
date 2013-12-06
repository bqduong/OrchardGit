using Orchard.UI.Resources;

namespace HelloWorld
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery").SetUrl("jquery-1.9.1.min.js", "jquery-1.9.1.js").SetVersion("1.9.1")
                .SetCdn("//ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.1.min.js", "//ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.1.js", true);

            manifest.DefineScript("helloWorld").SetUrl("helloWorld.js", "helloWorld.js").SetVersion("1.0");


            //watch youtube.com/watch?v=8zZzjDl1VeM (Ron Peterson - Orchard CMS Module - 15) Resource Manifest
        }
    }
}