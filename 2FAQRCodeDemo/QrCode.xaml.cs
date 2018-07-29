using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.QrCode;

namespace _2FAQRCodeDemo
{
    public enum ErrorCorrection
    {
        /// <summary> M = ~15% correction</summary>
        M,
        /// <summary> L = ~7% correction</summary>
        L,
        /// <summary> H = ~30% correction</summary>
        H,
        /// <summary> Q = ~25% correction</summary>
        Q
    }

    /// <summary>
    /// Interaction logic for QrCode.xaml
    /// </summary>
    public partial class QrCode : UserControl
    {
        #region Dependency Properties

        public static DependencyProperty UriProperty = DependencyProperty.Register(nameof(Uri), typeof(string),
            typeof(QrCode), new PropertyMetadata(null, PropertyChangedCallback));

        public string Uri
        {
            get => (string) GetValue(UriProperty);
            set => SetValue(UriProperty, value);
        }

        public static DependencyProperty ContentMarginProperty = DependencyProperty.Register(nameof(ContentMargin),
            typeof(int), typeof(QrCode), new PropertyMetadata(0, PropertyChangedCallback));

        public int ContentMargin
        {
            get => (int) GetValue(ContentMarginProperty);
            set => SetValue(ContentMarginProperty, value);
        }

        public static DependencyProperty VersionProperty = DependencyProperty.Register(nameof(Version), typeof(int?),
            typeof(QrCode), new PropertyMetadata(null, PropertyChangedCallback));

        public int? Version
        {
            get => (int?) GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        public static DependencyProperty ErrorCorrectionLevelProperty =
            DependencyProperty.Register(nameof(ErrorCorrectionLevel), typeof(ErrorCorrection?), typeof(QrCode),
                new PropertyMetadata(null, PropertyChangedCallback));

        public ErrorCorrection? ErrorCorrectionLevel
        {
            get => (ErrorCorrection?) GetValue(ErrorCorrectionLevelProperty);
            set => SetValue(ErrorCorrectionLevelProperty, value);
        }

        private static DependencyProperty DrawWidthProperty = DependencyProperty.Register(nameof(DrawWidth),
            typeof(int), typeof(QrCode), new PropertyMetadata(1, PropertyChangedCallback));

        private int DrawWidth
        {
            get => (int) GetValue(DrawWidthProperty);
            set => SetValue(DrawWidthProperty, value);
        }

        private static DependencyProperty DrawHeightProperty = DependencyProperty.Register(nameof(DrawHeight),
            typeof(int), typeof(QrCode), new PropertyMetadata(1, PropertyChangedCallback));

        private int DrawHeight
        {
            get => (int)GetValue(DrawHeightProperty);
            set => SetValue(DrawHeightProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((QrCode)d).GenerateQrCode();
        }

        #endregion

        #region Construction

        public QrCode()
        {
            Loaded += (sender, args) => GenerateQrCode();
            SetBinding(DrawWidthProperty, new Binding(WidthProperty.Name) {RelativeSource = RelativeSource.Self});
            SetBinding(DrawHeightProperty, new Binding(HeightProperty.Name) { RelativeSource = RelativeSource.Self });
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void GenerateQrCode()
        {
            if (!IsLoaded) return;

            try
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions()
                    {
                        Width = DrawWidth,
                        Height = DrawHeight,
                        PureBarcode = true,
                        Margin = ContentMargin,
                        ErrorCorrection = ErrorCorrectionLevel.HasValue
                            ? ZXing.QrCode.Internal.ErrorCorrectionLevel.forBits((int) ErrorCorrectionLevel)
                            : null,
                        QrVersion = Version
                    }
                };
                var bitmap = writer.Write(Uri);

                Image.Source = Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
                ToolTip = "QR Code";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                // Build a box with an x through it when we can't draw the QR Code
                const int dim = 100;
                var drawing = new GeometryDrawing(Brushes.Transparent, new Pen(Brushes.Black, 2), new GeometryGroup
                {
                    Children =
                    {
                        new LineGeometry(new Point(0, 0), new Point(dim, dim)),
                        new LineGeometry(new Point(0, dim), new Point(dim, 0)),
                        new RectangleGeometry(new Rect(new Size(dim, dim)))
                    }
                });

                var geometry = new DrawingImage(drawing);
                geometry.Freeze();

                Image.Source = geometry;
                ToolTip = e.Message;
            }
        }

        #endregion
    }
}
