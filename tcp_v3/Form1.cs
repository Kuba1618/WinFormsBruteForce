using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace tcp_v3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Polaczenie pol = new Polaczenie();

        public delegate void DodajKolorowyTekst(RichTextBox RichTextBox, string Text, Color kolor);
        private void DodajKolorowyTekstFn(RichTextBox rtb, string tekst, Color kolor)
        {
            var StartIndex = rtb.TextLength;
            rtb.AppendText(tekst);
            var EndIndex = rtb.TextLength;
            rtb.Select(StartIndex, EndIndex - StartIndex);
            rtb.SelectionColor = kolor;
        }


        /// <summary>
        /// Dodaje do RichTextBox'a tekst o podanym kolorze. 
        /// Modyfikuje w sposób bezpieczny RichTextBoxa z innych wątków.
        /// </summary>
        /// <param name="RTB">Gdzie</param>
        /// <param name="Text">Co</param>
        /// <param name="Color">W jakim kolorze</param>
        private void AppendColoredText(RichTextBox RTB, string Text, Color kolor)
        {
            if (RTB.InvokeRequired)
            {
                RTB.Invoke(new DodajKolorowyTekst(DodajKolorowyTekstFn), RTB, Text, kolor);
            }
            else
            {
                DodajKolorowyTekstFn(RTB, Text, kolor);
            }
        }

        private void klientOdlaczAsync(Form fm)
        {
            if (fm.InvokeRequired)
            {
                fm.Invoke(new MethodInvoker(() => { klientOdlacz(); }));
            }
            else
                klientOdlacz();
        }

        private void serwerStop()
        {
            pol.odlacz();
            //wycofanie obsługi zdarzeń
            pol.KomunikatPrzybyl -= pol_KomunikatPrzybyl;
            pol.PolaczenieUstanowione -= pol_PolaczenieUstanowione;
            pol.PolaczenieZerwane -= pol_PolaczenieZerwane;
            //przywrócenie stanu pierwotnego
            buttonSerwuj.Text = "Serwuj";
            buttonPolacz.Enabled = true;
            textBoxAdres.Enabled = true;
            textBoxPort.Enabled = true;
            textBoxKomunikat.Enabled = false;
            buttonWyslij.Enabled = false;
            AppendColoredText(richTextBoxOdbior, "Serwer STOP\n", Color.Red);
        }

        private void serwerStart()
        {
            //dodanie obsługi zdarzeń
            
            pol.KomunikatPrzybyl += new Polaczenie.KomunikatEventsHandler(pol_KomunikatPrzybyl);
            pol.PolaczenieUstanowione += new Polaczenie.PolaczenieUstanowioneEventsHandler(pol_PolaczenieUstanowione);
            pol.PolaczenieZerwane += new Polaczenie.PolaczenieZerwaneEventsHandler(pol_PolaczenieZerwane);
            buttonSerwuj.Text = "Stop"; //zmiana nazwy
            buttonPolacz.Enabled = false; //zmiany stanów
            textBoxAdres.Enabled = false;
            textBoxPort.Enabled = false;
            textBoxKomunikat.Enabled = true;
            buttonWyslij.Enabled = true;
            AppendColoredText(richTextBoxOdbior, "Serwer START\n", Color.Red);
        }

        private void buttonSerwuj_Click(object sender, EventArgs e)
        {
            if (buttonSerwuj.Text == "Stop") //mamy dwa stany: albo wyłączamy
            {
                try
                {
                    serwerStop();
                }
                catch (Exception ex)
                {
                    AppendColoredText(richTextBoxOdbior, "Bład odłączenia: \n", Color.Red);
                    AppendColoredText(richTextBoxOdbior, ex.Message + "\n", Color.Red);
                }
            }
            else //albo włączamy
            {
                try
                {
                    if (pol.startSerwer(textBoxAdres.Text, int.Parse(textBoxPort.Text)))
                    {
                        serwerStart();
                    }
                }
                catch (Exception ex)
                {
                    AppendColoredText(richTextBoxOdbior, "Bład połączenia: \n", Color.Red);
                    AppendColoredText(richTextBoxOdbior, ex.Message + "\n", Color.Red);
                }
            }
        }

        void pol_PolaczenieZerwane(object sender, PolaczenieZerwaneEventArgs e)
        {
            AppendColoredText(richTextBoxOdbior, "Połączenie o id: " + e.idPolaczenia + " zerwane" + "\n", Color.Red);
        }

        void pol_PolaczenieKlientZerwane(object sender, PolaczenieZerwaneEventArgs e)
        {
            AppendColoredText(richTextBoxOdbior, "Połączenie klienta o id: " + e.idPolaczenia + " zerwane" + "\n", Color.Red);
            klientOdlaczAsync(this); //wołana metoda bezpiecznej zmiany na formatce (ponieważ ten wątek nie jest właścicielem formatki).
        }

        void pol_PolaczenieUstanowione(object sender, PolaczenieUstanowioneEventArgs e)
        {
            AppendColoredText(richTextBoxOdbior, "Połączono z: " , Color.Red);
            AppendColoredText(richTextBoxOdbior, e.adres.ToString() + "\n", Color.Blue);
        }

        void pol_KomunikatPrzybyl(object sender, KomunikatEventArgs e)
        {
            AppendColoredText(richTextBoxOdbior, "["+e.kom.czasOdbioru.ToString()+"] ", Color.Blue);
            AppendColoredText(richTextBoxOdbior, e.kom.tresc, Color.Green);
            AppendColoredText(richTextBoxOdbior, "\n", Color.Green);
            //START BruteForceAlghoritm
            string s = BreakPasswords();
            AppendColoredText(richTextBoxOdbior, s + "\n", Color.DeepSkyBlue);
        }

        //----------------------------- Brute Force -------------------------------------------------------

        public static string BreakPasswords()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString() + "\\passwordsToBreak" + ".txt";
            int numberOfFileLines = File.ReadLines(path).Count();

            DateTime start = DateTime.Now;
            for (int i = 1; i < numberOfFileLines; i++)
            {
                string password = File.ReadLines(path).Skip(i - 1).Take(1).First();
                BruteForce(password);
            }

            //Koniec mierzenia czasu
            DateTime stop = DateTime.Now;
            TimeSpan czasWykonania = stop - start;
            int czasLiczbowy = Convert.ToInt32(czasWykonania.TotalMilliseconds);

            StringBuilder stringBuilder = new StringBuilder("");
            stringBuilder.Append("It took about:  " + czasWykonania.TotalSeconds + "s\n");
            stringBuilder.Append("It took about:  " + czasLiczbowy + "ms\n");
            stringBuilder.Append("It took about:  " + czasLiczbowy / numberOfFileLines + "ms / hasło\n");
            return stringBuilder.ToString();
        }

        public static void BruteForce(string password1)
        {
            BruteForceIter b = new BruteForceIter();
            b.min = 2;
            b.max = 4;
            b.alphabet = "!#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

            foreach (string result in b)
            {
                //Console.Write(result + "\r");
                if (result == password1)
                {
                    Console.WriteLine(result);
                    b.SaveResultToFile(result);
                    return;
                }
            }

        }
        //----------------------------- KONIEC CZĘŚCI Brute Force -------------------------------------------------------

        private void klientOdlacz()
        {
            pol.odlacz();
            pol.KomunikatPrzybyl -= pol_KomunikatPrzybyl;
            pol.PolaczenieUstanowione -= pol_PolaczenieUstanowione;
            pol.PolaczenieZerwane -= pol_PolaczenieKlientZerwane;
            textBoxKomunikat.Enabled = false;
            buttonWyslij.Enabled = false;
            buttonSerwuj.Enabled = true;
            textBoxAdres.Enabled = true;
            textBoxPort.Enabled = true;
            buttonPolacz.Text = "Połacz";
            AppendColoredText(richTextBoxOdbior, "Odłączono \n", Color.Red);
        }

        private void klientPolacz()
        {
            pol.KomunikatPrzybyl += new Polaczenie.KomunikatEventsHandler(pol_KomunikatPrzybyl);
            pol.PolaczenieUstanowione += new Polaczenie.PolaczenieUstanowioneEventsHandler(pol_PolaczenieUstanowione);
            pol.PolaczenieZerwane += new Polaczenie.PolaczenieZerwaneEventsHandler(pol_PolaczenieKlientZerwane);
            textBoxKomunikat.Enabled = true;
            buttonWyslij.Enabled = true;
            buttonSerwuj.Enabled = false;
            textBoxAdres.Enabled = false;
            textBoxPort.Enabled = false;
            buttonPolacz.Text = "Odłącz";
            AppendColoredText(richTextBoxOdbior, "Podłączono \n", Color.Red);
        }

        private void buttonPolacz_Click(object sender, EventArgs e)
        {
            if (buttonPolacz.Text == "Odłącz")
            {
                try
                {
                    klientOdlacz();
                }
                catch (Exception ex)
                {
                    AppendColoredText(richTextBoxOdbior, "Błąd odłaczenia: \n", Color.Red);
                    AppendColoredText(richTextBoxOdbior, ex.Message + "\n", Color.Red);
                }
            }
            else
            {
                try
                {
                    if (pol.startKlient(textBoxAdres.Text, Int32.Parse(textBoxPort.Text)))
                    {
                        klientPolacz();
                    }
                }
                catch (Exception ex)
                {
                    AppendColoredText(richTextBoxOdbior, "Błąd podłaczenia: \n", Color.Red);
                    AppendColoredText(richTextBoxOdbior, ex.Message + "\n", Color.Red);
                }
            }
        }

        private void buttonWyslij_Click(object sender, EventArgs e)
        {
            string str = textBoxKomunikat.Text;
            Komunikat kom = new Komunikat();
            kom.czasNadania = DateTime.Now;
            kom.nadawca = "nad1";
            kom.tresc = str;
            kom.wazna = false;
            pol.wyslij(kom);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            pol.odlacz();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
