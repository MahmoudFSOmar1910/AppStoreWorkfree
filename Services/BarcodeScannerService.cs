using System;
using System.Threading.Tasks;
using ZXing.Mobile;
using Xamarin.Forms;

namespace DirectSalesApp.Services
{
    public class BarcodeScannerService
    {
        private readonly MobileBarcodeScanner _scanner;

        public BarcodeScannerService()
        {
            _scanner = new MobileBarcodeScanner();
            _scanner.AutoFocus();
        }

        public async Task<string> ScanAsync(string topText = "Position the barcode in the scanner view")
        {
            _scanner.TopText = topText;
            _scanner.BottomText = "Scanning will happen automatically";

            var scanResult = await _scanner.Scan();

            if (scanResult == null)
                return null;

            return scanResult.Text;
        }

        public void Cancel()
        {
            _scanner.Cancel();
        }

        public void ToggleTorch()
        {
            _scanner.ToggleTorch();
        }

        public async Task<bool> IsAvailableAsync()
        {
            return await MobileBarcodeScanner.Current.IsTorchAvailable();
        }
    }
} 