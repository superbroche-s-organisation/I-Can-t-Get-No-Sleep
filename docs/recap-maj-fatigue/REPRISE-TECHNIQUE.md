# Reprise technique — Fatigue progressive

État reconstitué le **31 octobre 2026**, sur `codex/progressive-fatigue`, depuis le commit de récupération `b0511703044d955a7b527fb3496e434e6a51d443`, puis complété avec l'ajout de la barre HUD sur cette branche.

## Contexte

Le travail de fatigue a été récupéré depuis GitHub après avoir été réalisé sur un autre PC. La branche part du correctif de musique matinale `b2585d7` et ajoute la fatigue dans le commit `b051170`.

La dernière version publique décrite par ces sources est **1.3.0**. La fatigue et le correctif de musique matinale sont en attente de publication. La consigne actuelle est d'accumuler les correctifs avant une nouvelle version ; la reconstitution de ce dossier ne crée ni release, ni tag, ni nouveau paquet de mod publié.

Le contexte de compatibilité du projet est Stardew Valley **1.6.15, build 24356**, SMAPI **4.5.2**, sur Windows 11.

## Fichiers concernés

| Fichier | Rôle |
| --- | --- |
| `ICGNS/FatigueProgress.cs` | Modèle indépendant du jeu : compteur, paliers, malus, fin de nuit, sérialisation et rechargement. |
| `ICGNS/Fatigue.cs` | Lecture/écriture des données du joueur, effet natif, plafond d'énergie, affichage et notifications. |
| `ICGNS/FatigueBar.cs` | Jauge HUD, seuil de visibilité, remplissage, placement et infobulle. |
| `ICGNS/ModConfig.cs` et `ConfigMenuIntegration.cs` | Réglage personnel `ShowFatigueBar`, activé par défaut, et case GMCM. |
| `ICGNS/ModEntry.cs` | Événements de temps, chargement, démarrage de journée, rafraîchissement périodique et nettoyage par écran. |
| `ICGNS/GamePatches.cs` | Appelle `Fatigue.FinishNight` au début de `PrepareSettlement`, avant le changement de date. |
| `ICGNS/SalePolicy.cs` | Fournit le statut réel de sommeil ou d'épuisement, distinct de la vente du bac et du faux évanouissement à 6 h. |
| `ICGNS/i18n/default.json` et `fr.json` | Nom, description et notification de fatigue en anglais/français. |
| `tests/FatigueChecks.cs` | Vérifications communes aux lanceurs autonome et complet. |
| `tests/FatigueBarChecks.cs` | Seuil d'apparition, masquage, remplissage et placement selon les jauges natives et la taille de l'interface. |
| `tests/FatigueHudGraphicsChecks.cs` | Rendu GPU avec les cadres du jeu, contrôles de pixels et aperçu local facultatif. |
| `tests/fatigue-logic/ICGNS.Fatigue.Tests.csproj` | Lanceur .NET 6 qui compile directement le modèle de progression sans charger le jeu. |
| `tests/fatigue-logic/Program.cs` | Exécution et comptage des vérifications autonomes. |
| `tests/RuntimeChecks.cs` | Inclut les vérifications de fatigue dans la suite complète. |
| `tests/ICGNS.Tests.csproj` | Exclut le sous-dossier du lanceur autonome pour éviter une compilation en double. |
| `README.md`, `docs/README.release.md`, `docs/DEVELOPMENT.fr.md`, `CHANGELOG.md` | Documentation et historique de la fonctionnalité non publiée. |

## Modèle de progression

`FatigueProgress` stocke deux entiers longs :

- `AwakeMinutes` : minutes de jeu comptées depuis la première aube sans sommeil ;
- `LastMinute` : dernier instant de jeu observé, exprimé en minutes absolues.

Les règles actuellement implémentées sont :

```text
Palier = AwakeMinutes / 720, division entière
Réduction d'énergie (%) = min(Palier, 5) × 10
Malus de vitesse = min(max(Palier - 2, 0), 8) × 0,25
Malus d'énergie = plancher(maximum normal × réduction / 100)
```

Le premier passage à l'aube crée un compteur à zéro. Le numéro de palier continue d'augmenter lorsque les malus sont plafonnés. Le compteur est borné pour éviter un dépassement numérique.

L'horloge absolue est calculée ainsi :

```text
jour total × 1 440 + heure du jeu convertie en minutes
```

