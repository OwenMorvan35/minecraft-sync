# Portage macOS — proposition, non implémentée

Une application macOS peut fournir les mêmes boutons dans un bundle `.app`, éventuellement distribué dans un `.dmg`. L'EXE Windows Forms actuel ne fonctionne pas directement sur macOS.

Pour partager une interface entre Windows et macOS, une évolution possible est C#/.NET avec Avalonia. Il faut remplacer Windows Forms et les commandes Windows (`robocopy`, chemins AppData, launcher Microsoft Store). Le moteur de copie doit conserver les mêmes règles de sélection, exclusion de session.lock et sauvegarde avant remplacement.

Chemins envisagés : mondes Java dans `~/Library/Application Support/minecraft/saves` ; clone et copies de secours dans un dossier propre à l'application sous `~/Library/Application Support/MinecraftSync`. Lancer le launcher macOS avec `open -a Minecraft` en permettant un chemin configurable.

Utiliser Git et les clés SSH propres à chaque Mac ; ne pas copier la clé privée du PC dans le dépôt. Prévoir les architectures Apple Silicon et Intel selon les machines utilisées. Prévoir signature et notarisation pour une distribution fluide, et tester réellement le bundle sur macOS.

Références :
- https://docs.avaloniaui.net/docs/supported-platforms
- https://docs.avaloniaui.net/docs/deployment/macos

Ce document décrit une piste de portage ; aucune application Mac n'est livrée dans la version initiale.
