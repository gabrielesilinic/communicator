using System.Diagnostics;
using System.IO.Ports;
namespace communicator;

public partial class frmMain : Form
{
    SerialPort? channel;
    Stack<string> commandsb = new Stack<string>(10);
    Stack<string> commandsf = new Stack<string>(10);
    public frmMain()
    {
        InitializeComponent();
    }

    private void frmMain_Load(object sender, EventArgs e)
    {
        var ports= SerialPort.GetPortNames();
        foreach (var item in ports)
        {
            this.cmbPort.Items.Add(item);
        }
        this.cmbBaud.Items.AddRange(new string[] { "110", "134", "150", "200", "300", "600", "1200", "1800", "2400", "4800", "9600", "19200", "28800", "38400", "57600", "115200", "230400", "460800", "576000", "921600" });
        this.cmbBaud.SelectedIndex = this.cmbBaud.Items.IndexOf("9600");
    }

    private void cmbBaud_KeyDown(object sender, KeyEventArgs e)
    {
        if ((!char.IsNumber(((char)e.KeyValue)))&&e.KeyCode!=Keys.Back && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
    }

    private void btnConnect_Click(object sender, EventArgs e)
    {
        bool badformat = false;
        int val = 0;
        if (channel?.IsOpen??false)
        {
            channel.Close();
            ((Control)sender).Text = "Connect";
            return;
        }
        try
        {
            val=int.Parse(cmbBaud.Text);
        }
        catch (FormatException)
        {
            badformat = true;
        }
        finally
        {
            if (!badformat)
            {
                if (val < 100||val> 921600) 
                {
                    badformat=true;
                }
            }
        }
        if (badformat)
        {
            MessageBox.Show("bad number format");
            return;
        }
        if (this.cmbPort.SelectedIndex < 0)
        {
            MessageBox.Show("port not selected");
            return ;
        }
        channel = new SerialPort(this.cmbPort.Text, int.Parse(cmbBaud.Text));
        try
        {
            channel.Open();
        }
        catch(System.UnauthorizedAccessException)
        {
            MessageBox.Show($"Cannot connect to {cmbPort.Text}\nAnother Program may already be connected to it", "Uauthorized", MessageBoxButtons.OK,MessageBoxIcon.Error);
            channel = null;
            return ;
        }
        channel.DiscardInBuffer();
        channel.DataReceived += Channel_DataReceived;
        ((Control)sender).Text = "Disconnect";

    }

    private void Channel_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort sp = (SerialPort)sender;
        this.Invoke((string txt) =>
        {
            switch (txt.ReplaceLineEndings(""))
            {
                case "\\clear":
                case "\\c":
                    this.txtTerminal.Clear();
                    break;
                case "\\rickroll":
                    Process proc = new Process();
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.FileName = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                    proc.Start();
                    break;
                case "\\dice":
                    Random dice = new Random();
                    this.txtTerminal.Text += $"he sent you a dice which value is {dice.Next(1,6)}"+Environment.NewLine;
                    break;
                default:
                    this.txtTerminal.Text += txt;
                    break;
            }
        }, new string[]{ sp.ReadExisting()});
    }

    private void txtSend_KeyDown(object sender, KeyEventArgs e)
    {
        TextBoxBase txtbox = ((TextBoxBase)sender);
        if (e.KeyCode == Keys.Enter)
        {
            string txt = (sender as TextBoxBase)?.Text ?? "";
            maySend(txt);
            if (txt!="")
            {
                commandsb.Push(txt);
            }
            txtbox?.Clear();
            commandsf.Clear();
        }
        else if(e.KeyCode == Keys.Up && commandsb.Count() > 0)
        {
            string txt= commandsb.Pop();
            commandsf.Push(txt);
            txtbox.Text = txt;
            txtbox.SelectionStart = txt.Length;
            txtbox.SelectionLength = 0;
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Down&&commandsf.Count()>0)
        {
            string txt = commandsf.Pop();
            commandsb.Push(txt);
            txtbox.Text = txt;
            txtbox.SelectionStart = txt.Length;
            txtbox.SelectionLength = 0;
            e.SuppressKeyPress = true;
        }
    }
    private bool maySend(string message)
    {
        if(message == null || message == "") return false;
        if (this.channel?.IsOpen??false)
        {
            this.channel.Write(message+"\r\n");
        }
        return this.channel?.IsOpen ?? false;
    }
}
