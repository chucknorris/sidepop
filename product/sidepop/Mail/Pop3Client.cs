namespace sidepop.Mail
{
    using System.Collections.Generic;
    using System.Net.Sockets;
    using Mime;
    using Results;

    public interface Pop3Client
    {
        /// <summary>
        /// Gets the hostname.
        /// </summary>
        /// <value>The hostname.</value>
        string Hostname { get; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <value>The port.</value>
        int Port { get; }

        /// <summary>
        /// Gets a value indicating whether [use SSL].
        /// </summary>
        /// <value><c>true</c> if [use SSL]; otherwise, <c>false</c>.</value>
        bool UseSsl { get; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        string Password { get; set; }

        /// <summary>
        /// For when you need to get under the covers.
        /// </summary>
        TcpClient Client { get; }

        /// <summary>
        /// Gets the state of the current.
        /// </summary>
        /// <value>The state of the current.</value>
        Pop3State CurrentState { get; }

        /// <summary>
        /// Connects this instance and properly sets the 
        /// client stream to Use Ssl if it is specified.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        void Disconnect();

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
        void Authenticate();

        /// <summary>
        /// Executes the POP3 DELE command.
        /// </summary>
        /// <param name="item">The item.</param>
        /// /// <exception cref="Pop3Exception">If the DELE command was unable to be executed successfully.</exception>
        void Delete(Pop3ListItemResult item);

        /// <summary>
        /// Executes the POP3 NOOP command.
        /// </summary>
        /// <exception cref="Pop3Exception">If the NOOP command was unable to be executed successfully.</exception>
        void SendNoOperation();

        /// <summary>
        /// Executes the POP3 RSET command.
        /// </summary>
        /// <exception cref="Pop3Exception">If the RSET command was unable to be executed successfully.</exception>
        void SendReset();

        /// <summary>
        /// Executes the POP3 STAT command.
        /// </summary>
        /// <returns>A Stat object containing the results of STAT command.</returns>
        /// <exception cref="Pop3Exception">If the STAT command was unable to be executed successfully.</exception>
        StatisticsResult GetStatistics();

        /// <summary>
        /// Executes the POP3 List command.
        /// </summary>
        /// <returns>A generic List of Pop3Items containing the results of the LIST command.</returns>
        /// <exception cref="Pop3Exception">If the LIST command was unable to be executed successfully.</exception>
        List<Pop3ListItemResult> List();

        /// <summary>
        /// Retrieves the specified message.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>A MimeEntity for the requested Pop3 Mail Item.</returns>
        MimeEntity RetrieveMimeEntity(Pop3ListItemResult item);

        SidePOPMailMessage Top(int messageId, int lineCount);

        /// <summary>
        /// Retrieves the extended mail message.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        SidePOPMailMessage RetrieveExtendedMailMessage(Pop3ListItemResult item);

        /// <summary>
        /// Executes the Pop3 QUIT command.
        /// </summary>
        /// <exception cref="Pop3Exception">If the quit command returns a -ERR server message.</exception>
        void Quit();
    }
}