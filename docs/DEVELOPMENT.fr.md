# I Can't Get No Sleep

Mod SMAPI pour Stardew Valley 1.6.15 (build 24356), Windows 64 bits et SMAPI 4.5.2.

Vous pouvez continuer à jouer après 2 h du matin. À 6 h, le jeu passe au lendemain avec son cycle habituel : mise à jour de la ferme, événements nocturnes, choix de compétences éventuels et sauvegarde.

L'horloge garde sa couleur normale aux heures tardives et les bulles de fatigue horaires sont supprimées. Entre 4 h et 6 h, la lumière extérieure augmente progressivement, en respectant les pauses et la vitesse du temps.

Lors d'une transition ordinaire à 6 h avec ICGNS, la scène reste affichée avec une petite icône de sauvegarde en bas à gauche pendant le traitement du lendemain. Elle se fige brièvement, puis reprend sans écran noir. Le message indique simplement « 6AM, a new day begins! » (ou sa traduction française). Les événements nocturnes spéciaux et les choix de professions conservent leur fonctionnement et leurs écrans nécessaires.

## Comportement

| Situation | Contenu du bac d'expédition |
| --- | --- |
| Éveillé jusqu'à 6 h avec ICGNS | Conservé, sans versement d'argent |
| Sommeil accepté dans un lit ou une tente | Vendu normalement |
| Plusieurs jours successifs sans dormir | Conservé jusqu'au prochain sommeil ou évanouissement |
| Sauvegarde puis rechargement | Les objets invendus sont remis dans le bac |
| Évanouissement par épuisement | Vendu normalement lors du passage au lendemain |

Le passage à 6 h ne déclenche ni frais de sauvetage, ni récupération d'énergie ou de santé. Les règles habituelles d'épuisement et de combat restent actives. Le sommeil volontaire garde ses règles habituelles de récupération.

Le joueur reprend à sa position sur les cartes permanentes accessibles. Depuis une mine, un étage du volcan ou une carte temporaire, le jeu utilise son retour habituel à la maison. Les cultures, animaux, constructions, quêtes et saisons avancent chaque jour. Les boutiques gardent leurs horaires habituels.

Les mini-bacs sont pris en compte : leur contenu invendu est transféré dans le bac principal au lendemain. Les objets conservent leur quantité, qualité et données personnalisées.

## Installation

