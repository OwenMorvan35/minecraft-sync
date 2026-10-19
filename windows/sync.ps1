param([ValidateSet('list','pull','push','check')][string]$Mode='list',[string]$SelectionFile)
$ErrorActionPreference='Stop'
$cfg=Get-Content (Join-Path $PSScriptRoot 'config.json') -Raw | ConvertFrom-Json
$hasher=[Security.Cryptography.SHA256]::Create()
try {$repoId=([BitConverter]::ToString($hasher.ComputeHash([Text.Encoding]::UTF8.GetBytes($cfg.Remote.Trim())))).Replace('-','').ToLowerInvariant()} finally {$hasher.Dispose()}
$repo=Join-Path $env:LOCALAPPDATA "MinecraftSync\repositories\$repoId"
$saves=Join-Path $env:APPDATA '.minecraft\saves'
$env:GIT_SSH_COMMAND='ssh -i "'+($cfg.Key -replace '\\','/')+'" -o IdentitiesOnly=yes -o BatchMode=yes -o StrictHostKeyChecking=yes'
$env:GIT_TERMINAL_PROMPT='0'
function GitRun {
  & $cfg.Git -C $repo @args
  if($LASTEXITCODE -ne 0){throw "Git a echoue : $args"}
}
if($Mode -eq 'check') {
  & $cfg.Git ls-remote $cfg.Remote HEAD
  if($LASTEXITCODE -ne 0){throw 'Connexion GitHub impossible.'}
  Write-Output 'Connexion GitHub OK.'
  exit
}
if(!(Test-Path (Join-Path $repo '.git'))){
  New-Item -ItemType Directory -Force (Split-Path $repo) | Out-Null
  & $cfg.Git clone $cfg.Remote $repo
  if($LASTEXITCODE -ne 0){throw 'Clonage impossible.'}
}
$worlds=Join-Path $repo 'worlds'
if($Mode -eq 'list') {
  $names=@()
  if(Test-Path $worlds){$names=@(Get-ChildItem $worlds -Directory | Select-Object -ExpandProperty Name)}
  if(Test-Path $saves){$names+=@(Get-ChildItem $saves -Directory | Where-Object {Test-Path (Join-Path $_.FullName 'level.dat')} | Select-Object -ExpandProperty Name)}
  $names | Sort-Object -Unique
  exit
}
if(Get-Process java,javaw -ErrorAction SilentlyContinue){throw 'Ferme Minecraft (et les autres programmes Java) avant de synchroniser.'}
$selected=Get-Content -LiteralPath $SelectionFile -Raw | ConvertFrom-Json
$selected=@($selected)
if(!$selected.Count){throw 'Selectionne au moins un monde.'}
foreach($name in $selected){if(!$name -or $name -in @('.','..') -or $name.IndexOfAny([IO.Path]::GetInvalidFileNameChars()) -ge 0){throw 'Nom de monde invalide.'}}
if(GitRun status --porcelain){throw 'Le depot contient des changements non envoyes. Ouvre le dossier du depot pour les verifier avant de continuer.'}
& $cfg.Git -C $repo ls-remote --heads origin | Out-Null
if($LASTEXITCODE -ne 0){throw 'Impossible de lire le depot distant.'}
$heads=@(& $cfg.Git -C $repo ls-remote --heads origin)
if($LASTEXITCODE -ne 0){throw 'Impossible de lire les branches distantes.'}
if($heads.Count -gt 0){GitRun pull --ff-only}
function Mirror($from,$to){
  New-Item -ItemType Directory -Force $to | Out-Null
  & robocopy $from $to /MIR /XF session.lock /NFL /NDL /NJH /NJS /NP | Out-Null
  if($LASTEXITCODE -ge 8){throw "Copie impossible : $from"}
}
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss-fff'
foreach($name in $selected){
  if($Mode -eq 'pull'){$from=Join-Path $worlds $name;$to=Join-Path $saves $name}
  else{$from=Join-Path $saves $name;$to=Join-Path $worlds $name}
  if(!(Test-Path (Join-Path $from 'level.dat'))){throw "Monde source absent ou invalide : $name"}
}
foreach($name in $selected){
  if($Mode -eq 'pull'){$from=Join-Path $worlds $name;$to=Join-Path $saves $name}
  else{$from=Join-Path $saves $name;$to=Join-Path $worlds $name}
  if(Test-Path $to){Mirror $to (Join-Path $env:LOCALAPPDATA "MinecraftSync\backups\$stamp\$Mode\$name")}
  Mirror $from $to
  Write-Output "Copie OK : $name"
}
if($Mode -eq 'push'){
  foreach($name in $selected){GitRun add -A -- "worlds/$name"}
  & $cfg.Git -C $repo diff --cached --quiet
  if($LASTEXITCODE -eq 1){
    & $cfg.Git -C $repo -c user.name=MinecraftSync -c user.email=minecraft-sync@local commit -m "Sauvegarde depuis $env:COMPUTERNAME ($stamp)"
    if($LASTEXITCODE -ne 0){throw 'Commit impossible.'}
  }elseif($LASTEXITCODE -ne 0){throw 'Verification Git impossible.'}
  $branch=GitRun symbolic-ref --short HEAD
  GitRun push --set-upstream origin "HEAD:refs/heads/$branch"
}
Write-Output 'Synchronisation terminee.'
exit 0
