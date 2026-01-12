using Microsoft.UI.Xaml.Markup;
using MyShop.Core.Interfaces;

namespace MyShop.Infrastructure.Services
{
    public class CompositeMetadataProvider : IXamlMetadataProvider, ICompositeMetaDataProvider
    {
        private List<IXamlMetadataProvider> _providers = new List<IXamlMetadataProvider>();

        public void AddProvider(IXamlMetadataProvider provider)
        {
            if (provider != null && !_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }

        public IXamlType GetXamlType(Type type)
        {
            foreach (var provider in _providers)
            {
                var result = provider.GetXamlType(type);
                if (result != null) return result;
            }
            return null!;
        }

        public IXamlType GetXamlType(string fullName)
        {
            foreach (var provider in _providers)
            {
                var result = provider.GetXamlType(fullName);
                if (result != null) return result;
            }
            return null!;
        }

        public XmlnsDefinition[] GetXmlnsDefinitions()
        {
            var definitions = new List<XmlnsDefinition>();
            foreach (var provider in _providers)
            {
                var defs = provider.GetXmlnsDefinitions();
                if (defs != null) definitions.AddRange(defs);
            }
            return definitions.ToArray();
        }
    }
}
