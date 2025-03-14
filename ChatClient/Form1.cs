using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatClientGUI
{
    public partial class ClientForm : Form
    {
        private TcpClient client;
        private NetworkStream clientStream;
        private Thread receiveThread;
        private bool isConnected = false;
        private string username = "";
        private Dictionary<string, Color> userColors = new Dictionary<string, Color>();
        private Random random = new Random();

        public ClientForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.txtServerIP = new System.Windows.Forms.TextBox();
            this.txtServerPort = new System.Windows.Forms.TextBox();
            this.lblIP = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtChatLog = new System.Windows.Forms.RichTextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.lblUsers = new System.Windows.Forms.Label();
            this.pnlCommands = new System.Windows.Forms.Panel();
            this.cmbCommands = new System.Windows.Forms.ComboBox();
            this.lblCommands = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtServerIP
            // 
            this.txtServerIP.Location = new System.Drawing.Point(50, 12);
            this.txtServerIP.Name = "txtServerIP";
            this.txtServerIP.Size = new System.Drawing.Size(120, 22);
            this.txtServerIP.TabIndex = 0;
            this.txtServerIP.Text = "127.0.0.1";
            // 
            // txtServerPort
            // 
            this.txtServerPort.Location = new System.Drawing.Point(221, 12);
            this.txtServerPort.Name = "txtServerPort";
            this.txtServerPort.Size = new System.Drawing.Size(60, 22);
            this.txtServerPort.TabIndex = 1;
            this.txtServerPort.Text = "8888";
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(12, 15);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(24, 16);
            this.lblIP.TabIndex = 2;
            this.lblIP.Text = "IP:";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(176, 15);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(37, 16);
            this.lblPort.TabIndex = 3;
            this.lblPort.Text = "Port:";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(296, 12);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(176, 23);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Se connecter";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtChatLog
            // 
            this.txtChatLog.Location = new System.Drawing.Point(12, 47);
            this.txtChatLog.Name = "txtChatLog";
            this.txtChatLog.ReadOnly = true;
            this.txtChatLog.Size = new System.Drawing.Size(460, 267);
            this.txtChatLog.TabIndex = 5;
            this.txtChatLog.Text = "";
            this.txtChatLog.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.txtChatLog_LinkClicked);
            // 
            // txtMessage
            // 
            this.txtMessage.Enabled = false;
            this.txtMessage.Location = new System.Drawing.Point(12, 357);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(375, 22);
            this.txtMessage.TabIndex = 6;
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);
            // 
            // btnSend
            // 
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(393, 356);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(79, 23);
            this.btnSend.TabIndex = 7;
            this.btnSend.Text = "Envoyer";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 388);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(99, 16);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Non connecté";
            // 
            // lstUsers
            // 
            this.lstUsers.FormattingEnabled = true;
            this.lstUsers.ItemHeight = 16;
            this.lstUsers.Location = new System.Drawing.Point(478, 67);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(150, 244);
            this.lstUsers.TabIndex = 9;
            this.lstUsers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstUsers_MouseDoubleClick);
            // 
            // lblUsers
            // 
            this.lblUsers.AutoSize = true;
            this.lblUsers.Location = new System.Drawing.Point(478, 47);
            this.lblUsers.Name = "lblUsers";
            this.lblUsers.Size = new System.Drawing.Size(150, 16);
            this.lblUsers.TabIndex = 10;
            this.lblUsers.Text = "Utilisateurs connectés";
            // 
            // pnlCommands
            // 
            this.pnlCommands.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCommands.Location = new System.Drawing.Point(12, 323);
            this.pnlCommands.Name = "pnlCommands";
            this.pnlCommands.Size = new System.Drawing.Size(616, 25);
            this.pnlCommands.TabIndex = 11;
            // 
            // cmbCommands
            // 
            this.cmbCommands.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCommands.Enabled = false;
            this.cmbCommands.FormattingEnabled = true;
            this.cmbCommands.Items.AddRange(new object[] {
            "Message normal",
            "/nick - Changer de nom",
            "/msg - Message privé",
            "/list - Liste des utilisateurs",
            "/help - Aide"});
            this.cmbCommands.Location = new System.Drawing.Point(478, 357);
            this.cmbCommands.Name = "cmbCommands";
            this.cmbCommands.Size = new System.Drawing.Size(150, 24);
            this.cmbCommands.TabIndex = 12;
            this.cmbCommands.SelectedIndexChanged += new System.EventHandler(this.cmbCommands_SelectedIndexChanged);
            // 
            // lblCommands
            // 
            this.lblCommands.AutoSize = true;
            this.lblCommands.Location = new System.Drawing.Point(478, 335);
            this.lblCommands.Name = "lblCommands";
            this.lblCommands.Size = new System.Drawing.Size(87, 16);
            this.lblCommands.TabIndex = 13;
            this.lblCommands.Text = "Commandes";
            // 
            // ClientForm
            // 
            this.ClientSize = new System.Drawing.Size(640, 413);
            this.Controls.Add(this.lblCommands);
            this.Controls.Add(this.cmbCommands);
            this.Controls.Add(this.pnlCommands);
            this.Controls.Add(this.lblUsers);
            this.Controls.Add(this.lstUsers);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.txtChatLog);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.txtServerPort);
            this.Controls.Add(this.txtServerIP);
            this.Name = "ClientForm";
            this.Text = "Client de Chat";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientForm_FormClosing);
            this.Load += new System.EventHandler(this.ClientForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.TextBox txtServerPort;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.RichTextBox txtChatLog;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Label lblUsers;
        private System.Windows.Forms.Panel pnlCommands;
        private System.Windows.Forms.ComboBox cmbCommands;
        private System.Windows.Forms.Label lblCommands;

        private void ClientForm_Load(object sender, EventArgs e)
        {
            cmbCommands.SelectedIndex = 0; // Sélectionner "Message normal" par défaut
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                ConnectToServer();
            }
            else
            {
                DisconnectFromServer();
            }
        }

        private void ConnectToServer()
        {
            try
            {
                string serverIP = txtServerIP.Text;
                int serverPort;

                // Valider le port
                if (!int.TryParse(txtServerPort.Text, out serverPort))
                {
                    serverPort = 8888;
                    txtServerPort.Text = serverPort.ToString();
                }

                // Créer le client et se connecter
                client = new TcpClient();
                client.Connect(serverIP, serverPort);
                clientStream = client.GetStream();
                isConnected = true;

                // Mettre à jour l'interface utilisateur
                btnConnect.Text = "Déconnecter";
                txtServerIP.Enabled = false;
                txtServerPort.Enabled = false;
                txtMessage.Enabled = true;
                btnSend.Enabled = true;
                cmbCommands.Enabled = true;
                lblStatus.Text = $"Connecté à {serverIP}:{serverPort}";

                // Démarrer le thread de réception
                receiveThread = new Thread(new ThreadStart(ReceiveMessages));
                receiveThread.IsBackground = true;
                receiveThread.Start();

                AppendMessageToChat("Connecté au serveur", Color.Green);
                txtMessage.Focus();
            }
            catch (Exception ex)
            {
                AppendMessageToChat($"Erreur de connexion: {ex.Message}", Color.Red);
            }
        }

        private void DisconnectFromServer()
        {
            try
            {
                // Fermer le client
                isConnected = false;
                if (clientStream != null)
                {
                    clientStream.Close();
                }
                if (client != null)
                {
                    client.Close();
                }

                // Arrêter le thread de réception
                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Interrupt();
                }

                // Mettre à jour l'interface utilisateur
                btnConnect.Text = "Se connecter";
                txtServerIP.Enabled = true;
                txtServerPort.Enabled = true;
                txtMessage.Enabled = false;
                btnSend.Enabled = false;
                cmbCommands.Enabled = false;
                lblStatus.Text = "Non connecté";
                lstUsers.Items.Clear();

                AppendMessageToChat("Déconnecté du serveur", Color.Red);
            }
            catch (Exception ex)
            {
                AppendMessageToChat($"Erreur lors de la déconnexion: {ex.Message}", Color.Red);
            }
        }

        private void ReceiveMessages()
        {
            byte[] message = new byte[4096];
            int bytesRead;

            try
            {
                while (isConnected && client.Connected)
                {
                    bytesRead = 0;

                    // Lire le message du serveur
                    try
                    {
                        bytesRead = clientStream.Read(message, 0, 4096);
                    }
                    catch (Exception)
                    {
                        // Client déconnecté ou interruption
                        if (isConnected)
                        {
                            break;
                        }
                        return;
                    }

                    if (bytesRead == 0)
                    {
                        // Déconnecté
                        break;
                    }

                    // Convertir le message en chaîne de caractères
                    string serverMessage = Encoding.UTF8.GetString(message, 0, bytesRead);
                    ProcessServerMessage(serverMessage);
                }

                // Déconnecté du serveur
                if (isConnected)
                {
                    // Le serveur nous a déconnectés
                    this.Invoke(new Action(() =>
                    {
                        DisconnectFromServer();
                    }));
                }
            }
            catch (ThreadInterruptedException)
            {
                // Le thread a été interrompu, ne rien faire
            }
            catch (Exception ex)
            {
                if (isConnected)
                {
                    this.Invoke(new Action(() =>
                    {
                        AppendMessageToChat($"Erreur lors de la réception: {ex.Message}", Color.Red);
                        DisconnectFromServer();
                    }));
                }
            }
        }

        private void ProcessServerMessage(string message)
        {
            this.Invoke(new Action(() =>
            {
                // Extraction du nom d'utilisateur du message si présent
                if (message.StartsWith("Bienvenue sur le serveur de chat"))
                {
                    // Message de bienvenue, extraire le nom d'utilisateur
                    int usernameIndex = message.IndexOf("Votre nom d'utilisateur:");
                    if (usernameIndex > 0)
                    {
                        int startOfUsername = usernameIndex + "Votre nom d'utilisateur:".Length;
                        int endOfLine = message.IndexOf(Environment.NewLine, startOfUsername);
                        if (endOfLine > startOfUsername)
                        {
                            username = message.Substring(startOfUsername, endOfLine - startOfUsername).Trim();
                            this.Text = $"Client de Chat - {username}";
                        }
                    }
                    AppendMessageToChat(message, Color.Blue);
                }
                else if (message.StartsWith("[SYSTÈME]") || message.StartsWith("[SERVEUR]"))
                {
                    // Message système ou serveur
                    AppendMessageToChat(message, Color.Purple);
                }
                else if (message.StartsWith("[Privé"))
                {
                    // Message privé
                    AppendMessageToChat(message, Color.RoyalBlue);
                }
                else if (message.Contains(" a rejoint le chat") || message.Contains(" a quitté le chat"))
                {
                    // Notification de connexion/déconnexion
                    AppendMessageToChat(message, Color.Gray);

                    // Extraire le nom d'utilisateur et mettre à jour la liste
                    string user = message.Split(' ')[0];
                    UpdateUsersList(message);
                }
                else if (message.Contains(" est maintenant connu sous le nom de "))
                {
                    // Notification de changement de nom
                    AppendMessageToChat(message, Color.Gray);

                    // Mettre à jour le nom local si nécessaire
                    string[] parts = message.Split(new string[] { " est maintenant connu sous le nom de " }, StringSplitOptions.None);
                    if (parts.Length == 2 && parts[0].Trim().Equals(username))
                    {
                        username = parts[1].Trim();
                        this.Text = $"Client de Chat - {username}";
                    }

                    // Mettre à jour la liste des utilisateurs
                    UpdateUsersList(message);
                }
                else if (message.StartsWith("Utilisateurs connectés:"))
                {
                    // Liste des utilisateurs
                    AppendMessageToChat(message, Color.DarkGreen);

                    // Mettre à jour la liste des utilisateurs
                    string[] lines = message.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    lstUsers.Items.Clear();
                    for (int i = 1; i < lines.Length; i++) // Ignorer la première ligne
                    {
                        if (lines[i].StartsWith("- "))
                        {
                            string user = lines[i].Substring(2).Split(' ')[0]; // Enlever "- " et ne prendre que le nom
                            if (!lstUsers.Items.Contains(user))
                            {
                                lstUsers.Items.Add(user);
                            }
                        }
                    }
                }
                else
                {
                    // Message normal
                    string messageText = message;
                    Color textColor = Color.Black;

                    // Extraire le nom d'utilisateur pour la coloration
                    int colonIndex = message.IndexOf(":");
                    if (colonIndex > 0)
                    {
                        string user = message.Substring(0, colonIndex).Trim();

                        // Si c'est un message horodaté, extraire le vrai nom d'utilisateur
                        if (user.Contains("]"))
                        {
                            user = user.Substring(user.LastIndexOf("]") + 1).Trim();
                        }

                        // Obtenir ou générer une couleur pour cet utilisateur
                        if (!userColors.ContainsKey(user))
                        {
                            // Générer une couleur aléatoire moyennement foncée (pour une bonne lisibilité)
                            Color randomColor = Color.FromArgb(
                                random.Next(100, 200),
                                random.Next(100, 200),
                                random.Next(100, 200));
                            userColors[user] = randomColor;
                        }
                        textColor = userColors[user];

                        // Ajouter à la liste des utilisateurs si nécessaire
                        if (!lstUsers.Items.Contains(user) && !string.IsNullOrEmpty(user))
                        {
                            lstUsers.Items.Add(user);
                        }
                    }

                    AppendMessageToChat(messageText, textColor);
                }
            }));
        }

        private void UpdateUsersList(string message)
        {
            if (message.Contains(" a rejoint le chat"))
            {
                string user = message.Split(' ')[0];
                if (!lstUsers.Items.Contains(user))
                {
                    lstUsers.Items.Add(user);
                }
            }
            else if (message.Contains(" a quitté le chat"))
            {
                string user = message.Split(' ')[0];
                lstUsers.Items.Remove(user);
            }
            else if (message.Contains(" est maintenant connu sous le nom de "))
            {
                string[] parts = message.Split(new string[] { " est maintenant connu sous le nom de " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string oldName = parts[0].Trim();
                    string newName = parts[1].Trim();

                    int index = lstUsers.Items.IndexOf(oldName);
                    if (index >= 0)
                    {
                        lstUsers.Items.RemoveAt(index);
                        lstUsers.Items.Add(newName);
                    }

                    // Mettre à jour la couleur de l'utilisateur
                    if (userColors.ContainsKey(oldName))
                    {
                        Color color = userColors[oldName];
                        userColors.Remove(oldName);
                        userColors[newName] = color;
                    }
                }
            }
        }

        private void AppendMessageToChat(string message, Color color)
        {
            // Consigner le message de manière thread-safe pour l'interface utilisateur
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, Color>(AppendMessageToChat), new object[] { message, color });
            }
            else
            {
                int start = txtChatLog.TextLength;
                txtChatLog.AppendText($"{message}{Environment.NewLine}");
                int end = txtChatLog.TextLength;

                // Appliquer la couleur
                txtChatLog.Select(start, end - start);
                txtChatLog.SelectionColor = color;

                // Détecter et formater les commandes
                if (message.Contains("/"))
                {
                    FormatCommandsAsLinks(start, end);
                }

                txtChatLog.SelectionLength = 0; // Désélectionner le texte
                txtChatLog.ScrollToCaret();
            }
        }

        private void FormatCommandsAsLinks(int start, int end)
        {
            string text = txtChatLog.Text.Substring(start, end - start);
            string[] commands = { "/nick", "/msg", "/list", "/help" };

            foreach (string cmd in commands)
            {
                int cmdIndex = text.IndexOf(cmd);
                while (cmdIndex >= 0)
                {
                    // Seulement si c'est une commande distincte (début de ligne ou après un espace)
                    bool isValidCommand = cmdIndex == 0 || char.IsWhiteSpace(text[cmdIndex - 1]) || text[cmdIndex - 1] == '\n';

                    if (isValidCommand)
                    {
                        txtChatLog.Select(start + cmdIndex, cmd.Length);
                        txtChatLog.SelectionColor = Color.Blue;
                        txtChatLog.SelectionFont = new Font(txtChatLog.Font, FontStyle.Underline);
                    }

                    cmdIndex = text.IndexOf(cmd, cmdIndex + cmd.Length);
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage();
                e.SuppressKeyPress = true;
            }
        }

        private void SendMessage()
        {
            if (isConnected && !string.IsNullOrEmpty(txtMessage.Text))
            {
                try
                {
                    string messageText = txtMessage.Text.Trim();

                    // Vérifier si c'est une commande
                    if (messageText.StartsWith("/"))
                    {
                        // Ne pas afficher les commandes localement sauf si c'est un message privé
                        if (messageText.StartsWith("/msg "))
                        {
                            // Extraire le destinataire et le contenu du message privé
                            string[] parts = messageText.Split(new[] { ' ' }, 3);
                            if (parts.Length >= 3)
                            {
                                string recipient = parts[1];
                                string content = parts[2];

                                // Afficher localement le message privé dans le format attendu
                                string localPrivateMsg = $"[Privé à {recipient}]: {content}";
                                AppendMessageToChat(localPrivateMsg, Color.RoyalBlue);
                            }
                        }
                    }
                    else
                    {
                        // Pour les messages normaux, afficher une version locale
                        string timeStamp = DateTime.Now.ToString("HH:mm:ss");
                        string localMessage = $"[{timeStamp}] {username}: {messageText}";

                        // Obtenir ou générer la couleur pour l'utilisateur local
                        Color userColor = Color.Black;
                        if (userColors.ContainsKey(username))
                        {
                            userColor = userColors[username];
                        }
                        else
                        {
                            // Générer une couleur cohérente pour cet utilisateur
                            userColor = Color.FromArgb(
                                random.Next(100, 200),
                                random.Next(100, 200),
                                random.Next(100, 200));
                            userColors[username] = userColor;
                        }

                        // Afficher le message localement
                        AppendMessageToChat(localMessage, userColor);
                    }

                    // Convertir le message en octets
                    byte[] messageBytes = Encoding.UTF8.GetBytes(messageText);

                    // Envoyer au serveur
                    clientStream.Write(messageBytes, 0, messageBytes.Length);
                    clientStream.Flush();

                    // Effacer la zone de texte
                    txtMessage.Clear();
                }
                catch (Exception ex)
                {
                    AppendMessageToChat($"Erreur lors de l'envoi: {ex.Message}", Color.Red);
                    DisconnectFromServer();
                }
            }
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Se déconnecter lorsque le formulaire se ferme
            if (isConnected)
            {
                DisconnectFromServer();
            }
        }

        private void cmbCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCommands.SelectedIndex > 0) // Pas "Message normal"
            {
                string command = "";
                switch (cmbCommands.SelectedIndex)
                {
                    case 1: // /nick
                        command = "/nick ";
                        break;
                    case 2: // /msg
                        command = "/msg ";
                        if (lstUsers.SelectedItem != null)
                        {
                            command += lstUsers.SelectedItem.ToString() + " ";
                        }
                        break;
                    case 3: // /list
                        command = "/list";
                        break;
                    case 4: // /help
                        command = "/help";
                        break;
                }

                txtMessage.Text = command;
                txtMessage.Focus();
                txtMessage.SelectionStart = txtMessage.Text.Length;

                // Revenir à "Message normal" après avoir inséré la commande
                cmbCommands.SelectedIndex = 0;
            }
        }

        private void lstUsers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lstUsers.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                string selectedUser = lstUsers.Items[index].ToString();
                txtMessage.Text = $"/msg {selectedUser} ";
                txtMessage.Focus();
                txtMessage.SelectionStart = txtMessage.Text.Length;
            }
        }

        private void txtChatLog_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            // Gérer les liens cliqués dans le chat
            if (e.LinkText.StartsWith("http"))
            {
                try
                {
                    System.Diagnostics.Process.Start(e.LinkText);
                }
                catch (Exception ex)
                {
                    AppendMessageToChat($"Erreur lors de l'ouverture du lien: {ex.Message}", Color.Red);
                }
            }
        }
    }
}