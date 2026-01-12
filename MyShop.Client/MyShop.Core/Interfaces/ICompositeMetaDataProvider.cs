using Microsoft.UI.Xaml.Markup;

namespace MyShop.Core.Interfaces
{
    public interface ICompositeMetaDataProvider
    {
        void AddProvider(IXamlMetadataProvider provider);
        IXamlType GetXamlType(Type type);
        IXamlType GetXamlType(string fullName);
        XmlnsDefinition[] GetXmlnsDefinitions();
    }
}
