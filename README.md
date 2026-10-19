# Minecraft Sync

Petite interface Windows pour choisir les mondes Minecraft Java à récupérer ou envoyer dans le dépôt Git SSH de votre choix, dans son sous-dossier `worlds/`.

## Utilisation

1. Installer Git pour Windows et autoriser sa clé SSH sur GitHub.
2. Le fichier `windows/config.example.json` documente le format de configuration ; aucune configuration personnelle ne doit être publiée.
3. Compiler : `powershell -NoProfile -ExecutionPolicy Bypass -File windows/build.ps1`.
4. Lancer `windows/MinecraftSync.exe` en gardant `sync.ps1` et `config.json` à côté.
5. Renseigner dans le formulaire le dépôt SSH, le fichier de clé privée et git.exe, puis cliquer sur **Enregistrer et charger**.
6. Cocher les mondes ; **Récupérer** avant de jouer et **Envoyer** après avoir quitté le jeu.

Chaque URL utilise son propre clone local ; les mondes Minecraft locaux restent partagés entre les dépôts. Un dépôt vide est accepté pour un premier envoi. Format SSH accepté : `git@hote:proprietaire/depot.git` (pas HTTPS ou ssh://). La clé doit être utilisable sans invite interactive, via un agent si elle est protégée. L’hôte SSH doit déjà être connu du client SSH.

Conserver l’EXE et sync.ps1 dans un dossier accessible en écriture ; la configuration y est enregistrée dans config.json, ignoré par Git. Exclure config.json des archives publiques. Licence open source à choisir par le propriétaire avant publication.

**Récupérer remplace la copie locale ; Envoyer remplace celle du dépôt pour les mondes sélectionnés.** Les anciennes copies sont sauvegardées localement. Les mondes Minecraft ne se fusionnent pas : éviter de jouer sur les deux PC sans synchroniser.

Le bouton Ouvrir Minecraft ouvre le launcher officiel Microsoft Store détecté lors de la création de l'outil. Aucun transfert automatique ne se déclenche par ce bouton ou par les anciens raccourcis.

## Documents

- [Spécifications fonctionnelles et techniques](docs/SPECS.md)
- [Portage macOS envisagé](docs/MACOS.md)

Les mondes, clés privées et configurations personnelles ne font pas partie de ce dépôt.
