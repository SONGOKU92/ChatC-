using System;
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
            this.txtChatLog = new System.Windows.Forms.TextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
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
            this.txtChatLog.Multiline = true;
            this.txtChatLog.Name = "txtChatLog";
            this.txtChatLog.ReadOnly = true;
            this.txtChatLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtChatLog.Size = new System.Drawing.Size(460, 267);
            this.txtChatLog.TabIndex = 5;
            // 
            // txtMessage
            // 
            this.txtMessage.Enabled = false;
            this.txtMessage.Location = new System.Drawing.Point(12, 327);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(375, 22);
            this.txtMessage.TabIndex = 6;
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);
            // 
            // btnSend
            // 
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(393, 326);
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
            this.lblStatus.Location = new System.Drawing.Point(12, 358);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(99, 16);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Non connecté";
            // 
            // ClientForm
            // 
            this.ClientSize = new System.Drawing.Size(484, 383);
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
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.TextBox txtServerPort;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtChatLog;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblStatus;

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
                lblStatus.Text = $"Connecté à {serverIP}:{serverPort}";

                // Démarrer le thread de réception
                receiveThread = new Thread(new ThreadStart(ReceiveMessages));
                receiveThread.IsBackground = true;
                receiveThread.Start();

                LogMessage("Connecté au serveur");
                txtMessage.Focus();
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur de connexion: {ex.Message}");
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
                    receiveThread.Abort();
                }

                // Mettre à jour l'interface utilisateur
                btnConnect.Text = "Se connecter";
                txtServerIP.Enabled = true;
                txtServerPort.Enabled = true;
                txtMessage.Enabled = false;
                btnSend.Enabled = false;
                lblStatus.Text = "Non connecté";

                LogMessage("Déconnecté du serveur");
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de la déconnexion: {ex.Message}");
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
                    catch
                    {
                        // Client déconnecté
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        // Déconnecté
                        break;
                    }

                    // Convertir le message en chaîne de caractères
                    string serverMessage = Encoding.UTF8.GetString(message, 0, bytesRead);
                    LogMessage(serverMessage);
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
            catch (ThreadAbortException)
            {
                // Le thread a été arrêté, ne rien faire
            }
            catch (Exception ex)
            {
                if (isConnected)
                {
                    LogMessage($"Erreur lors de la réception: {ex.Message}");
                    this.Invoke(new Action(() =>
                    {
                        DisconnectFromServer();
                    }));
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
                    // Convertir le message en octets
                    byte[] messageBytes = Encoding.UTF8.GetBytes(txtMessage.Text);

                    // Envoyer au serveur
                    clientStream.Write(messageBytes, 0, messageBytes.Length);
                    clientStream.Flush();

                    // Effacer la zone de texte
                    txtMessage.Clear();
                }
                catch (Exception ex)
                {
                    LogMessage($"Erreur lors de l'envoi: {ex.Message}");
                    DisconnectFromServer();
                }
            }
        }

        private void LogMessage(string message)
        {
            // Consigner le message de manière thread-safe pour l'interface utilisateur
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(LogMessage), new object[] { message });
            }
            else
            {
                txtChatLog.AppendText($"{message}{Environment.NewLine}");
                txtChatLog.ScrollToCaret();
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
    }
}