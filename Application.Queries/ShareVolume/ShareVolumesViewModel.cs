namespace Application.Queries.ShareVolume;

public sealed class ShareVolumesViewModel
{
    public List<ShareVolumeViewModel> ShareVolumes { get; set; } = new();

    public ShareVolumesViewModel() { }

    public ShareVolumesViewModel(IEnumerable<ShareVolumeViewModel> shareVolumes)
    {
        ShareVolumes.AddRange(shareVolumes);
    }

    public class ShareVolumeViewModel
    {
        public string Symbol { get; set; }

        public int Volume { get; set; }

        public ShareVolumeViewModel() { }

        public ShareVolumeViewModel(string symbol, int volume)
        {
            Symbol = symbol;
            Volume = volume;
        }
    }
}