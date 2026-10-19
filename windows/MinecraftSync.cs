using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

class MinecraftSync : Form {
  CheckedListBox worlds=new CheckedListBox();
  TextBox log=new TextBox();
  FlowLayoutPanel actions=new FlowLayoutPanel();
  string root=AppDomain.CurrentDomain.BaseDirectory;
  TextBox remote=new TextBox(), key=new TextBox(), git=new TextBox();
  TableLayoutPanel settings=new TableLayoutPanel();
  string activeRemote="";
  JavaScriptSerializer json=new JavaScriptSerializer();
  string RepoPath(){using(var hash=SHA256.Create()){return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MinecraftSync","repositories",BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(activeRemote))).Replace("-","").ToLowerInvariant());}}
  void SettingsRow(string label,TextBox field,int row){settings.Controls.Add(new Label{Text=label,AutoSize=true,Anchor=AnchorStyles.Left},0,row);field.Dock=DockStyle.Fill;settings.Controls.Add(field,1,row);}
  bool SaveSettings(){
    string url=remote.Text.Trim();
    if(!Regex.IsMatch(url,@"^git@[a-zA-Z0-9.-]+:[a-zA-Z0-9_.-]+/[a-zA-Z0-9_.-]+$")){MessageBox.Show("Indique une URL SSH de la forme git@github.com:utilisateur/depot.git");return false;}
    if(!File.Exists(key.Text.Trim()) || !File.Exists(git.Text.Trim())){MessageBox.Show("Choisis un fichier de clé SSH privée et le programme git.exe existants.");return false;}
    try{File.WriteAllText(Path.Combine(root,"config.json"),json.Serialize(new Dictionary<string,string>{{"Remote",url},{"Key",key.Text.Trim()},{"Git",git.Text.Trim()}}),new UTF8Encoding(false));
      if(activeRemote!=url)worlds.Items.Clear();activeRemote=url;return true;
    }catch(Exception e){MessageBox.Show(e.Message);return false;}
  }
  void LoadSettings(){
    git.Text=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Git","cmd","git.exe");
    key.Text=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".ssh","id_ed25519");
    try{string p=Path.Combine(root,"config.json");if(File.Exists(p)){var cfg=json.Deserialize<Dictionary<string,string>>(File.ReadAllText(p));remote.Text=cfg["Remote"];key.Text=cfg["Key"];git.Text=cfg["Git"];activeRemote=remote.Text.Trim();}}
    catch(Exception e){log.AppendText("Configuration à corriger : "+e.Message+"\r\n");}
  }
  public MinecraftSync(){
    Text="Minecraft — mes sauvegardes"; Size=new Size(860,780); MinimumSize=new Size(820,730); StartPosition=FormStartPosition.CenterScreen;
    Font=new Font("Segoe UI",10);
    var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(18),RowCount=6,ColumnCount=1};
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute,160));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,66));layout.RowStyles.Add(new RowStyle(SizeType.Percent,45));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,85));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,28));layout.RowStyles.Add(new RowStyle(SizeType.Percent,55));
    settings.Dock=DockStyle.Fill;settings.ColumnCount=3;settings.RowCount=4;
    settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,100));settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,115));
    SettingsRow("Dépôt SSH",remote,0);SettingsRow("Clé SSH",key,1);SettingsRow("Git",git,2);
    foreach(var field in new[]{key,git}){var target=field;var browse=new Button{Text="Parcourir…",AutoSize=true};browse.Click+=(s,e)=>{using(var dlg=new OpenFileDialog()){if(dlg.ShowDialog()==DialogResult.OK)target.Text=dlg.FileName;}};settings.Controls.Add(browse,2,field==key?1:2);}
    var save=new Button{Text="Enregistrer et charger",AutoSize=true};save.Click+=(s,e)=>Run("list");settings.Controls.Add(save,1,3);
    var intro=new Label{Text="Coche les mondes à synchroniser.\nFerme le jeu avant un transfert. Récupérer remplace la copie locale ; envoyer remplace celle sur GitHub.",Dock=DockStyle.Fill};
    worlds.Dock=DockStyle.Fill;worlds.CheckOnClick=true;
    actions.Dock=DockStyle.Fill;actions.Padding=new Padding(0,8,0,0);
    AddButton("Actualiser",()=>Run("list"));AddButton("Récupérer ↓",()=>Transfer("pull"));AddButton("Envoyer ↑",()=>Transfer("push"));
    AddButton("Ouvrir Minecraft",()=>{try{Process.Start("explorer.exe","shell:AppsFolder\\Microsoft.4297127D64EC6_8wekyb3d8bbwe!Minecraft");}catch(Exception e){MessageBox.Show(e.Message);}});
    AddButton("Copies de secours",()=>{string p=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MinecraftSync","backups");Directory.CreateDirectory(p);Process.Start("explorer.exe",p);});
    AddButton("Dépôt local",()=>{string p=RepoPath();Directory.CreateDirectory(p);Process.Start("explorer.exe",p);});
    log.Multiline=true;log.ReadOnly=true;log.ScrollBars=ScrollBars.Vertical;log.Dock=DockStyle.Fill;log.Font=new Font("Consolas",9);
    layout.Controls.Add(settings,0,0);layout.Controls.Add(intro,0,1);layout.Controls.Add(worlds,0,2);layout.Controls.Add(actions,0,3);layout.Controls.Add(new Label{Text="Journal des opérations",Dock=DockStyle.Fill},0,4);layout.Controls.Add(log,0,5);Controls.Add(layout);
    LoadSettings();
    Shown+=(s,e)=>{if(activeRemote!="")Run("list");else log.AppendText("Colle l'URL SSH de ton dépôt de mondes, choisis ta clé puis clique sur Enregistrer et charger.\r\n");};
    FormClosing+=(s,e)=>{if(!actions.Enabled){e.Cancel=true;MessageBox.Show("Attends la fin du transfert avant de fermer.");}};
  }
  void AddButton(string text,Action action){var b=new Button{Text=text,AutoSize=true,Height=32,Margin=new Padding(0,0,8,5)};b.Click+=(s,e)=>action();actions.Controls.Add(b);}
  string Quote(string s){return "'"+s.Replace("'","''")+"'";}
  void Transfer(string mode){
    if(remote.Text.Trim()!=activeRemote){MessageBox.Show("Enregistre et charge le dépôt avant de choisir les mondes.");return;}
    if(worlds.CheckedItems.Count==0){MessageBox.Show("Coche au moins un monde.");return;}
    var names=new List<string>();foreach(var item in worlds.CheckedItems)names.Add(item.ToString());
    string verb=mode=="pull"?"Récupérer depuis GitHub et remplacer les mondes locaux":"Envoyer vers GitHub et remplacer les mondes distants";
    if(MessageBox.Show(verb+" ?\n\n"+String.Join("\n",names.ToArray())+"\n\nUne copie de secours sera conservée.","Confirmer le transfert",MessageBoxButtons.OKCancel)!=DialogResult.OK)return;
    Run(mode);
  }
  async void Run(string mode){
    if(!SaveSettings())return;
    actions.Enabled=false;worlds.Enabled=false;settings.Enabled=false;
    string selection=Path.GetTempFileName();
    try{
      var names=new List<string>();foreach(var item in worlds.CheckedItems)names.Add("\""+item.ToString().Replace("\\","\\\\").Replace("\"","\\\"")+"\"");
      File.WriteAllText(selection,"["+String.Join(",",names.ToArray())+"]",new UTF8Encoding(false));
      string command="$ErrorActionPreference='Stop'; try { & "+Quote(Path.Combine(root,"sync.ps1"))+" -Mode "+Quote(mode)+" -SelectionFile "+Quote(selection)+" } catch { Write-Output $_.Exception.Message; exit 1 }";
      log.AppendText("\r\n> "+mode+"\r\n");
      var result=await Task.Run(()=>{
        var psi=new ProcessStartInfo("powershell.exe","-NoProfile -ExecutionPolicy Bypass -EncodedCommand "+Convert.ToBase64String(Encoding.Unicode.GetBytes(command))){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true,StandardOutputEncoding=Encoding.UTF8,StandardErrorEncoding=Encoding.UTF8};
        // PowerShell uses UTF-8 for its redirected console output.
        string utf8="[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false); "+command;
        psi.Arguments="-NoProfile -ExecutionPolicy Bypass -EncodedCommand "+Convert.ToBase64String(Encoding.Unicode.GetBytes(utf8));
        using(var p=Process.Start(psi)){
          var stdout=p.StandardOutput.ReadToEndAsync();var stderr=p.StandardError.ReadToEndAsync();p.WaitForExit();Task.WaitAll(stdout,stderr);
          return Tuple.Create(p.ExitCode,stdout.Result,stderr.Result);
        }
      });
      if(mode=="list" && result.Item1==0){worlds.Items.Clear();foreach(string line in result.Item2.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries))worlds.Items.Add(line);log.AppendText("Liste chargée : "+worlds.Items.Count+" monde(s).\r\n");}
      else log.AppendText(result.Item2+"\r\n"+result.Item3);
      if(result.Item1!=0){if(mode=="list")log.AppendText(result.Item2+result.Item3);MessageBox.Show("L'opération a échoué. Consulte le journal.","Minecraft Sync");}
    }catch(Exception e){log.AppendText(e.Message+"\r\n");MessageBox.Show(e.Message);}
    finally{File.Delete(selection);actions.Enabled=true;worlds.Enabled=true;settings.Enabled=true;}
  }
  [STAThread] static void Main(){
    bool created;using(var mutex=new System.Threading.Mutex(true,"Local\\MinecraftSync",out created)){
      if(!created){MessageBox.Show("Minecraft Sync est déjà ouvert.");return;}
      Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new MinecraftSync());
    }
  }
}
