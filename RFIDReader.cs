using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace RFIDTagReader
{
    public class RFIDReader
    {
        public delegate void RFIDDataHandler(object sender, RFIDEventArgs e);
        public event RFIDDataHandler OnDataTag;

        private SerialDevice _serialPort;
        private DataReader _dataReader;
        private ObservableCollection<DeviceInformation> _deviceList;
        private CancellationTokenSource _readCancellationTokenSource;

        public RFIDReader()
        {
            this.SetupPort();
        }

        private async void SetupPort()
        {
            try
            {
                string devSel = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(devSel);

                if (dis.Count > 0)
                {
                    DeviceInformation entry = (DeviceInformation)dis[0];
                    this._serialPort = await SerialDevice.FromIdAsync(entry.Id);
                    this._serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                    this._serialPort.BaudRate = 9600;
                    this._serialPort.DataBits = 8;
                    this._serialPort.StopBits = SerialStopBitCount.One;
                    this._serialPort.Parity = SerialParity.None;
                    this._serialPort.Handshake = SerialHandshake.None;
                    this._readCancellationTokenSource = new CancellationTokenSource();

                    this.Listen();
                }
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// start listening to RFID tag data found
        /// </summary>
        private async void Listen()
        {
            if (this._serialPort == null)
            {
                return;
            }

            try
            {
                this._dataReader = new DataReader(this._serialPort.InputStream);
                while (true)
                {
                    await this.ReadAsync(this._readCancellationTokenSource.Token);
                }
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            } finally
            {
                if (this._dataReader != null)
                {
                    this._dataReader.DetachStream();
                    this._dataReader = null;
                }
            }
        }

        /// <summary>
        /// Read async data from the input stream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;
            uint ReadBufferLength = 1024;
            cancellationToken.ThrowIfCancellationRequested();

            // create task and wait for the data on the input stream
            loadAsyncTask = this._dataReader.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                RFIDEventArgs argument = new RFIDEventArgs(this._dataReader.ReadString(bytesRead));

                if (OnDataTag == null)
                {
                    return;
                }
                OnDataTag(this, argument);
            }
        }
    }
}
