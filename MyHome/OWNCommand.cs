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
    public class OWNCommand
    {
        private string TAG = "Command - ";
        private HostName _host;
        private string _port;

        private StreamSocket _socket;
        private string _handshake = "*99*0##";
        private IOWNEventListener _listener;

        private DataReader reader;
        private DataWriter writer;

        public OWNCommand(string host, int port, IOWNEventListener listener)
        {
            this._host = new HostName(host);
            this._port = port.ToString();
            this._listener = listener;
        }

        public async Task Send(string[] messages)
        {
            // Stop any existing connections
            //Stop(null, "Stopping existing connection");

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
                        Debug.WriteLine(TAG + reader.ReadString(actualStringLength));
                        Debug.WriteLine(TAG + "Command session Connected");

                        // now send the handshake
                        byte[] hs = Encoding.ASCII.GetBytes(_handshake);
                        writer.WriteBytes(hs);
                        await writer.StoreAsync();
                        await writer.FlushAsync();

                        // Read the acknowledgement.
                        actualStringLength = await reader.LoadAsync(256);

                        // acknowledgement should be *#*1##
                        Debug.WriteLine(TAG + reader.ReadString(actualStringLength));
                        Debug.WriteLine(TAG + "Command handshake accepted");

                        foreach (string message in messages)
                        {
                            // now send the handshake
                            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                            writer.WriteBytes(messageBytes);
                            await writer.StoreAsync();
                            await writer.FlushAsync();

                            Logger.WriteLog(TAG, "Message " + message + " sent");

                            //// Read the string.
                            //actualStringLength = await reader.LoadAsync(256);

                            //string ownResponse = reader.ReadString(actualStringLength);
                            //Logger.WriteLog(TAG, ownResponse);

                            //_listener.handleEvent(ownResponse);
                            // Read the string.
                            actualStringLength = await reader.LoadAsync(256);

                            string respBuffer = "";
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

                        Logger.WriteLog(TAG, "All messages sent. Waiting for other responses ...");

                        writer.DetachStream();

                        
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

        //private async Task Connect()
        //{
        //    using (_socket = new StreamSocket())
        //    {
        //        // Set NoDelay to false so that the Nagle algorithm is not disabled
        //        _socket.Control.NoDelay = false;
        //        _socket.Control.KeepAlive = true;

        //        try
        //        {
        //            // Connect to the server
        //            await _socket.ConnectAsync(_host, _port);

        //            reader = new DataReader(_socket.InputStream);
        //            reader.InputStreamOptions = InputStreamOptions.Partial;

        //            writer = new DataWriter(_socket.OutputStream);

        //            try
        //            {
        //                // Read the acknowledgement.
        //                uint actualStringLength = await reader.LoadAsync(256);

        //                // acknowledgement should be *#*1##
        //                Debug.WriteLine(TAG + reader.ReadString(actualStringLength));
        //                Debug.WriteLine(TAG + "Command session Connected");

        //                // now send the handshake
        //                byte[] hs = Encoding.ASCII.GetBytes(_handshake);
        //                writer.WriteBytes(hs);
        //                await writer.StoreAsync();
        //                await writer.FlushAsync();

        //                // Read the acknowledgement.
        //                actualStringLength = await reader.LoadAsync(256);

        //                // acknowledgement should be *#*1##
        //                Debug.WriteLine(TAG + reader.ReadString(actualStringLength));
        //                Debug.WriteLine(TAG + "Command handshake accepted");
                        
        //            }
        //            catch (Exception exception)
        //            {
        //                Stop(exception, "Error reading data");
        //                // If this is an unknown status it means that the error is fatal and retry will likely fail.
        //                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
        //                {
        //                    throw;
        //                }

        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            switch (SocketError.GetStatus(exception.HResult))
        //            {
        //                case SocketErrorStatus.HostNotFound:
        //                    Stop(exception, "HostNotFound");
        //                    throw;
        //                default:
        //                    Stop(exception);
        //                    throw;
        //            }
        //        }
        //    }
        //}

        private void Stop(Exception exception, string message = null)
        {
            if (exception != null)
            {
                Debug.WriteLine(TAG + exception.Message);
                Debug.WriteLine(TAG + exception.StackTrace);
            }

            if (message != null)
            {
                Debug.WriteLine(TAG + message);
            }

            if (_socket != null)
            {
                try
                {
                    _socket.Dispose();
                    _socket = null;
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(TAG + exc.Message);
                }
                
            }
            
        }
    }
}
