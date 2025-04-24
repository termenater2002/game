using Unity.Muse.Animate.Usd;

namespace Unity.Muse.Animate
{
    readonly struct LoadLibraryItemAssetMessage
    {
        public readonly string Path;
        public LoadLibraryItemAssetMessage(string path)
        {
            Path = path;
        }
    }
    
    readonly struct SaveLibraryItemAssetMessage
    {
        public readonly LibraryItemAsset ItemAsset;
        public SaveLibraryItemAssetMessage(LibraryItemAsset itemAsset)
        {
            ItemAsset = itemAsset;
        }
    }
    
    readonly struct ExportAnimationMessage
    {
        public readonly LibraryItemAsset Asset;
        public readonly ExportData ExportData;
        public readonly ApplicationLibraryModel.ExportType ExportType;
        public readonly ApplicationLibraryModel.ExportFlow ExportFlow;
		public readonly string FileName;
		
        public ExportAnimationMessage(ApplicationLibraryModel.ExportType exportType, LibraryItemAsset asset, ExportData exportData, string fileName, ApplicationLibraryModel.ExportFlow exportFlow)
        {
            Asset = asset;
            ExportType = exportType;
            ExportData = exportData;
            FileName = fileName;
            ExportFlow = exportFlow;
        }
    }
}
