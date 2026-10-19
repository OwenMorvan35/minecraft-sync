# Minecraft Sync

Petite interface Windows pour choisir les mondes Minecraft Java à récupérer ou envoyer dans le dépôt privé `OwenMorvan35/minecraft-saves`.

## Utilisation

1. Installer Git pour Windows et autoriser sa clé SSH sur GitHub.
2. Copier `windows/config.example.json` vers `windows/config.json` et renseigner les chemins Git et clé SSH de ce PC.
3. Compiler : `powershell -NoProfile -ExecutionPolicy Bypass -File windows/build.ps1`.
4. Lancer `windows/MinecraftSync.exe` en gardant `sync.ps1` et `config.json` à côté.
5. Cocher les mondes ; **Récupérer** avant de jouer et **Envoyer** après avoir quitté le jeu.

**Récupérer remplace la copie locale ; Envoyer remplace celle du dépôt pour les mondes sélectionnés.** Les anciennes copies sont sauvegardées localement. Les mondes Minecraft ne se fusionnent pas : éviter de jouer sur les deux PC sans synchroniser.

Le bouton Ouvrir Minecraft ouvre le launcher officiel Microsoft Store détecté lors de la création de l'outil. Aucun transfert automatique ne se déclenche par ce bouton ou par les anciens raccourcis.

## Documents

- [Spécifications fonctionnelles et techniques](docs/SPECS.md)
- [Portage macOS envisagé](docs/MACOS.md)

Les mondes, clés privées et configurations personnelles ne font pas partie de ce dépôt.