1. Fermez Stardew Valley.
2. Après compilation, ouvrez `artifacts/ICGNS-1.1.0.zip` et extrayez le dossier **ICGNS** dans le dossier `Mods` du jeu (remplacez les fichiers d'une version précédente).
3. Relancez le jeu avec SMAPI.

Le dossier installé doit contenir :

```text
Mods\ICGNS\manifest.json
Mods\ICGNS\ICGNS.dll
```

Aucun réglage n'est nécessaire. Le projet produit le ZIP dans son propre dossier ; la compilation ne modifie pas l'installation du jeu. Le paquet Nexus contient un guide d'installation en anglais.

### Configuration de l'aube

Pour laisser **Dynamic Dawn/Dusk**, **Golden Hours** ou un autre mod gérer la lumière du matin, lancez une fois le jeu avec ICGNS puis fermez-le. Dans `Mods/ICGNS/config.json`, définissez :

```json
{
  "EnableSunrise": false
}
```

Relancez le jeu pour appliquer ce réglage. La valeur par défaut, `true`, conserve l'aube progressive d'ICGNS entre 4 h et 6 h. Avec `false`, le patch d'éclairage n'est pas installé. Le passage au lendemain à 6 h, la sauvegarde automatique, son icône et les règles d'expédition restent actifs. Ce réglage visuel est propre à chaque joueur.

Pour passer d'une version de test à ICGNS, fermez le jeu et retirez le dossier de la version de test avant d'installer le nouveau dossier **ICGNS**. Les deux versions ne doivent pas être chargées ensemble. Les objets conservés et les marqueurs de position de la version de test sont repris automatiquement avec l'identifiant **Tom.ICGNS** au chargement.

## Multijoueur

Installez la même version chez l'hôte **et chez chaque joueur**, y compris en écran partagé.

- **Argent partagé** : le bac commun est vendu uniquement si tous les joueurs présents ont accepté de dormir ou se sont évanouis. Si quelqu'un reste éveillé, tout le bac reste pour une prochaine nuit.
- **Argent séparé** : chaque joueur vend s'il a accepté de dormir ou s'est évanoui. Les objets des joueurs éveillés ou déconnectés sont conservés.
- Le lendemain commence lorsque tous les joueurs ont dormi, se sont évanouis ou atteint 6 h. Les dialogues de disponibilité et la sauvegarde utilisent la synchronisation native du jeu.
- Annuler son choix de dormir rétablit le statut « éveillé ».

Le support multijoueur est implémenté ; une session réelle à plusieurs reste à vérifier. Le mod respecte les pauses et la vitesse du temps du jeu. L'affichage de l'horloge d'**ItsStardewTime** est aussi adapté pour retirer sa couleur rouge tardive ; son contrôle de vitesse reste actif.

## Compilation et vérifications

Un SDK .NET capable de cibler .NET 6 est nécessaire pour compiler. Les joueurs ont seulement besoin du ZIP et de SMAPI. Le script utilise le SDK local `.tools/dotnet` s'il existe, sinon `dotnet` installé sur l'ordinateur.

Depuis le dossier du projet, dans PowerShell :

```powershell
.\scripts\Build.ps1 -GamePath 'C:\Path\To\Stardew Valley' -RunTests
```

Le paramètre `-GamePath` peut être omis si le journal SMAPI permet de retrouver le jeu. Toutes les références viennent de l'installation locale ; aucun fichier du jeu ou de SMAPI n'est inclus dans le ZIP.

Les vérifications automatisées utilisent les DLL du jeu installé pour installer les patches Harmony, contrôler les règles de vente, vérifier la conservation/sérialisation des objets et vérifier que l'affichage discret débloque bien le démarrage de la sauvegarde native. Elles contrôlent aussi la continuité du lever du soleil et le patch de l'horloge ItsStardewTime. Elles s'exécutent dans un processus séparé et ne chargent aucune sauvegarde du joueur. Une session réelle à plusieurs reste à vérifier en jeu.

Elles vérifient également la valeur par défaut d'`EnableSunrise` et l'installation des patches dans les deux modes : le mode désactivé retire uniquement le patch de lumière, en conservant ceux de l'horloge, de l'expédition et de l'affichage de sauvegarde.

L'option `-RunGraphicsTests` exécute aussi un test de rendu MonoGame sur le GPU dans une fenêtre invisible, avec les paramètres de mélange lus dans la DLL du jeu. Ce test reproduit le noir de la version 0.2.0, puis vérifie que chaque minute de l'aube corrigée éclaircit la scène jusqu'à la luminosité du matin, par temps clair comme sous la pluie. Il ne remplace pas une vérification en jeu de tous les événements et mods installés.

Pour vérifier en jeu : déposer un objet, rester jusqu'à 6 h, constater l'absence de vente et la date suivante ; quitter et recharger, vérifier le bac ; passer une deuxième journée sans dormir, puis dormir et vérifier que les objets sont vendus une seule fois. Vérifier aussi qu'un évanouissement par épuisement déclenche la vente au lendemain. En multijoueur, vérifier les nuits mixtes, les évanouissements et l'annulation du sommeil.

Pour les changements visuels : vérifier l'horloge à minuit et à 1 h, le lever progressif du soleil entre 4 h et 6 h, l'icône de sauvegarde, l'absence d'écran noir sur une journée ordinaire et le message court au matin. Tester aussi une nuit avec un niveau gagné ou un événement nocturne.

La version **0.2.1** corrige l'aube qui assombrissait le monde jusqu'au noir : le masque de lumière du jeu est soustractif, et doit donc diminuer jusqu'à zéro pour retrouver le plein jour. L'image conservée pendant la sauvegarde est affichée en mode opaque, comme le monde dans le moteur du jeu, et la capture reste disponible à 100 % de zoom. Les règles de vente et la sauvegarde native restent identiques.

La version **1.0.0** uniformise le nom **I Can't Get No Sleep**, le dossier/DLL **ICGNS** et l'identifiant **Tom.ICGNS**, avec migration des données de la version de test. Voir `CHANGELOG.md` pour l'historique et `docs/README.release.md` pour le guide anglais inclus dans le ZIP.

La version **1.1.0** ajoute le réglage `EnableSunrise`, qui permet de laisser un autre mod gérer l'éclairage de l'aube. Le ZIP à installer ou à publier sur Nexus Mods doit être compilé avec les DLL du jeu et de SMAPI ; l'archive des sources n'est pas un paquet de mod installable.

## Compatibilité et désinstallation

Le mod utilise Harmony sur des méthodes de Stardew Valley 1.6.15. Si leur structure a changé, il retire ses patches et signale son arrêt dans le journal SMAPI. Évitez de l'associer à un autre mod qui remplace l'évanouissement horaire ou la vente nocturne.

Les objets invendus sont enregistrés dans des inventaires natifs de l'équipe, puis remis dans le bac après sauvegarde ou chargement. Pour désinstaller, dormez une fois et laissez la vente/sauvegarde se terminer avant de retirer le dossier du mod. Si le mod a été retiré avec des objets invendus, réinstallez-le pour les retrouver dans le bac.

Documentation de référence : [SMAPI](https://github.com/Pathoschild/SMAPI) et [configuration des projets de mods](https://github.com/Pathoschild/SMAPI/blob/develop/docs/technical/mod-package.md).
