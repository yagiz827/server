using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace server
{
    struct obj
    {
        public string name;
        public Socket client;
        public int score;
    }

    public partial class Form1 : Form
    {
        Game game;
        bool clicked=false;

        List<obj> clientSockets = new List<obj>();

        double puan1 = 0;
        double puan2 = 0;
        int question_number = 1;
        List<string> name_list = new List<string>();

        bool terminating = false;
        bool listening = false;


        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();

            quetext.Text = "";
            textBox_port.Text = "";
        }
        private void button_listen_Click(object sender, EventArgs e)
        {
            int serverPort;
            int numberOfQuestions;
            var isNumberOfQuestionsEntered = Int32.TryParse(quetext.Text, out numberOfQuestions);
            var isPortEntered = Int32.TryParse(textBox_port.Text, out serverPort);
            RemoveLogs();

            if (isNumberOfQuestionsEntered && isPortEntered)
            {
                var fileName = "questions.txt";
                game = new Game(logs);
                if (game.StartServer(fileName, numberOfQuestions, serverPort))
                {
                    button1.Enabled = true;
                    logs.AppendText("Started listening on port: " + serverPort + "\n");
                }
                else
                {
                    PrintCantStartListening(isNumberOfQuestionsEntered, isPortEntered);

                }

                listening = true;
                button_listen.Enabled = false;
                textBox_message.Enabled = true;
                button_send.Enabled = true;


                //Thread acceptt = new Thread(Accept);
                //acceptt.Start();

            }
            else
            {
                PrintCantStartListening(isNumberOfQuestionsEntered, isPortEntered);
            }
        }

        private void PrintCantStartListening(bool isNumberOfQuestionsEntered, bool isPortEntered)
        {
            logs.AppendText("Couldn't start listening\n");
            if (!isNumberOfQuestionsEntered)
            {
                logs.AppendText("Number of questions box is empty!\n");
            }
            if (!isPortEntered)
            {
                logs.AppendText("Port number box is empty!\n");
            }
        }

        private void RemoveLogs()
        {
            logs.ResetText();
        }

        //private void Accept()
        //{
        //    int ids = 1;
        //    while (listening)
        //    {
        //        try
        //        {
        //            obj h = new obj();

        //            Socket newClient = serverSocket.Accept();
        //            h.client = newClient;

        //            ids += 1;
        //            h.score = 0;

        //            Byte[] buffer = new Byte[64];
        //            newClient.Receive(buffer);

        //            string incomingMessage = Encoding.Default.GetString(buffer);
        //            incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));

        //            h.name = incomingMessage;
        //            name_list.Add(incomingMessage);

        //            bool namein = true;
        //            foreach (var t in clientSockets)
        //            {
        //                if (t.name == incomingMessage)
        //                {
        //                    namein = false;
        //                }
        //            }

        //            if (namein)
        //            {
        //                if (clientSockets.Count <= 1)
        //                {
        //                    logs.AppendText("\n" + h.name + " is connected.\n");

        //                    clientSockets.Add(h);
        //                    string message1 = "you are connected to the server.\n";
        //                    if (message1 != "") // Baya garip bir if kullanımı
        //                    {
        //                        Byte[] buffer3 = Encoding.Default.GetBytes(message1);
        //                        newClient.Send(buffer3);
        //                    }

        //                    if (clientSockets.Count() >= 2)
        //                    {
        //                        int numq;
        //                        Int32.TryParse(quetext.Text, out numq);

        //                        if (numq >= 1)
        //                        {
        //                            string gamest = "Game has been started";
        //                            Byte[] buffer4 = Encoding.Default.GetBytes(gamest);


        //                            foreach (var t in clientSockets)
        //                            {
        //                                t.client.Send(buffer4);
        //                            }


        //                            The_Game(clientSockets[0].client, clientSockets[1].client, clientSockets);

        //                        }

        //                    }
        //                    //Thread receiveThread = new Thread(() => Receive(newClient)); // updated
        //                    //receiveThread.Start();
        //                }

        //            }

        //        }
        //        catch
        //        {
        //            if (terminating)
        //            {
        //                listening = false;
        //            }
        //            else
        //            {
        //                logs.AppendText("The socket stopped working.\n");
        //            }

        //        }
        //    }
        //}

        //private void Receive(Socket thisClient) // updated
        //{
        //    bool connected = true;

        //    while (connected && !terminating)
        //    {
        //        try
        //        {
        //            Byte[] buffer = new Byte[64];
        //            thisClient.Receive(buffer);

        //            string incomingMessage = Encoding.Default.GetString(buffer);
        //            incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
        //            foreach (var y in clientSockets)
        //            {
        //                if (y.client == thisClient)
        //                {
        //                    logs.AppendText(y.name + ": " + incomingMessage + "\n");
        //                }
        //            }

        //        }
        //        catch
        //        {
        //            if (!terminating)
        //            {
        //                logs.AppendText("A client has disconnected\n");
        //            }

        //            //logs.AppendText("you are the winner congratulations. Your oppenent has been disconnected\n");
        //            thisClient.Close();
        //            foreach (var t in clientSockets)
        //            {
        //                if (t.client == thisClient)
        //                {
        //                    string name = t.name;
        //                    Socket socket = thisClient;
        //                    int score = t.score;
        //                    obj w = new obj { name = name, client = thisClient, score = score };
        //                    clientSockets.Remove(w);
        //                    break;
        //                }
        //            }

        //            string message = "you are the winner congratulations.\n";
        //            if (message != "")
        //            {
        //                Byte[] buffer = Encoding.Default.GetBytes(message);
        //                foreach (var client in clientSockets)
        //                {
        //                    client.client.Send(buffer);
        //                }

        //            }
        //            string message2 = "your oppenent has been disconneected. \n";
        //            Byte[] buffer2 = Encoding.Default.GetBytes(message2);
        //            foreach (var client in clientSockets)
        //            {
        //                client.client.Send(buffer2);
        //            }
        //            connected = false;
        //        }
        //    }
        //}

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            string message = textBox_message.Text;
            if (message != "" && message.Length <= 64)
            {
                Byte[] buffer = Encoding.Default.GetBytes(message);
                foreach (var t in clientSockets)
                {
                    try
                    {
                        t.client.Send(buffer);
                    }
                    catch
                    {
                        logs.AppendText("There is a problem! Check the connection...\n");
                        terminating = true;
                        textBox_message.Enabled = false;
                        button_send.Enabled = false;
                        textBox_port.Enabled = true;
                        button_listen.Enabled = true;
                        //serverSocket.Close();
                    }

                }
            }
        }


        //void The_Game(Socket s1, Socket s2, List<obj> clients)
        //{
        //    Dictionary<string, int> soru_cevap = new Dictionary<string, int>();
        //    string path = "questions.txt";
        //    string textFile = path;
        //    string[] lines = File.ReadAllLines(textFile);

        //    int size_of_lines = 0;
        //    foreach (var s in lines)
        //        size_of_lines++;


        //    for (int index = 0; index < size_of_lines; index++)
        //    {
        //        if (index % 2 == 0)
        //        {
        //            soru_cevap.Add(lines[index], 0);
        //        }

        //        if (index % 2 == 1)
        //        {
        //            soru_cevap[lines[index - 1]] = Int32.Parse(lines[index]);
        //        }

        //    }

        //    int tur_sayac = 1;
        //    question_number = Int32.Parse(quetext.Text);
        //    // size_of_lines 22
        //    for (int i = 0; i < question_number * 2; i += 2)
        //    {


        //        int s1a;
        //        int s2a;


        //        if (i < tur_sayac * lines.Count())
        //        {
        //            logs.AppendText(lines[i] + "\n");
        //            Byte[] buffer = Encoding.Default.GetBytes(lines[i]);

        //            s1.Send(buffer);
        //            s2.Send(buffer);

        //            Byte[] buffer1 = new Byte[64];
        //            Byte[] buffer2 = new Byte[64];

        //            s1.Receive(buffer1);
        //            s2.Receive(buffer2);

        //            string s1ans = Encoding.Default.GetString(buffer1);
        //            string s2ans = Encoding.Default.GetString(buffer2);

        //            s1ans = s1ans.Substring(0, s1ans.IndexOf("\0"));
        //            s2ans = s2ans.Substring(0, s2ans.IndexOf("\0"));

        //            Int32.TryParse(s1ans, out s1a);
        //            Int32.TryParse(s2ans, out s2a);

        //            cevap_takip(s1a, s2a, soru_cevap[lines[i]], lines[i]);
        //        }

        //        else
        //        {
        //            tur_sayac += 1;
        //            logs.AppendText(lines[i - lines.Count()] + "\n");
        //            Byte[] buffer = Encoding.Default.GetBytes(lines[i - lines.Count()]);

        //            s1.Send(buffer);
        //            s2.Send(buffer);

        //            Byte[] buffer1 = new Byte[64];
        //            Byte[] buffer2 = new Byte[64];

        //            s1.Receive(buffer1);
        //            s2.Receive(buffer2);

        //            string s1ans = Encoding.Default.GetString(buffer1);
        //            string s2ans = Encoding.Default.GetString(buffer2);

        //            s1ans = s1ans.Substring(0, s1ans.IndexOf("\0"));
        //            s2ans = s2ans.Substring(0, s2ans.IndexOf("\0"));

        //            Int32.TryParse(s1ans, out s1a);
        //            Int32.TryParse(s2ans, out s2a);

        //            cevap_takip(s1a, s2a, soru_cevap[lines[i - lines.Count()]], lines[i - lines.Count()]);
        //        }

        //    }
        //    //clients[0].score = puan1; clients[1].score = puan2;


        //    logs.AppendText(puan1.ToString() + "\t" + puan2.ToString());



        //    void cevap_takip(double cevap1, double cevap2, int gercek_cevap, string question)
        //    {
        //        string sendto = "";
        //        sendto += ("\nQuestion is:" + question + "\n" + name_list[0] + "'s Answer is:" + cevap1 + "\n" + name_list[1] + "'s Answer is:" + cevap2 + "\nCorrect Answer is: " + gercek_cevap + "\n");
        //        logs.AppendText(sendto);



        //        double fark_cevap1_gercek = Math.Abs(gercek_cevap - cevap1);
        //        double fark_cevap2_gercek = Math.Abs(gercek_cevap - cevap2);

        //        if (fark_cevap1_gercek > fark_cevap2_gercek)
        //        {
        //            puan2++;
        //            logs.AppendText("Winner of the round is " + name_list[1] + "\n");
        //        }

        //        else if (fark_cevap1_gercek < fark_cevap2_gercek)
        //        {
        //            puan1++;
        //            logs.AppendText("Winner of the round is " + name_list[0] + "\n");
        //        }

        //        else
        //        {
        //            puan1 += 0.5;
        //            puan2 += 0.5;
        //            logs.AppendText("Tie\n");

        //        }

        //        logs.AppendText("Points:    " + puan1.ToString() + "\t" + puan2.ToString() + "\n");
        //    }

                

        //    string winner;
        //    if (puan1 > puan2)
        //        winner = name_list[0];
        //    if (puan1 < puan2)
        //        winner = name_list[1];
        //    else
        //        winner = "No one, tie";

        //    string Game_end_msg = "\n Game has been finished the winner is " + winner + "\n";
        //    Byte[] buffer34 = Encoding.Default.GetBytes(Game_end_msg);
        //    s1.Send(buffer34);
        //    s2.Send(buffer34);

        //    string ack = "Congratulations!!";
        //    Byte[] bufferack = Encoding.Default.GetBytes(ack);
        //    s1.Send(bufferack);
        //    s2.Send(bufferack);




        //}

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            question_number = Int32.Parse(quetext.Text);
            logs.Clear();
            clientSockets.Clear();
            game.ResetGame(Int32.Parse(quetext.Text));   
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox_port_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            clicked = true;

            game.ShouldGameStart(clicked);
        }
    }
}

