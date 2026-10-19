# Spécifications — Minecraft Sync

## Objectif

Gérer manuellement les sauvegardes Minecraft Java entre plusieurs PC et un Mac via GitHub. Ce dépôt contient l'outil et ses spécifications ; `minecraft-saves` conserve les mondes et leur historique. Version initiale : Windows, C# Windows Forms et PowerShell 5.1, .NET Framework.

## Interface actuelle

- Configuration graphique : URL SSH `git@hote:proprietaire/depot.git`, fichier de clé privée et programme Git, boutons Parcourir et Enregistrer et charger. Sans configuration, le premier lancement présente le formulaire sans connexion automatique.
- Changer de dépôt vide la sélection ; chaque URL possède un clone distinct. Les mondes Minecraft locaux restent partagés entre les dépôts.

- Liste à cocher : union des sous-dossiers `worlds/` du clone local et des mondes locaux possédant `level.dat` ; tri et dédoublonnage.
- Actualiser : recharge la liste du clone local ; ne récupère pas les nouveaux noms distants tant qu'un pull n'a pas été effectué.
- Récupérer : confirme les noms choisis, met à jour le clone avec `git pull --ff-only`, sauvegarde les destinations existantes puis copie les mondes choisis vers Minecraft.
- Envoyer : confirme les noms choisis, met à jour le clone avec `git pull --ff-only` si le distant contient des branches, sauvegarde les anciennes copies du dépôt, copie les mondes locaux choisis, stage uniquement leurs chemins, commit si nécessaire et push vers la branche courante du clone. Un dépôt vide peut recevoir un premier envoi.
- Ouvrir Minecraft : ouvre le launcher officiel Windows ; pas de synchronisation implicite.
- Copies de secours et Dépôt local : ouvrent les dossiers correspondants dans l'Explorateur.
- Journal : affiche les résultats et erreurs à la fin de l'opération ; boutons désactivés durant l'opération ; fermeture de la fenêtre bloquée jusqu'à sa fin.
- Une seule instance d'interface par session Windows.

## Données et configuration

Configuration `config.json` près de l'exécutable : `Git` (chemin de git.exe), `Key` (chemin de clé SSH privée locale), `Remote` (URL SSH du dépôt de mondes).

Chemins Windows :

- Mondes locaux : `%APPDATA%\.minecraft\saves`
- Clone de travail : `%LOCALAPPDATA%\MinecraftSync\repositories\<SHA-256 de l’URL SSH>`
- Copies de secours : `%LOCALAPPDATA%\MinecraftSync\backups\<horodatage>\<pull|push>\<monde>`

Le clone est créé au premier chargement. Git emploie la clé explicitement configurée, `IdentitiesOnly`, `BatchMode` et une vérification stricte des hôtes. L'empreinte de GitHub doit donc avoir été validée auparavant via un client SSH de confiance. Aucune clé privée n'est embarquée ou envoyée.

## Règles de transfert

- Le jeu doit être fermé. La présence de `java` ou `javaw` bloque le transfert, y compris pour un autre programme Java.
- Au moins un monde est requis. Les noms invalides et traversées de chemin sont rejetés.
- Tous les mondes sélectionnés doivent exister à la source et posséder `level.dat` avant de commencer les copies.
- Un clone Git contenant des modifications non commitées bloque le transfert.
- `pull --ff-only` refuse une divergence de commits ; pas de merge, reset ou force push automatique.
- Robocopy `/MIR` réplique les fichiers et suppressions dans chaque monde sélectionné, en excluant `session.lock`.
- Chaque destination existante est copiée dans les sauvegardes avant remplacement. Les sauvegardes ne sont pas purgées automatiquement.
- Les mondes non sélectionnés ne sont pas copiés vers ou depuis Minecraft. Le pull Git met à jour tout le clone de travail.
- Les commits utilisent l'identité locale `MinecraftSync <minecraft-sync@local>` et indiquent le PC et l'heure locale de la machine.
- Si le push est refusé, le commit reste local. Ne jamais écraser le distant pour masquer une divergence.

## Limites connues

Pas de fusion des fichiers de monde, résolution de conflits, restauration graphique, contrôle de version Minecraft, récupération transactionnelle ou verrou intermachines. Un échec de copie peut laisser une copie partielle ; les copies de secours restent disponibles. L'interface n'empêche pas un autre outil de modifier les mondes ou le clone simultanément. Le dossier Minecraft et le launcher sont ceux de l'installation Java standard ; Prism, Modrinth et dossiers personnalisés nécessitent une évolution. Pas de synchronisation automatique au démarrage ou à la fermeture. L'EXE actuel est Windows uniquement et dépend des fichiers adjacents.

## Validation réalisée

Connexion SSH et lecture du dépôt réel ; découverte des mondes Nice et New World. Test sur dépôt Git local temporaire : pull d'un monde sélectionné, absence de copie d'un monde non sélectionné, modification locale puis commit/push et vérification du contenu distant, copie de secours avant un pull remplaçant une version locale. Aucun push de test ni transfert de monde vers l'installation Minecraft réelle. Compilation C# réussie ; pas de test interactif complet de l'interface effectué.

## Critères pour une prochaine version

Paramétrage graphique du dossier Minecraft et du launcher, statut local/distant par monde, aperçu des changements, fetch de la liste distante, diagnostic SSH, restauration guidée, tests automatisés reproductibles, prise en charge macOS et coordination des sessions entre machines.

## Validation de la configuration multiutilisateur

Compilation réussie. Pull et push testés avec clone séparé par URL ; premier push vers un dépôt vide sur la branche master vérifié. Le garde Java a refusé un transfert pendant que Java tournait ; pour les tests sur fichiers fictifs uniquement, ce garde a été neutralisé dans une copie temporaire du script, jamais dans le livrable. Pas de test interactif complet du nouveau formulaire.
