using System;
using System.Windows;
using System.Windows.Interop;

namespace WPF_NFCSample_01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region fields

        NFCService nfcService = null;

        #endregion

        public MainWindow()
        {
            InitializeComponent();        
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
            InitializeNFCListener(windowHandle);

        }

        void InitializeNFCListener(IntPtr window)
        {
            nfcService = new NFCService();
            nfcService.Initialize(window);
            nfcService.MessageReceived += OnNFCMessageReceived;
        }

        void OnNFCMessageReceived(object sender, NFCMessageEventArgs args)
        {
            var eventType = args.EventType;
            var messageData = args.Data;

            this.txtEventType.Text = eventType.ToString();
            this.txtEventData.Text = messageData;

        }
    }
}