Cela rend **jour N à 3000** et **jour N+1 à 0600** identiques. Le changement de date ne doit donc pas compter deux fois la même nuit.

## Stockage et effet natif

La progression est enregistrée dans :

```text
Farmer.modData["Tom.ICGNS/Fatigue"]
```

Format : `AwakeMinutes|LastMinute`, écrit en culture invariante. Une valeur absente ou invalide est ignorée. Les données sont propres à chaque joueur ; le chargement recale le dernier instant observé pour ne pas compter les périodes hors ligne.

L'identifiant de l'effet natif est aussi `Tom.ICGNS/Fatigue`. Il est de durée infinie et utilise `Game1.buffsIcons`, index 7. Le nom et la description sont traduits.

Avant de recalculer le plafond, le code restitue le malus de son propre effet au maximum d'énergie observé. Cela évite de multiplier la pénalité à chaque rafraîchissement et permet de tenir compte des autres bonus. Si l'énergie actuelle dépasse le nouveau plafond, elle est limitée à ce plafond ; une valeur négative n'est pas relevée.

## Points d'intégration

1. **Fin de nuit** : `PrepareSettlement` appelle `Fatigue.FinishNight` avant que le jeu ne change la date et ne réinitialise les indicateurs de sommeil.
2. **Sommeil réel** : `SalePolicy.HasSlept` fait foi. Le statut de vente du bac et le drapeau synthétique de passage à 6 h ne donnent pas de récupération.
3. **Horloge** : `TimeChanged` appelle `Refresh` pour faire avancer le compteur.
4. **Rafraîchissement périodique** : `UpdateTicked`, toutes les 60 mises à jour, tient aussi compte des changements d'options et des bonus lorsque l'heure est en pause.
5. **Chargement** : `OnLoaded` conserve les minutes acquises, recale l'horloge et rafraîchit l'effet.
6. **Début de journée** : un rafraîchissement est demandé ; les gardes évitent de lire une transition de date/heure inachevée.
7. **Écran partagé** : l'objet d'affichage est un `PerScreen<Fatigue>`.
8. **Retour au titre** : les objets de fatigue par écran sont réinitialisés.

## Barre de fatigue

La barre est dessinée dans un postfix de `Game1.drawHUD`, dans le même batch et les mêmes coordonnées d'interface que les jauges natives. Elle reprend leur cadre en pixels et ajoute un remplissage violet ainsi qu'une lettre **F**.

Le rendu lit `Fatigue.Progress`, un instantané mis à jour par `Refresh`, pour éviter de décoder les données du joueur à chaque image. Au début d'une journée, le rafraîchissement intervient après le marquage de la transition comme prête.

La jauge est cachée si `ShowFatigueBar` est désactivé, si le palier est inférieur à 1, si le mod est désactivé ou si le HUD natif n'est pas dessiné. Les événements, les transitions et les captures de carte sont également respectés. Un affichage trop petit pour recevoir une troisième jauge la masque plutôt que de recouvrir les barres existantes.

Le remplissage vaut `min(AwakeMinutes / 7200, 1)` ; il évolue donc entre les seuils de douze heures. À partir du palier 10, la barre reste pleine et l'infobulle affiche toujours le vrai niveau et les pénalités. Masquer la barre ne change pas le compteur ni l'effet natif. Le réglage visuel n'est pas synchronisé avec les préférences de gameplay de l'hôte.

## Points ouverts à vérifier

- Compilation contre les DLL réelles, suite automatisée complète et rendu GPU effectués après ajout de la barre. Une session réelle reste nécessaire pour les points suivants.
- Vérifier le traitement natif des fractions de vitesse : un malus de 0,25 doit avoir le comportement attendu dans le moteur.
- Contrôler le retrait de l'effet avant la récupération d'un vrai sommeil et sa réapplication après une nuit blanche.
- Vérifier l'interaction avec la préservation de l'énergie à l'aube, les autres bonus et les deux modes de vente.
- Tester sauvegarde/reprise, déconnexion/reconnexion et écran partagé.
- Contrôler l'icône, la barre et son infobulle, les échelles d'interface et le moment des notifications dans une vraie partie.
- Mesurer le coût du rafraîchissement dans une partie modifiée ; les anciens rapports de lag ne sont pas expliqués par ces seuls tests de logique.

Il ne faut pas considérer ces points comme validés simplement parce que le lanceur autonome réussit. Le détail des scénarios se trouve dans [VALIDATION.md](VALIDATION.md).
