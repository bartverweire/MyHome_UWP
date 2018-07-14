using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace MyHome
{
    public class OWNMonitor
    {
        private string TAG = "Monitor - ";
        private HostName _host;
        private string _port;

        private StreamSocket        _socket;
        private string              _handshake = "*99*1##";
        private IOWNEventListener    _listener;

        public OWNMonitor(string host, int port, IOWNEventListener listener)
        {
            this._host      = new HostName(host);
            this._port      = port.ToString();
            this._listener  = listener;
        }

        public async Task start()
        {
            if (_socket == null)
            {
                await this.connect();
            }
            
        }


        private async Task connect()
        {
            using (_socket = new StreamSocket())
            {
                // Set NoDelay to false so that the Nagle algorithm is not disabled
                _socket.Control.NoDelay = false;
                _socket.Control.KeepAlive = true;
                
                try
                {
                    // Connect to the server
                    await _socket.ConnectAsync(_host, _port);

                    DataReader reader = new DataReader(_socket.InputStream);
                    reader.InputStreamOptions = InputStreamOptions.Partial;

                    DataWriter writer = new DataWriter(_socket.OutputStream);

                    try
                    {
                        // Read the acknowledgement.
                        uint actualStringLength = await reader.LoadAsync(256);

                        // acknowledgement should be *#*1##
                        Debug.Write(TAG + reader.ReadString(actualStringLength));

                        // now send the handshake
                        byte[] hs = Encoding.ASCII.GetBytes(_handshake);
                        writer.WriteBytes(hs);
                        await writer.StoreAsync();
                        await writer.FlushAsync();

                        // Read the acknowledgement.
                        actualStringLength = await reader.LoadAsync(256);

                        // acknowledgement should be *#*1##
                        Debug.WriteLine(TAG + reader.ReadString(actualStringLength));

                        string respBuffer = "";

                        // wait for other messages
                        while (true)
                        {
                            // Read the string.
                            actualStringLength = await reader.LoadAsync(256);

                            string respPart = "";
                            // Keep reading until we consume the complete stream.
                            while (reader.UnconsumedBufferLength > 0)
                            {
                                respPart += reader.ReadString(reader.UnconsumedBufferLength);
                            }

                            respBuffer = respBuffer + respPart;

                            Logger.WriteLog(TAG, respBuffer);
                            int splitPos;
                            while ((splitPos = respBuffer.IndexOf("##")) >= 0)
                            {
                                string ownResponse = respBuffer.Substring(0, respBuffer.IndexOf("##") + 2);
                                respBuffer = respBuffer.Substring(respBuffer.IndexOf("##") + 2);

                                Logger.WriteLog(TAG, "Sending " + ownResponse + " to event handler, and keeping " + respBuffer + " in buffer");
                                _listener.handleEvent(ownResponse);
                            }
                            
                        }
                    }
                    catch (Exception exception)
                    {
                        Stop(exception, "Error reading data");
                        // If this is an unknown status it means that the error is fatal and retry will likely fail.
                        if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                        {
                            throw;
                        }

                    }
                }
                catch (Exception exception)
                {
                    switch (SocketError.GetStatus(exception.HResult))
                    {
                        case SocketErrorStatus.HostNotFound:
                            Stop(exception, "HostNotFound");
                            throw;
                        default:
                            Stop(exception);
                            throw;
                    }
                }
            }
        }

        private async Task send(string message)
        {
            DataWriter writer;

            // Create the data writer object backed by the in-memory stream. 
            using (writer = new DataWriter(_socket.OutputStream))
            {
                // Send the contents of the writer to the backing stream.
                try
                {
                    await writer.StoreAsync();
                }
                catch (Exception exception)
                {
                    switch (SocketError.GetStatus(exception.HResult))
                    {
                        case SocketErrorStatus.HostNotFound:
                            Stop(exception, "HostNotFound");
                            throw;
                        default:
                            Stop(exception);
                            throw;
                    }
                }

                await writer.FlushAsync();
                // In order to prolong the lifetime of the stream, detach it from the DataWriter
                writer.DetachStream();
            }
        }

        private void Stop(Exception exception, string message = null)
        {
            if (exception != null)
            {
                Logger.WriteLog(TAG, exception.Message);
            }
            
            if (message != null)
            {
                Logger.WriteLog(TAG, message);
            }

            _socket.Dispose();
            _socket = null;
        }
    }
}
