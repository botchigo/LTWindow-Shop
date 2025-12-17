using System.Threading.Tasks;
using Windows.Storage;

namespace MyShop.Services
{
    public interface IFilePickerService
    {
        Task<StorageFile> PickImportFileAsync();
    }
}
