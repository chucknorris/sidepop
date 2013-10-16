using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using sidepop.Mail.Commands;
using sidepop.Mail.Responses;
using sidepop.Mail.Results;
using sidepop.Mime;

namespace sidepop.Mail
{
    using infrastructure.logging;
    using System.Text;

    /// <summary>
    /// The DefaultPop3Client class provides a wrapper for the Pop3 commands
    /// that can be executed against a Pop3Server.  This class will 
    /// execute and return results for the various commands that are 
    /// executed.
    /// </summary>
    public sealed class DefaultPop3Client : IDisposable, Pop3Client
    {
        private const int DefaultPort = 110;
        private const double DefaultTimeout = 1d;
        private TcpClient _client;
        private Stream _clientStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPop3Client"/> class using the default POP3 port 110
        /// without using SSL.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public DefaultPop3Client(string hostname, string username, string password)
            : this(hostname, DefaultPort, false, username, password, DefaultTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPop3Client"/> class using the default POP3 port 110.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="useSsl">if set to <c>true</c> [use SSL].</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public DefaultPop3Client(string hostname, bool useSsl, string username, string password)
            : this(hostname, DefaultPort, useSsl, username, password, DefaultTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPop3Client"/> class.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="port">The port.</param>
        /// <param name="useSsl">if set to <c>true</c> [use SSL].</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public DefaultPop3Client(string hostname, int port, bool useSsl, string username, string password)
            : this(hostname, port, useSsl, username, password, DefaultTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPop3Client"/> class.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="port">The port.</param>
        /// <param name="useSsl">if set to <c>true</c> [use SSL].</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="timeout">The amount of time to allow before timing out. Defaults to 1 minute</param>
        public DefaultPop3Client(string hostname, int port, bool useSsl, string username, string password, double timeout)
            : this()
        {
            if (string.IsNullOrEmpty(hostname))
            {
                throw new ArgumentNullException("hostname");
            }

            if (port < 0)
            {
                port = DefaultPort;
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }

            Hostname = hostname;
            Port = port;
            UseSsl = useSsl;
            Username = username;
            Password = password;
            Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPop3Client"/> class.
        /// </summary>
        private DefaultPop3Client()
        {
            _client = new TcpClient();
            CurrentState = Pop3State.Unknown;
        }

        /// <summary>
        /// Gets the hostname.
        /// </summary>
        /// <value>The hostname.</value>
        public string Hostname { get; private set; }

        /// <summary>
        /// For when you need to get under the covers.
        /// </summary>
        public TcpClient Client
        {
            get { return _client; }
        }

        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [use SSL].
        /// </summary>
        /// <value><c>true</c> if [use SSL]; otherwise, <c>false</c>.</value>
        public bool UseSsl { get; private set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; set; }

        /// <summary>
        /// The amount of time to wait on a call to the remote host.
        /// </summary>
        /// <value>The timeout</value>
        public double Timeout { get; set; }

        /// <summary>
        /// Gets the state of the current.
        /// </summary>
        /// <value>The state of the current.</value>
        public Pop3State CurrentState { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            Disconnect();
        }

        #endregion

        /// <summary>
        /// Checks the connection.
        /// </summary>
        private void EnsureConnection()
        {
            if (!_client.Connected)
            {
                throw new Pop3Exception("Pop3 client is not connected.");
            }
        }

        /// <summary>
        /// Resets the state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void SetState(Pop3State state)
        {
            CurrentState = state;
        }

        /// <summary>
        /// Ensures the response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="error">The error.</param>
        private static void EnsureResponse(Pop3Response response, string error)
        {
            if (response == null)
            {
                throw new Pop3Exception("Unable to get Response.  Response object null.");
            }

            if (response.StatusIndicator)
            {
                return;
            } //the command execution was successful.

            string errorMessage = string.IsNullOrEmpty(error) ? response.HostMessage : string.Concat(error, ": ", error);

            string responseContent = Encoding.ASCII.GetString( response.ResponseContents );
            throw new Pop3Exception(errorMessage + "\r\n\t" + responseContent.Replace("\n", "\n\t"));
        }

        /// <summary>
        /// Ensures the response.
        /// </summary>
        /// <param name="response">The response.</param>
        private static void EnsureResponse(Pop3Response response)
        {
            EnsureResponse(response, string.Empty);
        }

        /// <summary>
        /// Traces the command.
        /// </summary>
        /// <param name="command">The command.</param>
        private void TraceCommand<TCommand, TResponse>(TCommand command)
            where TCommand : Pop3Command<TResponse>
            where TResponse : Pop3Response
        {
            //Log.bound_to(this).log_a_debug_event_containing("{0} is running {1}",ApplicationParameters.name,command);
        }

        /// <summary>
        /// Connects this instance and properly sets the 
        /// client stream to Use Ssl if it is specified.
        /// </summary>
        public void Connect()
        {
            if (_client == null)
            {
                _client = new TcpClient();
            } //If a previous quit command was issued, the client would be disposed of.

            _client.ReceiveTimeout = (int)TimeSpan.FromMinutes(Timeout).TotalMilliseconds;
            _client.SendTimeout = (int)TimeSpan.FromMinutes(Timeout).TotalMilliseconds;

            if (_client.Connected)
            {
                return;
            } //if the connection already is established no need to reconnect.

            SetState(Pop3State.Unknown);
            ConnectResponse response;
            using (ConnectCommand command = new ConnectCommand(_client, Hostname, Port, UseSsl))
            {
                TraceCommand<ConnectCommand, ConnectResponse>(command);
                response = command.Execute(CurrentState);
                EnsureResponse(response);
            }

            SetClientStream(response.NetworkStream);

            SetState(Pop3State.Authorization);
        }

        /// <summary>
        /// Sets the client stream.  If UseSsl <c>true</c> then wrap 
        /// the client's <c>NetworkStream</c> in an <c>SslStream</c>, if UseSsl <c>false</c> 
        /// then set the client stream to the <c>NetworkStream</c>
        /// </summary>
        private void SetClientStream(Stream networkStream)
        {
            if (_clientStream != null)
            {
                _clientStream.Dispose();
            }

            _clientStream = networkStream;
        }

        /// <summary>
        /// Authenticates this instance.
        /// </summary>
        /// <remarks>A successful execution of this method will result in a Current State of Transaction.
        /// Unsuccessful USER or PASS commands can be reattempted by resetting the Username or Password 
        /// properties and re-execution of the methods.</remarks>
        /// <exception cref="Pop3Exception">
        /// If the Pop3Server is unable to be connected.
        /// If the User command is unable to be successfully executed.
        /// If the Pass command is unable to be successfully executed.
        /// </exception>
        public void Authenticate()
        {
            //Connect();

            //execute the user command.
            using (UserCommand userCommand = new UserCommand(_clientStream, Username))
            {
                ExecuteCommand<Pop3Response, UserCommand>(userCommand);
            }

            //execute the pass command.
            using (PassCommand passCommand = new PassCommand(_clientStream, Password))
            {
                ExecuteCommand<Pop3Response, PassCommand>(passCommand);
            }

            CurrentState = Pop3State.Transaction;
        }

        /// <summary>
        /// Executes the POP3 DELE command.
        /// </summary>
        /// <param name="item">The item.</param>
        /// /// <exception cref="Pop3Exception">If the DELE command was unable to be executed successfully.</exception>
        public void Delete(Pop3ListItemResult item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            using (DeleteCommand command = new DeleteCommand(_clientStream, item.MessageId))
            {
                ExecuteCommand<Pop3Response, DeleteCommand>(command);
            }
        }

        /// <summary>
        /// Executes the POP3 NOOP command.
        /// </summary>
        /// <exception cref="Pop3Exception">If the NOOP command was unable to be executed successfully.</exception>
        public void SendNoOperation()
        {
            using (NoOperationCommand command = new NoOperationCommand(_clientStream))
            {
                ExecuteCommand<Pop3Response, NoOperationCommand>(command);
            }
        }

        /// <summary>
        /// Executes the POP3 RSET command.
        /// </summary>
        /// <exception cref="Pop3Exception">If the RSET command was unable to be executed successfully.</exception>
        public void SendReset()
        {
            using (ResetCommand command = new ResetCommand(_clientStream))
            {
                ExecuteCommand<Pop3Response, ResetCommand>(command);
            }
        }

        /// <summary>
        /// Executes the POP3 STAT command.
        /// </summary>
        /// <returns>A Stat object containing the results of STAT command.</returns>
        /// <exception cref="Pop3Exception">If the STAT command was unable to be executed successfully.</exception>
        public StatisticsResult GetStatistics()
        {
            StatResponse response;
            using (StatisticsCommand command = new StatisticsCommand(_clientStream))
            {
                response = ExecuteCommand<StatResponse, StatisticsCommand>(command);
            }

            return new StatisticsResult(response.MessageCount, response.Octets);
        }

        /// <summary>
        /// Executes the POP3 List command.
        /// </summary>
        /// <returns>A generic List of Pop3Items containing the results of the LIST command.</returns>
        /// <exception cref="Pop3Exception">If the LIST command was unable to be executed successfully.</exception>
        public List<Pop3ListItemResult> List()
        {
            ListResponse response;
            using (ListCommand command = new ListCommand(_clientStream))
            {
                response = ExecuteCommand<ListResponse, ListCommand>(command);
            }
            return response.Items;
        }

        /// <summary>
        /// Lists the specified message.
        /// </summary>
        /// <param name="messageId">The message.</param>
        /// <returns>A <c>Pop3ListItemResult</c> for the requested Pop3Item.</returns>
        /// <exception cref="Pop3Exception">If the LIST command was unable to be executed successfully for the provided message id.</exception>
        public Pop3ListItemResult List(int messageId)
        {
            ListResponse response;
            using (ListCommand command = new ListCommand(_clientStream, messageId))
            {
                response = ExecuteCommand<ListResponse, ListCommand>(command);
            }
            return new Pop3ListItemResult(response.MessageNumber, response.Octets);
        }

        /// <summary>
        /// Retrieves the specified message.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>A MimeEntity for the requested Pop3 Mail Item.</returns>
        public byte[] Retrieve(Pop3ListItemResult item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (item.MessageId < 1)
            {
                throw new ArgumentOutOfRangeException(string.Format("{0}", item.MessageId));
            }

            RetrieveResponse response;
            using (RetrieveCommand command = new RetrieveCommand(_clientStream, item.MessageId))
            {
                response = ExecuteCommand<RetrieveResponse, RetrieveCommand>(command);
            }

            return response.RawBytes;
        }

        /// <summary>
        /// Retrieves the specified message.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>A MimeEntity for the requested Pop3 Mail Item.</returns>
        public MimeEntity RetrieveMimeEntity(Pop3ListItemResult item)
        {
            byte[] rawBytes = Retrieve(item);

            return MimeEntity.CreateFrom(rawBytes, false);
        }

        public SidePOPMailMessage Top(int messageId, int lineCount)
        {
            if (messageId < 1)
            {
                throw new ArgumentOutOfRangeException("messageId");
            }

            if (lineCount < 0)
            {
                throw new ArgumentOutOfRangeException("lineCount");
            }

            RetrieveResponse response;
            using (TopCommand command = new TopCommand(_clientStream, messageId, lineCount))
            {
                response = ExecuteCommand<RetrieveResponse, TopCommand>(command);
            }

            MimeReader reader = new MimeReader(response.RawBytes);
            MimeEntity entity = reader.CreateMimeEntity();
            entity.RawBytes = response.RawBytes;
            SidePOPMailMessage message = entity.ToMailMessageEx();
            message.Octets = response.Octets;
            message.MessageNumber = messageId;
            return entity.ToMailMessageEx();
        }

        /// <summary>
        /// Retrieves the extended mail message.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public SidePOPMailMessage RetrieveExtendedMailMessage(Pop3ListItemResult item)
        {
            SidePOPMailMessage message = RetrieveMimeEntity(item).ToMailMessageEx();
            message.MessageNumber = item.MessageId;
            return message;
        }

        /// <summary>
        /// Executes the Pop3 QUIT command.
        /// </summary>
        /// <exception cref="Pop3Exception">If the quit command returns a -ERR server message.</exception>
        public void Quit()
        {
            if (_clientStream == null)
            {
               return;
            }
            using (QuitCommand command = new QuitCommand(_clientStream))
            {
                if (_client.Connected)
                {
                    ExecuteCommand<Pop3Response, QuitCommand>(command);
                }
                
                if (CurrentState.Equals(Pop3State.Transaction))
                {
                    SetState(Pop3State.Update);
                } // Messages could have been deleted, reflect the server state.

                //Disconnect();

                //Quit command can only be called in Authorization or Transaction state, reset to Unknown.
                SetState(Pop3State.Unknown);
            }
        }

        /// <summary>
        /// Provides a common way to execute all commands.  This method
        /// validates the connection, traces the command and finally
        /// validates the response message for a -ERR response.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The Pop3Response for the provided command</returns>
        /// <exception cref="Pop3Exception">If the HostMessage does not start with '+OK'.</exception>
        /// <exception cref="Pop3Exception">If the client is no longer connected.</exception>
        private TResponse ExecuteCommand<TResponse, TCommand>(TCommand command)
            where TResponse : Pop3Response
            where TCommand : Pop3Command<TResponse>
        {
            EnsureConnection();
            TraceCommand<TCommand, TResponse>(command);
            TResponse response = command.Execute(CurrentState);
            EnsureResponse(response);
            return response;
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            if (_clientStream != null)
            {
                _clientStream.Close();
            } //release underlying socket.

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
            SetClientStream(null);
            SetState(Pop3State.Unknown);
        }
    }
}