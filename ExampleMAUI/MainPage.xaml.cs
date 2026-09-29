using FaviconFetcher;
using System.Collections.ObjectModel;

namespace ExampleMAUI
{
    public partial class MainPage : ContentPage
    {
        private bool _isScanning = false;
        private bool _isFetching = false;
        private CancellationTokenSource? _cancellationTokenSource = null;
        private IconImage? _image = null;

        public string URIToScanOrFetch { get; set; } = string.Empty;
        public int MinSize { get; set; } = 16;
        public int MaxSize { get; set; } = 64;
        public int PerfectSize { get; set; } = 64;
        public Image? FetchedImage { get; set; } = null;
        public ObservableCollection<ScanResult> ScanResults { get; set; } = new ObservableCollection<ScanResult>() { };

        public MainPage()
        {
            InitializeComponent();

            BindingContext = this;
        }

        private async void ScanButton_Clicked(object sender, EventArgs e)
        {
            ScanResultsListView.IsVisible = false;
            ScanResults.Clear();

            try
            {
                if (_isScanning)
                {
                    _cancellationTokenSource?.Cancel();
                    return;
                }

                _isScanning = true;
                ((Button)sender).Text = "Cancel";

                var uri = new Uri(URIToScanOrFetch);

                _cancellationTokenSource = new CancellationTokenSource();

                foreach (var result in await new Scanner().Scan(uri, _cancellationTokenSource))
                {
                    ScanResults.Add(result);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _isScanning = false;
                ((Button)sender).Text = "Scan";
                _cancellationTokenSource?.Dispose();
            }

            ScanResultsListView.IsVisible = true;
        }

        private async void FetchButton_Clicked(object sender, EventArgs e)
        {
            FetchResultsImageGrid.IsVisible = false;
            FetchResultsImage.Source = null;

            if (_isFetching)
            {
                _cancellationTokenSource?.Cancel();
                return;
            }

            _isFetching = true;
            ((Button)sender).Text = "Cancel";

            try
            {
                var uri = new Uri(URIToScanOrFetch);

                _cancellationTokenSource = new CancellationTokenSource();

                FetchResultsImage.WidthRequest = 100;

                FetchResultsImage.WidthRequest = 16;
                FetchResultsImage.HeightRequest = 16;
                FetchResultsImage.Source = null;

                _image = await new Fetcher().Fetch(
                    uri,
                    new FetchOptions
                    {
                        MinimumSize = new IconSize(MinSize, MinSize),
                        MaximumSize = new IconSize(MaxSize, MaxSize),
                        PerfectSize = new IconSize(PerfectSize, PerfectSize)
                    },
                    _cancellationTokenSource);

                if (_image != null)
                {
                    FetchResultsImage.WidthRequest = _image.Size.Width;
                    FetchResultsImage.HeightRequest = _image.Size.Height;
                    FetchResultsImage.Source = ImageSource.FromStream(() => _image.ToSKBitmap().Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).AsStream(true));
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _isFetching = false;
                ((Button)sender).Text = "Fetch";
            }

            FetchResultsImageGrid.IsVisible = true;
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            if (_image == null) return;

            if (!_image.Save(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test.png")
                ))
            {
                await DisplayAlert("Error", "Error saving file", "OK");
            }
        }
    }

}
