# I Can't Get No Sleep

Mod SMAPI pour Stardew Valley 1.6.15 (build 24356), Windows 64 bits et SMAPI 4.5.2.

Vous pouvez continuer à jouer après 2 h du matin. À 6 h, le jeu passe au lendemain avec son cycle habituel : mise à jour de la ferme, événements nocturnes, choix de compétences éventuels et sauvegarde.

L'horloge garde sa couleur normale aux heures tardives et les bulles de fatigue horaires sont supprimées. Entre 4 h et 6 h, la lumière extérieure augmente progressivement, en respectant les pauses et la vitesse du temps.

Lors d'une transition ordinaire à 6 h avec ICGNS, la scène reste affichée avec la phrase de sauvegarde du jeu en bas à gauche pendant le traitement du lendemain. Elle se fige brièvement, puis reprend sans écran noir. Le message indique simplement « 6AM, a new day begins! » (ou sa traduction française). Les événements nocturnes spéciaux et les choix de professions conservent leur fonctionnement et leurs écrans nécessaires.

L'option facultative **Vendre le bac d'expédition à 6 h** change cette présentation : fondu noir normal, bilan des ventes lorsqu'il y a des objets, sauvegarde, puis fondu vers le jeu avec retour à la position conservée. Elle est désactivée par défaut. Elle ne fait pas récupérer d'énergie ou de santé et ne facture pas de sauvetage.

## Comportement

| Situation | Contenu du bac d'expédition |
| --- | --- |
| Éveillé jusqu'à 6 h avec les réglages par défaut | Conservé, sans versement d'argent |
| Éveillé jusqu'à 6 h avec la vente à 6 h activée | Vendu normalement, avec bilan et fondus natifs |
| Sommeil accepté dans un lit ou une tente | Vendu normalement |
| Plusieurs jours successifs sans dormir, vente à 6 h désactivée | Conservé jusqu'au prochain sommeil ou évanouissement |
| Sauvegarde puis rechargement | Les objets invendus sont remis dans le bac |
| Évanouissement par épuisement | Vendu normalement lors du passage au lendemain |

Le passage à 6 h ne déclenche ni frais de sauvetage, ni récupération d'énergie ou de santé. Les règles habituelles d'épuisement et de combat restent actives. Le sommeil volontaire garde ses règles habituelles de récupération.

Le joueur reprend à sa position sur les cartes permanentes accessibles et dans la mine, la Caverne du Crâne ou le volcan. Les étages occupés à 6 h sont conservés avec leur contenu pendant la session, y compris ceux des joueurs distants éveillés. Les autres étages sont nettoyés normalement. Après avoir quitté puis rechargé la partie, le jeu régénère l'étage et place le joueur à son entrée, au même niveau, pour éviter une apparition dans un mur. Les cartes temporaires d'événements gardent le retour habituel à la maison. Les cultures, animaux, constructions, quêtes et saisons avancent chaque jour. Les boutiques gardent leurs horaires habituels.

Les mini-bacs sont pris en compte : leur contenu invendu est transféré dans le bac principal au lendemain. Les objets conservent leur quantité, qualité et données personnalisées.

## Installation

