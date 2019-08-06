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
        private RFIDEventArgs argument;

        public RFIDReader()
        {
            
        }

        /// <summary>
        /// Start the process
        /// </summary>
        public void Start()
        {
            this.SetupPort();
        }

        /// <summary>
        /// Stop the process
        /// </summary>
        public void Stop()
        {
            try
            {
                // cancel read task
                if (_readCancellationTokenSource != null)
                {
                    if (!_readCancellationTokenSource.IsCancellationRequested)
                    {
                        _readCancellationTokenSource.Cancel();
                    }
                }

                // close device
                if (_serialPort != null)
                {
                    _serialPort.Dispose();
                }
                _serialPort = null;
            }
            catch
            {
                return;
            }
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
                    this._serialPort.ReadTimeout = TimeSpan.FromMilliseconds((1000));
                    this._serialPort.BaudRate = 9600;
                    this._serialPort.DataBits = 8;
                    this._serialPort.StopBits = SerialStopBitCount.One;
                    this._serialPort.Parity = SerialParity.None;
                    this._serialPort.Handshake = SerialHandshake.None;
                    this._readCancellationTokenSource = new CancellationTokenSource();

                    this.Listen(this._serialPort);
                } else
                {
                    // if RFID Reader is not connected
                    throw new Exception("RFID Reader Not Found");
                }
            } catch
            {
                return;
            }
        }

        /// <summary>
        /// start listening to RFID tag data found
        /// </summary>
        private async void Listen(SerialDevice serialPort)
        {

            try
            {
                if (serialPort == null)
                {
                    throw new Exception("RFID Device not found");
                }

                this._dataReader = new DataReader(serialPort.InputStream);
                while (true)
                {
                    await this.ReadAsync(this._readCancellationTokenSource.Token, this._dataReader);
                }
            } catch
            {
                return;
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
        private async Task ReadAsync(CancellationToken cancellationToken, DataReader dataReader)
        {
            Task<UInt32> loadAsyncTask;
            uint ReadBufferLength = 1024;
            cancellationToken.ThrowIfCancellationRequested();

            // create task and wait for the data on the input stream
            loadAsyncTask = dataReader.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // If RFID device cannot read the tag, the below code will never be executed
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                this.UpdateStatus(dataReader.ReadString(bytesRead));
            }
        }

        /// <summary>
        /// Emit the RFID data to the event
        /// </summary>
        /// <param name="message"></param>
        private void UpdateStatus(string message)
        {
            this.argument = new RFIDEventArgs(message);

            if (this.OnDataTag == null)
            {
                return;
            }
            this.OnDataTag(this, this.argument);
        }
    }
}
