using RFIDTagReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static RFIDTagReader.RFIDReader;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private RFIDReader _rfidReader;

        public MainPage()
        {
            this.InitializeComponent();
            this._rfidReader = new RFIDReader();
            this._rfidReader.OnDataTag += new RFIDDataHandler(this.GetTagData);
            this._rfidReader.Start();
        }

        private void GetTagData(object sender, RFIDEventArgs e)
        {
            // tag data
            string tag = e.TagData;
            Debug.WriteLine(tag);
        }

        // stop rfid tracking when page moved
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // stop rfid process
            this._rfidReader.Stop();
        }
    }
}