1. Fermez Stardew Valley.
2. Après compilation, ouvrez `artifacts/ICGNS-1.3.0.zip` et extrayez le dossier **ICGNS** dans le dossier `Mods` du jeu (remplacez les fichiers d'une version précédente et conservez votre `config.json`).
3. Relancez le jeu avec SMAPI.

Le dossier installé doit contenir :

```text
Mods\ICGNS\manifest.json
Mods\ICGNS\ICGNS.dll
```

Aucun réglage n'est nécessaire. Le projet produit le ZIP dans son propre dossier ; la compilation ne modifie pas l'installation du jeu. Le paquet Nexus contient un guide d'installation en anglais.

### Configuration

Avec **Generic Mod Config Menu**, ouvrez les options des mods et sélectionnez **I Can't Get No Sleep**. Trois cases apparaissent dans cet ordre :

| Option | Défaut | Effet |
| --- | --- | --- |
| Activer le mod | Activé | En tête de page. Décocher rétablit l'évanouissement à 2 h, les avertissements de l'horloge et la vente normale. Une sauvegarde déjà commencée se termine d'abord. |
| Activer le lever du soleil | Activé | Éclairage progressif entre 4 h et 6 h. |
| Vendre le bac d'expédition à 6 h | Désactivé | Vente, bilan et fondus natifs à 6 h, puis retour à la position conservée. |

Les changements s'appliquent sans redémarrer ; utilisez **Sauvegarder** pour les conserver. GMCM reste facultatif. En multijoueur, l'hôte décide de l'activation du mod et de la vente à 6 h ; le lever du soleil reste propre à chaque joueur. Les préférences de gameplay d'un invité s'appliquent à ses propres parties.

Sans GMCM, lancez une fois le jeu avec ICGNS puis fermez-le. Les réglages par défaut de `Mods/ICGNS/config.json` sont :

```json
{
  "EnableMod": true,
  "EnableSunrise": true,
  "SellAt6AM": false
}
```

Relancez le jeu après une modification manuelle du JSON. Pour laisser **Dynamic Dawn/Dusk**, **Golden Hours** ou un autre mod gérer l'aube, passez `EnableSunrise` à `false`. Le passage au lendemain à 6 h, la sauvegarde et les règles d'expédition restent actifs. `EnableMod: false` rétablit le comportement natif ; les objets invendus stockés par ICGNS sont tout de même récupérés sans perte.

Pour passer d'une version de test à ICGNS, fermez le jeu et retirez le dossier de la version de test avant d'installer le nouveau dossier **ICGNS**. Les deux versions ne doivent pas être chargées ensemble. Les objets conservés et les marqueurs de position de la version de test sont repris automatiquement avec l'identifiant **Tom.ICGNS** au chargement.

## Multijoueur

Installez la même version chez l'hôte **et chez chaque joueur**, y compris en écran partagé.

- **Argent partagé** : le bac commun est vendu uniquement si tous les joueurs présents ont accepté de dormir ou se sont évanouis. Si quelqu'un reste éveillé, tout le bac reste pour une prochaine nuit.
- **Argent séparé** : chaque joueur vend s'il a accepté de dormir ou s'est évanoui. Les objets des joueurs éveillés ou déconnectés sont conservés.
- Le lendemain commence lorsque tous les joueurs ont dormi, se sont évanouis ou atteint 6 h. Les dialogues de disponibilité et la sauvegarde utilisent la synchronisation native du jeu.
- Annuler son choix de dormir rétablit le statut « éveillé ».
- L'hôte contrôle `EnableMod` et `SellAt6AM`, avec synchronisation par les données natives du joueur hôte. Lorsque la vente à 6 h est activée, les bacs des joueurs connectés sont vendus à 6 h même sans sommeil. Les bacs personnels des joueurs déconnectés restent conservés.

Le support multijoueur est implémenté ; une session réelle à plusieurs reste à vérifier. Le mod respecte les pauses et la vitesse du temps du jeu. L'affichage de l'horloge d'**ItsStardewTime** est aussi adapté pour retirer sa couleur rouge tardive ; son contrôle de vitesse reste actif.

### Saccades après 2 h

La protection ajoutée en 1.3.0 limite à une tentative par seconde les recherches natives de chemin identiques ayant échoué pour un PNJ ou animal entre 2 h et 6 h. Un changement de carte, de position, de destination, de limite de recherche, de date ou d'heure invalide l'échec mémorisé. Un changement de lieu, une nouvelle journée ou le retour au titre vident ce cache. Les trajets réussis, les monstres, les algorithmes à destination personnalisée, les événements et les recherches en journée restent inchangés. Le délai maximal avant de retenter après disparition d'un obstacle est d'une seconde.

Le rendu supplémentaire nécessaire à la sauvegarde discrète est limité au dernier intervalle avant 6 h et n'est pas demandé avec la vente native. Le test synthétique utilise le véritable pathfinder du jeu pour vérifier qu'une série de demandes bloquées ne relance pas la recherche complète à chaque fois. Sans journal, lieu ou liste de mods du joueur concerné, la cause exacte des saccades signalées reste non confirmée ; ce sont des mesures ciblées à faire retester dans sa partie.

## Compilation et vérifications

Un SDK .NET capable de cibler .NET 6 est nécessaire pour compiler. Les joueurs ont seulement besoin du ZIP et de SMAPI. Le script utilise le SDK local `.tools/dotnet` s'il existe, sinon `dotnet` installé sur l'ordinateur.

Depuis le dossier du projet, dans PowerShell :

```powershell
.\scripts\Build.ps1 -GamePath 'C:\Path\To\Stardew Valley' -RunTests
```

Le paramètre `-GamePath` peut être omis si le journal SMAPI permet de retrouver le jeu. Toutes les références viennent de l'installation locale ; aucun fichier du jeu ou de SMAPI n'est inclus dans le ZIP.

Les vérifications automatisées utilisent les DLL du jeu installé pour installer les patches Harmony, contrôler les règles de vente, vérifier la conservation/sérialisation des objets et vérifier que l'affichage discret débloque bien le démarrage de la sauvegarde native. Elles contrôlent aussi la continuité du lever du soleil et le patch de l'horloge ItsStardewTime. Elles s'exécutent dans un processus séparé et ne chargent aucune sauvegarde du joueur. Une session réelle à plusieurs reste à vérifier en jeu.

Elles vérifient également la valeur par défaut d'`EnableSunrise`, le changement immédiat de configuration par les callbacks GMCM, leur sauvegarde/réinitialisation et leurs signatures face au GMCM installé. Le hook de lumière lit le réglage actif à chaque mise à jour ; aucune désinstallation/réinstallation des patches n'est nécessaire quand la case change. Les tests de donjons utilisent les listes, recherches et nettoyages natifs pour vérifier la conservation des étages occupés, les nuits successives, le retour à la position exacte, l'entrée sûre après rechargement et le nettoyage après un vrai sommeil.

L'option `-RunGraphicsTests` exécute aussi un test de rendu MonoGame sur le GPU dans une fenêtre invisible, avec les paramètres de mélange lus dans la DLL du jeu. Ce test reproduit le noir de la version 0.2.0, puis vérifie que chaque minute de l'aube corrigée éclaircit la scène jusqu'à la luminosité du matin, par temps clair comme sous la pluie. Il ne remplace pas une vérification en jeu de tous les événements et mods installés.

Pour vérifier en jeu : déposer un objet, rester jusqu'à 6 h, constater l'absence de vente et la date suivante ; quitter et recharger, vérifier le bac ; passer une deuxième journée sans dormir, puis dormir et vérifier que les objets sont vendus une seule fois. Vérifier aussi qu'un évanouissement par épuisement déclenche la vente au lendemain. En multijoueur, vérifier les nuits mixtes, les évanouissements et l'annulation du sommeil.

Pour les changements visuels : vérifier l'horloge à minuit et à 1 h, le lever progressif du soleil entre 4 h et 6 h, la phrase de sauvegarde en bas à gauche, l'absence d'écran noir sur une journée ordinaire et le message court au matin. Tester aussi une nuit avec un niveau gagné ou un événement nocturne. Dans GMCM, décocher puis sauvegarder, vérifier l'effet sans redémarrer et sa persistance après relance. Rester dans la mine jusqu'à 6 h et vérifier l'étage, la position et les objets, puis renouveler une deuxième nuit et tester avec un joueur distant.

La version **0.2.1** corrige l'aube qui assombrissait le monde jusqu'au noir : le masque de lumière du jeu est soustractif, et doit donc diminuer jusqu'à zéro pour retrouver le plein jour. L'image conservée pendant la sauvegarde est affichée en mode opaque, comme le monde dans le moteur du jeu, et la capture reste disponible à 100 % de zoom. Les règles de vente et la sauvegarde native restent identiques.

La version **1.0.0** uniformise le nom **I Can't Get No Sleep**, le dossier/DLL **ICGNS** et l'identifiant **Tom.ICGNS**, avec migration des données de la version de test. Voir `CHANGELOG.md` pour l'historique et `docs/README.release.md` pour le guide anglais inclus dans le ZIP.

La version **1.1.0** ajoute le réglage `EnableSunrise`, qui permet de laisser un autre mod gérer l'éclairage de l'aube. Le ZIP à installer ou à publier sur Nexus Mods doit être compilé avec les DLL du jeu et de SMAPI ; l'archive des sources n'est pas un paquet de mod installable.

La version **1.2.0** corrige le retour à la maison depuis les donjons à 6 h, ajoute la case du lever du soleil dans GMCM et remplace l'icône par la phrase de sauvegarde traduite par le jeu.

La version **1.3.0** ajoute l'interrupteur principal en tête du menu, la vente facultative à 6 h avec les écrans et fondus natifs, la synchronisation des deux réglages de gameplay de l'hôte et les protections de performance nocturne. Les vérifications couvrent aussi les valeurs par défaut, l'ordre des cases, la désactivation en cours de transition, le choix du fondu, les inventaires de vente réels et les recherches de chemin bloquées.

## Compatibilité et désinstallation

Le mod utilise Harmony sur des méthodes de Stardew Valley 1.6.15. Si leur structure a changé, il retire ses patches et signale son arrêt dans le journal SMAPI. Évitez de l'associer à un autre mod qui remplace l'évanouissement horaire ou la vente nocturne.

Les objets invendus sont enregistrés dans des inventaires natifs de l'équipe, puis remis dans le bac après sauvegarde ou chargement. Pour désinstaller, dormez une fois et laissez la vente/sauvegarde se terminer avant de retirer le dossier du mod. Si le mod a été retiré avec des objets invendus, réinstallez-le pour les retrouver dans le bac.

Documentation de référence : [SMAPI](https://github.com/Pathoschild/SMAPI) et [configuration des projets de mods](https://github.com/Pathoschild/SMAPI/blob/develop/docs/technical/mod-package.md).
