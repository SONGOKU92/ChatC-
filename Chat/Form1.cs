using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatServerGUI
{
    public partial class ServerForm : Form
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<TcpClient> clients = new List<TcpClient>();
        private bool isRunning = false;
        private int port = 8888;

        public ServerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnStartServer = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 15);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(120, 16);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Serveur: Arrêté";
            // 
            // btnStartServer
            // 
            this.btnStartServer.Location = new System.Drawing.Point(282, 12);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(190, 23);
            this.btnStartServer.TabIndex = 1;
            this.btnStartServer.Text = "Démarrer le serveur";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 49);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(460, 300);
            this.txtLog.TabIndex = 2;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(219, 12);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(57, 22);
            this.txtPort.TabIndex = 3;
            this.txtPort.Text = "8888";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(176, 15);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(37, 16);
            this.lblPort.TabIndex = 4;
            this.lblPort.Text = "Port:";
            // 
            // ServerForm
            // 
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnStartServer);
            this.Controls.Add(this.lblStatus);
            this.Name = "ServerForm";
            this.Text = "Serveur de Chat";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblPort;

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                StartServer();
                btnStartServer.Text = "Arrêter le serveur";
                lblStatus.Text = "Serveur: En marche";
                isRunning = true;
            }
            else
            {
                StopServer();
                btnStartServer.Text = "Démarrer le serveur";
                lblStatus.Text = "Serveur: Arrêté";
                isRunning = false;
            }
        }

        private void StartServer()

        {

            try

            {

                // Analyser le port à partir de la zone de texte

                if (!int.TryParse(txtPort.Text, out port))

                {

                    port = 8888;

                    txtPort.Text = port.ToString();

                }



                // Démarrer le TcpListener

                tcpListener = new TcpListener(IPAddress.Any, port);

                tcpListener.Start();



                // Démarrer le thread d'écoute après avoir démarré le TcpListener

                listenThread = new Thread(new ThreadStart(ListenForClients));

                listenThread.Start();



                LogMessage($"Serveur démarré sur le port {port}");

            }

            catch (Exception ex)

            {

                LogMessage($"Erreur lors du démarrage du serveur: {ex.Message}");

            }

        }


        private void StopServer()
        {
            try
            {
                // Arrêter le serveur
                isRunning = false;
                tcpListener.Stop();

                // Fermer toutes les connexions clients
                foreach (TcpClient client in clients)
                {
                    client.Close();
                }
                clients.Clear();

                // Abandonner le thread d'écoute
                if (listenThread != null && listenThread.IsAlive)
                {
                    listenThread.Abort();
                }

                LogMessage("Serveur arrêté");
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de l'arrêt du serveur: {ex.Message}");
            }
        }

        private void ListenForClients()
        {
            isRunning = true;

            try
            {
                while (isRunning)
                {
                    // Accepter la connexion d'un client
                    TcpClient client = tcpListener.AcceptTcpClient();
                    clients.Add(client);

                    // Obtenir l'adresse IP du client
                    IPEndPoint clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    string clientIP = clientEndPoint.Address.ToString();

                    LogMessage($"Nouveau client connecté: {clientIP}");

                    // Diffuser le message de connexion à tous les clients
                    string welcomeMsg = $"Nouveau client connecté: {clientIP}";
                    BroadcastMessage(welcomeMsg, null);

                    // Créer un thread pour gérer le client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);
                }
            }
            catch (ThreadAbortException)
            {
                // Le thread a été arrêté, ne rien faire
            }
            catch (Exception ex)
            {
                if (isRunning)
                {
                    LogMessage($"Erreur dans le thread d'écoute: {ex.Message}");
                }
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            IPEndPoint clientEndPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
            string clientIP = clientEndPoint.Address.ToString();

            byte[] message = new byte[4096];
            int bytesRead;

            try
            {
                while (isRunning && tcpClient.Connected)
                {
                    bytesRead = 0;

                    // Lire le message du client
                    try
                    {
                        bytesRead = clientStream.Read(message, 0, 4096);
                    }
                    catch
                    {
                        // Client déconnecté
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        // Client déconnecté
                        break;
                    }

                    // Convertir les octets du message en chaîne
                    string clientMessage = Encoding.UTF8.GetString(message, 0, bytesRead);

                    // Consigner le message
                    LogMessage($"Message de {clientIP}: {clientMessage}");

                    // Diffuser le message à tous les clients
                    BroadcastMessage($"{clientIP}: {clientMessage}", tcpClient);
                }

                // Client déconnecté, le supprimer de la liste
                clients.Remove(tcpClient);
                tcpClient.Close();
                LogMessage($"Client déconnecté: {clientIP}");
                BroadcastMessage($"Client déconnecté: {clientIP}", null);
            }
            catch (Exception ex)
            {
                if (isRunning)
                {
                    LogMessage($"Erreur lors de la communication avec le client: {ex.Message}");
                }

                // Supprimer le client de la liste
                if (clients.Contains(tcpClient))
                {
                    clients.Remove(tcpClient);
                    tcpClient.Close();
                    LogMessage($"Client déconnecté: {clientIP}");
                    BroadcastMessage($"Client déconnecté: {clientIP}", null);
                }
            }
        }

        private void BroadcastMessage(string message, TcpClient excludeClient)
        {
            // Convertir le message en octets
            byte[] broadcastBytes = Encoding.UTF8.GetBytes(message);

            // Envoyer à tous les clients sauf l'expéditeur
            foreach (TcpClient client in clients)
            {
                if (client != excludeClient && client.Connected)
                {
                    try
                    {
                        NetworkStream clientStream = client.GetStream();
                        clientStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                        clientStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Erreur lors de l'envoi du message: {ex.Message}");
                    }
                }
            }
        }

        private void LogMessage(string message)
        {
            // Consigner le message de manière sécurisée pour le thread de l'interface utilisateur
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(LogMessage), new object[] { message });
            }
            else
            {
                txtLog.AppendText($"[{DateTime.Now}] {message}{Environment.NewLine}");
                txtLog.ScrollToCaret();
            }
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Arrêter le serveur lorsque le formulaire se ferme
            if (isRunning)
            {
                StopServer();
            }
        }
    }
}