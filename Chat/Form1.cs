using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServerGUI
{
    public partial class ServerForm : Form
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<ClientInfo> clients = new List<ClientInfo>();
        private bool isRunning = false;
        private bool isClosing = false; // Flag pour indiquer que l'application est en cours de fermeture
        private int port = 8888;
        private int maxMessageHistory = 10;
        private List<string> messageHistory = new List<string>();
        private CancellationTokenSource cts;

        // Classe pour stocker les informations des clients
        private class ClientInfo
        {
            public TcpClient Client { get; set; }
            public string IP { get; set; }
            public string Username { get; set; }
            public DateTime LastActivity { get; set; }

            public ClientInfo(TcpClient client, string ip)
            {
                Client = client;
                IP = ip;
                Username = $"User_{ip.Replace(".", "_")}";
                LastActivity = DateTime.Now;
            }
        }

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
            this.lblConnectedClients = new System.Windows.Forms.Label();
            this.txtServerMessage = new System.Windows.Forms.TextBox();
            this.btnSendServerMessage = new System.Windows.Forms.Button();
            this.lvUsers = new System.Windows.Forms.ListView();
            this.columnIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnUsername = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnLastActivity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblUsers = new System.Windows.Forms.Label();
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
            this.txtLog.Size = new System.Drawing.Size(460, 260);
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
            // lblConnectedClients
            // 
            this.lblConnectedClients.AutoSize = true;
            this.lblConnectedClients.Location = new System.Drawing.Point(12, 332);
            this.lblConnectedClients.Name = "lblConnectedClients";
            this.lblConnectedClients.Size = new System.Drawing.Size(103, 16);
            this.lblConnectedClients.TabIndex = 5;
            this.lblConnectedClients.Text = "Clients: 0";
            // 
            // txtServerMessage
            // 
            this.txtServerMessage.Location = new System.Drawing.Point(12, 360);
            this.txtServerMessage.Name = "txtServerMessage";
            this.txtServerMessage.Size = new System.Drawing.Size(365, 22);
            this.txtServerMessage.TabIndex = 6;
            this.txtServerMessage.Enabled = false;
            // 
            // btnSendServerMessage
            // 
            this.btnSendServerMessage.Location = new System.Drawing.Point(383, 360);
            this.btnSendServerMessage.Name = "btnSendServerMessage";
            this.btnSendServerMessage.Size = new System.Drawing.Size(89, 23);
            this.btnSendServerMessage.TabIndex = 7;
            this.btnSendServerMessage.Text = "Envoyer";
            this.btnSendServerMessage.UseVisualStyleBackColor = true;
            this.btnSendServerMessage.Enabled = false;
            this.btnSendServerMessage.Click += new System.EventHandler(this.btnSendServerMessage_Click);
            // 
            // lvUsers
            // 
            this.lvUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnIP,
            this.columnUsername,
            this.columnLastActivity});
            this.lvUsers.HideSelection = false;
            this.lvUsers.Location = new System.Drawing.Point(12, 410);
            this.lvUsers.Name = "lvUsers";
            this.lvUsers.Size = new System.Drawing.Size(460, 150);
            this.lvUsers.TabIndex = 8;
            this.lvUsers.UseCompatibleStateImageBehavior = false;
            this.lvUsers.View = System.Windows.Forms.View.Details;
            // 
            // columnIP
            // 
            this.columnIP.Text = "Adresse IP";
            this.columnIP.Width = 120;
            // 
            // columnUsername
            // 
            this.columnUsername.Text = "Nom d'utilisateur";
            this.columnUsername.Width = 150;
            // 
            // columnLastActivity
            // 
            this.columnLastActivity.Text = "Dernière activité";
            this.columnLastActivity.Width = 180;
            //
            // lblUsers
            //
            this.lblUsers.AutoSize = true;
            this.lblUsers.Location = new System.Drawing.Point(12, 390);
            this.lblUsers.Name = "lblUsers";
            this.lblUsers.Size = new System.Drawing.Size(120, 16);
            this.lblUsers.TabIndex = 9;
            this.lblUsers.Text = "Utilisateurs connectés:";
            // 
            // ServerForm
            // 
            this.ClientSize = new System.Drawing.Size(484, 570);
            this.Controls.Add(this.lblUsers);
            this.Controls.Add(this.lvUsers);
            this.Controls.Add(this.btnSendServerMessage);
            this.Controls.Add(this.txtServerMessage);
            this.Controls.Add(this.lblConnectedClients);
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
        private System.Windows.Forms.Label lblConnectedClients;
        private System.Windows.Forms.TextBox txtServerMessage;
        private System.Windows.Forms.Button btnSendServerMessage;
        private System.Windows.Forms.ListView lvUsers;
        private System.Windows.Forms.ColumnHeader columnIP;
        private System.Windows.Forms.ColumnHeader columnUsername;
        private System.Windows.Forms.ColumnHeader columnLastActivity;
        private System.Windows.Forms.Label lblUsers;

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                StartServer();
                btnStartServer.Text = "Arrêter le serveur";
                lblStatus.Text = "Serveur: En marche";
                txtServerMessage.Enabled = true;
                btnSendServerMessage.Enabled = true;
                isRunning = true;
            }
            else
            {
                StopServer();
                btnStartServer.Text = "Démarrer le serveur";
                lblStatus.Text = "Serveur: Arrêté";
                txtServerMessage.Enabled = false;
                btnSendServerMessage.Enabled = false;
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

                // Initialiser le jeton d'annulation
                cts = new CancellationTokenSource();

                // Démarrer le TcpListener
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();

                // Démarrer le thread d'écoute après avoir démarré le TcpListener
                listenThread = new Thread(new ThreadStart(ListenForClients));
                listenThread.IsBackground = true;
                listenThread.Start();

                // Démarrer le thread pour vérifier les clients inactifs
                Task.Run(() => CheckInactiveClients(cts.Token));

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
                // Indiquer que le serveur est en cours d'arrêt
                LogMessage("Arrêt du serveur en cours...");

                // Arrêter le serveur
                isRunning = false;

                // Annuler les tâches en cours
                if (cts != null)
                {
                    cts.Cancel();
                }

                // Envoyer un message d'arrêt du serveur
                BroadcastMessage("Le serveur va s'arrêter. Vous allez être déconnecté.", null, true);

                // Donner aux clients le temps de recevoir le message
                Thread.Sleep(500);

                // Forcer la fermeture du TcpListener pour que le thread d'écoute se débloque
                if (tcpListener != null)
                {
                    try
                    {
                        tcpListener.Stop();
                        LogMessage("TcpListener arrêté");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Erreur lors de l'arrêt du TcpListener: {ex.Message}");
                    }
                }

                // Attendre que le thread d'écoute se termine
                if (listenThread != null && listenThread.IsAlive)
                {
                    try
                    {
                        // Donner le temps au thread d'écoute de se terminer naturellement
                        if (!listenThread.Join(1000))
                        {
                            // Si le thread ne s'est pas terminé, l'interrompre
                            listenThread.Interrupt();
                            LogMessage("Thread d'écoute interrompu");

                            // Dernière option si l'interruption ne fonctionne pas
                            if (!listenThread.Join(500))
                            {
                                // Attention: Abort est déconseillé et peut causer des problèmes
                                // mais nous l'utilisons comme dernier recours
                                listenThread.Abort();
                                LogMessage("Thread d'écoute arrêté de force");
                            }
                        }
                        else
                        {
                            LogMessage("Thread d'écoute terminé normalement");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Erreur lors de l'arrêt du thread d'écoute: {ex.Message}");
                    }
                }

                // Fermer toutes les connexions clients
                lock (clients)
                {
                    foreach (ClientInfo client in clients.ToList())
                    {
                        try
                        {
                            client.Client.Close();
                            LogMessage($"Client {client.Username} déconnecté");
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Erreur lors de la fermeture du client {client.Username}: {ex.Message}");
                        }
                    }
                    clients.Clear();
                }

                UpdateClientCount();
                UpdateClientListView();
                LogMessage("Serveur arrêté avec succès");
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
                while (isRunning && !isClosing)
                {
                    try
                    {
                        // Accepter la connexion d'un client
                        TcpClient client = tcpListener.AcceptTcpClient();

                        // Obtenir l'adresse IP du client
                        IPEndPoint clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                        string clientIP = clientEndPoint.Address.ToString();

                        // Créer un nouvel objet ClientInfo
                        ClientInfo clientInfo = new ClientInfo(client, clientIP);

                        // Ajouter le client à la liste
                        lock (clients)
                        {
                            clients.Add(clientInfo);
                            UpdateClientCount();
                            UpdateClientListView();
                        }

                        LogMessage($"Nouveau client connecté: {clientIP}");

                        // Envoyer les règles du chat au client
                        SendWelcomeMessage(clientInfo);

                        // Diffuser le message de connexion à tous les clients
                        string connectMsg = $"{clientInfo.Username} a rejoint le chat";
                        BroadcastMessage(connectMsg, null);

                        // Ajouter à l'historique
                        AddToHistory(connectMsg);

                        // Créer un thread pour gérer le client
                        Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                        clientThread.IsBackground = true;
                        clientThread.Start(clientInfo);
                    }
                    catch (SocketException)
                    {
                        // Cette exception est attendue quand tcpListener.Stop() est appelé
                        if (!isRunning || isClosing)
                        {
                            LogMessage("Arrêt normal du listener");
                            break;
                        }
                        else
                        {
                            LogMessage("Erreur de socket pendant l'écoute. Tentative de reprise...");
                            // Petite pause pour éviter une consommation CPU excessive en cas d'erreurs répétées
                            Thread.Sleep(1000);
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        // Thread interrompu, sortir de la boucle
                        LogMessage("Thread d'écoute interrompu");
                        break;
                    }
                    catch (ThreadAbortException)
                    {
                        // Thread avorté, sortir de la boucle
                        LogMessage("Thread d'écoute annulé");
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        // Peut se produire si AcceptTcpClient est appelé après Stop
                        LogMessage("Listener déjà arrêté");
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Erreur inattendue dans le thread d'écoute: {ex.Message}");
                        if (!isRunning || isClosing)
                        {
                            break;
                        }
                        // Petite pause pour éviter une consommation CPU excessive en cas d'erreurs répétées
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                if (isRunning && !isClosing)
                {
                    LogMessage($"Erreur fatale dans le thread d'écoute: {ex.Message}");
                }
            }
            finally
            {
                LogMessage("Thread d'écoute terminé");
            }
        }

        private void SendWelcomeMessage(ClientInfo clientInfo)
        {
            try
            {
                NetworkStream clientStream = clientInfo.Client.GetStream();

                // Envoyer un message de bienvenue au client
                string welcomeMsg = $"Bienvenue sur le serveur de chat!{Environment.NewLine}" +
                                   $"Votre nom d'utilisateur: {clientInfo.Username}{Environment.NewLine}" +
                                   $"Commandes disponibles:{Environment.NewLine}" +
                                   $"/nick [nouveau_nom] - Change votre nom d'utilisateur{Environment.NewLine}" +
                                   $"/msg [utilisateur] [message] - Envoie un message privé{Environment.NewLine}" +
                                   $"/list - Affiche la liste des utilisateurs connectés{Environment.NewLine}" +
                                   $"/help - Affiche cette aide{Environment.NewLine}";

                // Convertir et envoyer le message
                byte[] buffer = Encoding.UTF8.GetBytes(welcomeMsg);
                clientStream.Write(buffer, 0, buffer.Length);
                clientStream.Flush();

                // Envoyer l'historique des messages récents
                SendMessageHistory(clientInfo);
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de l'envoi du message de bienvenue: {ex.Message}");
            }
        }

        private void SendMessageHistory(ClientInfo clientInfo)
        {
            try
            {
                if (messageHistory.Count > 0)
                {
                    NetworkStream clientStream = clientInfo.Client.GetStream();

                    string historyMsg = $"--- Derniers messages ---{Environment.NewLine}";
                    byte[] headerBuffer = Encoding.UTF8.GetBytes(historyMsg);
                    clientStream.Write(headerBuffer, 0, headerBuffer.Length);
                    clientStream.Flush();

                    // Attendre un peu avant d'envoyer l'historique
                    Thread.Sleep(100);

                    // Envoyer chaque message de l'historique
                    lock (messageHistory)
                    {
                        foreach (string msg in messageHistory)
                        {
                            byte[] msgBuffer = Encoding.UTF8.GetBytes($"{msg}{Environment.NewLine}");
                            clientStream.Write(msgBuffer, 0, msgBuffer.Length);
                            clientStream.Flush();
                            Thread.Sleep(50); // Eviter de submerger le client
                        }
                    }

                    string endHistoryMsg = $"--- Fin de l'historique ---{Environment.NewLine}";
                    byte[] endBuffer = Encoding.UTF8.GetBytes(endHistoryMsg);
                    clientStream.Write(endBuffer, 0, endBuffer.Length);
                    clientStream.Flush();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de l'envoi de l'historique: {ex.Message}");
            }
        }

        private void HandleClientComm(object clientObj)
        {
            ClientInfo clientInfo = (ClientInfo)clientObj;
            TcpClient tcpClient = clientInfo.Client;
            NetworkStream clientStream = null;
            string clientIP = clientInfo.IP;
            string username = clientInfo.Username;

            byte[] message = new byte[4096];
            int bytesRead;

            try
            {
                // Vérifier si le client est encore connecté avant d'obtenir le flux
                if (tcpClient.Connected)
                {
                    clientStream = tcpClient.GetStream();
                }
                else
                {
                    // Le client est déjà déconnecté
                    DisconnectClient(clientInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de l'initialisation du flux client: {ex.Message}");
                DisconnectClient(clientInfo);
                return;
            }

            try
            {
                while (isRunning && !isClosing && tcpClient.Connected)
                {
                    bytesRead = 0;

                    // Vérifier que le client est toujours connecté avant de lire
                    if (!tcpClient.Connected || clientStream == null)
                    {
                        // Client déconnecté
                        break;
                    }

                    // Lire le message du client
                    try
                    {
                        // Vérifier si des données sont disponibles avant de lire
                        if (clientStream.DataAvailable)
                        {
                            bytesRead = clientStream.Read(message, 0, 4096);
                        }
                        else
                        {
                            // Attendre un peu pour ne pas surcharger le CPU
                            Thread.Sleep(50);
                            continue;
                        }

                        // Mise à jour de l'activité du client
                        clientInfo.LastActivity = DateTime.Now;
                        UpdateClientListView(); // Mettre à jour la dernière activité dans la liste
                    }
                    catch (InvalidOperationException)
                    {
                        // Socket non connectée
                        LogMessage($"Socket non connectée pour {username}");
                        break;
                    }
                    catch (IOException)
                    {
                        // Erreur d'I/O - généralement lié à une déconnexion
                        LogMessage($"Erreur d'I/O pour {username}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Autre erreur de lecture
                        LogMessage($"Erreur lors de la lecture du message: {ex.Message}");
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        // Client déconnecté
                        break;
                    }

                    // Convertir les octets du message en chaîne
                    string clientMessage = Encoding.UTF8.GetString(message, 0, bytesRead);

                    // Vérifier si c'est une commande
                    if (clientMessage.StartsWith("/"))
                    {
                        ProcessCommand(clientInfo, clientMessage);
                    }
                    else
                    {
                        // Consigner le message
                        LogMessage($"Message de {username}: {clientMessage}");

                        // Formater le message avec l'horodatage
                        string formattedMessage = $"[{DateTime.Now.ToString("HH:mm:ss")}] {username}: {clientMessage}";

                        // Diffuser le message à tous les clients
                        BroadcastMessage(formattedMessage, tcpClient);

                        // Ajouter à l'historique
                        AddToHistory(formattedMessage);
                    }
                }

                // Client déconnecté, le supprimer de la liste
                DisconnectClient(clientInfo);
            }
            catch (Exception ex)
            {
                if (isRunning && !isClosing)
                {
                    LogMessage($"Erreur lors de la communication avec le client: {ex.Message}");
                }

                // Supprimer le client de la liste
                DisconnectClient(clientInfo);
            }
        }

        private void ProcessCommand(ClientInfo clientInfo, string command)
        {
            try
            {
                string[] parts = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string cmd = parts[0].ToLower();

                switch (cmd)
                {
                    case "/nick":
                        if (parts.Length >= 2)
                        {
                            string newUsername = parts[1];
                            // Vérifier si le nom est déjà utilisé
                            bool nameExists = false;
                            lock (clients)
                            {
                                nameExists = clients.Exists(c => c != clientInfo && c.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase));
                            }

                            if (nameExists)
                            {
                                SendPrivateMessage(clientInfo, "Ce nom d'utilisateur est déjà utilisé.");
                            }
                            else
                            {
                                string oldUsername = clientInfo.Username;
                                clientInfo.Username = newUsername;
                                string changeMsg = $"{oldUsername} est maintenant connu sous le nom de {newUsername}";
                                BroadcastMessage(changeMsg, null);
                                AddToHistory(changeMsg);

                                // Mettre à jour la liste des utilisateurs dans l'interface
                                UpdateClientListView();
                            }
                        }
                        else
                        {
                            SendPrivateMessage(clientInfo, "Utilisation: /nick [nouveau_nom]");
                        }
                        break;

                    case "/msg":
                        if (parts.Length >= 3)
                        {
                            string targetUser = parts[1];
                            string msgContent = string.Join(" ", parts, 2, parts.Length - 2);

                            // Trouver le client cible
                            ClientInfo targetClient = null;
                            lock (clients)
                            {
                                targetClient = clients.Find(c => c.Username.Equals(targetUser, StringComparison.OrdinalIgnoreCase));
                            }

                            if (targetClient != null)
                            {
                                // Envoyer le message privé
                                string privateMsg = $"[Privé de {clientInfo.Username}]: {msgContent}";
                                SendPrivateMessage(targetClient, privateMsg);

                                // Confirmer à l'expéditeur
                                SendPrivateMessage(clientInfo, $"[Privé à {targetUser}]: {msgContent}");

                                // Log du message privé
                                LogMessage($"Message privé de {clientInfo.Username} à {targetUser}: {msgContent}");
                            }
                            else
                            {
                                SendPrivateMessage(clientInfo, $"Utilisateur '{targetUser}' non trouvé.");
                            }
                        }
                        else
                        {
                            SendPrivateMessage(clientInfo, "Utilisation: /msg [utilisateur] [message]");
                        }
                        break;

                    case "/list":
                        StringBuilder userList = new StringBuilder("Utilisateurs connectés:");
                        lock (clients)
                        {
                            foreach (ClientInfo client in clients)
                            {
                                userList.Append($"{Environment.NewLine}- {client.Username} ({client.IP})");
                            }
                        }
                        SendPrivateMessage(clientInfo, userList.ToString());
                        break;

                    case "/help":
                        string helpMsg = $"Commandes disponibles:{Environment.NewLine}" +
                                         $"/nick [nouveau_nom] - Change votre nom d'utilisateur{Environment.NewLine}" +
                                         $"/msg [utilisateur] [message] - Envoie un message privé{Environment.NewLine}" +
                                         $"/list - Affiche la liste des utilisateurs connectés{Environment.NewLine}" +
                                         $"/help - Affiche cette aide";
                        SendPrivateMessage(clientInfo, helpMsg);
                        break;

                    default:
                        SendPrivateMessage(clientInfo, $"Commande inconnue: {cmd}. Tapez /help pour obtenir de l'aide.");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors du traitement de la commande: {ex.Message}");
                SendPrivateMessage(clientInfo, "Erreur lors du traitement de la commande.");
            }
        }

        private void SendPrivateMessage(ClientInfo clientInfo, string message)
        {
            try
            {
                // Vérifier si le client est connecté
                if (clientInfo.Client.Connected)
                {
                    NetworkStream clientStream = clientInfo.Client.GetStream();
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush();
                }
                else
                {
                    LogMessage($"Impossible d'envoyer un message privé à {clientInfo.Username}: client non connecté");
                }
            }
            catch (InvalidOperationException)
            {
                LogMessage($"Socket non connectée lors de l'envoi d'un message privé à {clientInfo.Username}");
            }
            catch (IOException)
            {
                LogMessage($"Erreur d'I/O lors de l'envoi d'un message privé à {clientInfo.Username}");
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de l'envoi du message privé à {clientInfo.Username}: {ex.Message}");
            }
        }

        private void BroadcastMessage(string message, TcpClient excludeClient, bool isSystemMessage = false)
        {
            // Ne rien faire si l'application est en train de se fermer
            if (isClosing)
                return;

            // Ajouter l'horodatage pour les messages système
            if (isSystemMessage)
            {
                message = $"[SYSTÈME] {message}";
            }

            // Convertir le message en octets
            byte[] broadcastBytes = Encoding.UTF8.GetBytes(message);

            // Envoyer à tous les clients sauf l'expéditeur
            lock (clients)
            {
                foreach (ClientInfo client in clients)
                {
                    if (client.Client != excludeClient && client.Client.Connected)
                    {
                        try
                        {
                            NetworkStream clientStream = client.Client.GetStream();
                            clientStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                            clientStream.Flush();
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Erreur lors de l'envoi du message à {client.Username}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void AddToHistory(string message)
        {
            lock (messageHistory)
            {
                messageHistory.Add(message);

                // Limiter la taille de l'historique
                if (messageHistory.Count > maxMessageHistory)
                {
                    messageHistory.RemoveAt(0);
                }
            }
        }

        private void DisconnectClient(ClientInfo clientInfo)
        {
            // Ne rien faire si l'application est en train de se fermer
            if (isClosing)
                return;

            lock (clients)
            {
                if (clients.Contains(clientInfo))
                {
                    clients.Remove(clientInfo);

                    try
                    {
                        clientInfo.Client.Close();
                    }
                    catch { }

                    string disconnectMsg = $"{clientInfo.Username} a quitté le chat";
                    LogMessage(disconnectMsg);
                    BroadcastMessage(disconnectMsg, null);
                    AddToHistory(disconnectMsg);

                    // Mettre à jour le compteur de clients et la liste
                    UpdateClientCount();
                    UpdateClientListView();
                }
            }
        }

        private void CheckInactiveClients(CancellationToken token)
        {
            try
            {
                while (isRunning && !isClosing && !token.IsCancellationRequested)
                {
                    // Vérifier les clients inactifs toutes les 30 secondes
                    Thread.Sleep(30000);

                    List<ClientInfo> inactiveClients = new List<ClientInfo>();

                    // Trouver les clients inactifs (plus de 5 minutes sans activité)
                    lock (clients)
                    {
                        DateTime now = DateTime.Now;
                        foreach (ClientInfo client in clients)
                        {
                            TimeSpan inactiveTime = now - client.LastActivity;
                            if (inactiveTime.TotalMinutes > 5)
                            {
                                inactiveClients.Add(client);
                            }
                        }
                    }

                    // Déconnecter les clients inactifs
                    foreach (ClientInfo client in inactiveClients)
                    {
                        try
                        {
                            // Envoyer un message d'inactivité avant la déconnexion
                            SendPrivateMessage(client, "Vous avez été déconnecté pour inactivité.");
                            Thread.Sleep(500); // Donner le temps au client de recevoir le message

                            LogMessage($"Client inactif déconnecté: {client.Username}");
                            DisconnectClient(client);
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Erreur lors de la déconnexion du client inactif: {ex.Message}");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Opération annulée
            }
            catch (Exception ex)
            {
                if (!isClosing)
                {
                    LogMessage($"Erreur dans la vérification des clients inactifs: {ex.Message}");
                }
            }
        }

        private void UpdateClientCount()
        {
            if (isClosing)
                return;

            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(UpdateClientCount));
                }
                else
                {
                    if (!isClosing && this.lblConnectedClients != null && !this.lblConnectedClients.IsDisposed)
                    {
                        lblConnectedClients.Text = $"Clients: {clients.Count}";
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Les contrôles ont été supprimés, ignorer
            }
            catch (InvalidOperationException)
            {
                // L'application est probablement en train de se fermer, ignorer
            }
            catch (Exception)
            {
                // Ignorer les autres exceptions lors de la fermeture
            }
        }

        private void UpdateClientListView()
        {
            if (isClosing)
                return;

            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(UpdateClientListView));
                }
                else
                {
                    if (!isClosing && lvUsers != null && !lvUsers.IsDisposed)
                    {
                        lvUsers.Items.Clear();

                        lock (clients)
                        {
                            foreach (ClientInfo client in clients)
                            {
                                ListViewItem item = new ListViewItem(client.IP);
                                item.SubItems.Add(client.Username);
                                item.SubItems.Add(client.LastActivity.ToString("yyyy-MM-dd HH:mm:ss"));
                                lvUsers.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Les contrôles ont été supprimés, ignorer
            }
            catch (InvalidOperationException)
            {
                // L'application est probablement en train de se fermer, ignorer
            }
            catch (Exception)
            {
                // Ignorer les autres exceptions lors de la fermeture
            }
        }

        private void LogMessage(string message)
        {
            // Si l'application est en train de se fermer, ne rien faire
            if (isClosing)
                return;

            try
            {
                // Consigner le message de manière sécurisée pour le thread de l'interface utilisateur
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action<string>(LogMessage), new object[] { message });
                }
                else
                {
                    if (!isClosing && txtLog != null && !txtLog.IsDisposed)
                    {
                        txtLog.AppendText($"[{DateTime.Now}] {message}{Environment.NewLine}");
                        txtLog.ScrollToCaret();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Les contrôles ont été supprimés, ignorer
            }
            catch (InvalidOperationException)
            {
                // L'application est probablement en train de se fermer, ignorer
            }
            catch (Exception)
            {
                // Ignorer les autres exceptions lors de la fermeture
            }
        }

        private void btnSendServerMessage_Click(object sender, EventArgs e)
        {
            if (isRunning && !isClosing && !string.IsNullOrEmpty(txtServerMessage.Text))
            {
                string serverMessage = txtServerMessage.Text;
                BroadcastMessage($"[SERVEUR] {serverMessage}", null, true);
                LogMessage($"Message serveur: {serverMessage}");
                AddToHistory($"[SERVEUR] {serverMessage}");
                txtServerMessage.Clear();
            }
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Si l'application est déjà en train de se fermer, ne rien faire
            if (isClosing)
                return;

            try
            {
                // Afficher un message de confirmation
                if (isRunning)
                {
                    DialogResult result = MessageBox.Show(
                        "Le serveur est en cours d'exécution. Voulez-vous vraiment fermer l'application ?",
                        "Confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                // Indiquer que l'application est en train de se fermer
                isClosing = true;

                // Se déconnecter lorsque le formulaire se ferme
                if (isRunning)
                {
                    try
                    {
                        // Arrêter le serveur
                        StopServer();
                    }
                    catch (Exception ex)
                    {
                        // Ignorer les erreurs lors de l'arrêt du serveur pendant la fermeture
                        Console.WriteLine($"Erreur lors de l'arrêt du serveur pendant la fermeture: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur pendant la fermeture: {ex.Message}");
            }
        }
    }
}