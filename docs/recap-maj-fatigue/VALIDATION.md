# Validation — Mise à jour fatigue

État au **31 octobre 2026**, depuis le commit de récupération `b0511703044d955a7b527fb3496e434e6a51d443`, avec l'ajout de la barre de fatigue sur la même branche.

## Résultat réellement vérifié lors de la reconstitution

| Vérification | Résultat |
| --- | --- |
| Lecture du modèle, de l'intégration et de la documentation récupérée | Effectuée. |
| Lanceur autonome de fatigue, sur ce PC | **38 vérifications réussies**, sans bibliothèques du jeu. |
| Présence des DLL de Stardew Valley et de SMAPI sur ce PC | Confirmée dans `E:\SteamLibrary\steamapps\common\Stardew Valley`. |
| Compilation du mod complet avec fatigue et barre | Réussie contre les DLL réelles de cette installation. |
| Suite automatisée complète avec fatigue et barre | **594 vérifications réussies**, rendu GPU inclus, avec contrôles de configuration, de visibilité et de placement. |
| Rendu de la barre | Contrôlé sur le GPU avec les cadres natifs ; aperçu des paliers 1, 5 et 10 inspecté visuellement. |
| Partie réelle solo, multijoueur ou écran partagé avec la fatigue | À vérifier. |
| Nouvelle release contenant la fatigue | Non publiée. |

Les tests réussis sur les versions antérieures ne remplacent pas une validation de cette branche.

## Relancer les vérifications autonomes

Depuis la racine du dépôt, dans PowerShell, avec le SDK local déjà présent sur ce PC :

```powershell
$env:DOTNET_CLI_HOME = Join-Path $PWD '.tools\cli'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$dotnetPath = Join-Path $PWD '.tools\dotnet\dotnet.exe'

& $dotnetPath restore .\tests\fatigue-logic\ICGNS.Fatigue.Tests.csproj --configfile .\NuGet.Config
& $dotnetPath run --project .\tests\fatigue-logic\ICGNS.Fatigue.Tests.csproj --no-restore --configuration Release -p:UseSharedCompilation=false
```

Le fichier `NuGet.Config` explicite permet d'utiliser la configuration du dépôt. C'est la procédure qui a réussi ici. Sur un autre PC équipé du SDK .NET approprié, `dotnet` peut remplacer le chemin du SDK local.

Résultat obtenu :

```text
38 fatigue logic checks passed (no game libraries used).
```

Ces vérifications couvrent notamment : seuils de 12 h, délai de grâce, plafonds, répétition d'une même heure, temps accéléré, minuit, passage de 3000 à 0600, changement d'année, sommeil, épuisement, pause pendant désactivation, sauvegarde/rechargement, déconnexion, indépendance des joueurs, données invalides et culture française.

Elles ne chargent pas une sauvegarde du joueur et ne vérifient ni les effets natifs, ni leur rendu, ni une vraie connexion multijoueur.

## Compilation et suite complète

Commande utilisée pour la compilation complète et les contrôles, depuis la racine du dépôt :

```powershell
.\scripts\Build.ps1 -GamePath 'E:\SteamLibrary\steamapps\common\Stardew Valley' -RunTests -RunGraphicsTests
```

Cette commande compile contre les bibliothèques installées, exécute les contrôles complets et produit un paquet local dans `artifacts/`. Elle ne publie rien sur GitHub ou Nexus Mods.

La branche conserve actuellement le numéro 1.3.0 pour les travaux non publiés. Un paquet local produit depuis ces sources diffère de la release publique 1.3.0 : il ne doit pas être présenté comme le paquet officiel existant, ni remplacer son fichier sous `releases/`.

## Scénarios à vérifier en solo

Les cases ci-dessous constituent un plan de validation ; elles ne sont pas cochées car aucune session réelle n'a été effectuée pour ce récapitulatif.

- [ ] Sans nuit blanche préalable, dormir normalement : aucun effet de fatigue n'apparaît.
- [ ] Première nuit blanche : à 6 h, compteur démarré et aucun malus immédiat.
- [ ] À 17 h 50 : toujours aucun malus ; à 18 h : palier 1, maximum d'énergie réduit de 10 %, vitesse normale.
- [ ] Deuxième aube sans sommeil : palier 2, réduction de 20 %, aucune récupération artificielle d'énergie et aucun double comptage de la transition.
- [ ] À 18 h après la deuxième nuit blanche : palier 3, réduction de 30 %, malus de vitesse programmé de 0,25.
- [ ] Atteindre les plafonds : réduction d'énergie limitée à 50 %, vitesse limitée à -2, même si le numéro de palier continue à monter.
- [ ] Observer les fractions de vitesse 0,25, 0,50 et 0,75, seules et avec un bonus de vitesse.
- [ ] Manger avec de la fatigue : énergie rendue dans la limite du plafond réduit ; niveau de fatigue conservé.
- [ ] Changer un bonus d'énergie : recalcul correct du malus, sans cumul à chaque rafraîchissement.
- [ ] Laisser l'horloge en pause : aucun temps de fatigue ajouté ; un changement d'option ou de bonus reste pris en compte.
- [ ] Sauvegarder/recharger à plusieurs paliers : progression et fraction d'intervalle conservées.
- [ ] Dormir réellement après plusieurs paliers : compteur effacé, effet retiré avant la récupération native.
- [ ] Provoquer un véritable évanouissement nocturne par épuisement : récupération conforme aux règles prévues et fatigue remise à zéro.
- [ ] Tester une énergie négative : le recalcul du plafond ne rend pas d'énergie.
- [ ] Désactiver le mod : effet retiré et compteur en pause ; réactiver sans finir une nuit : reprise du compteur.
- [ ] Terminer une nuit avec le mod désactivé : compteur effacé.
- [ ] Contrôler le nom, l'icône, l'infobulle et les notifications en français et en anglais.
- [ ] Vérifier en partie réelle que la barre reste cachée avant 18 h après la première nuit blanche et apparaît au palier 1.
- [ ] Cocher/décocher **Afficher la barre de fatigue** : affichage modifié, malus et compteur conservés ; préférence retrouvée après relance.
- [ ] Contrôler l'alignement avec ou sans la jauge de vie, les différentes échelles d'interface et le survol.
- [ ] Après un vrai sommeil, vérifier la disparition de la barre ; au-delà du palier 10, vérifier le remplissage plein et le vrai niveau dans l'infobulle.

## Vente à l'aube et lieux particuliers

- [ ] Vente à 6 h désactivée : objets conservés et fatigue poursuivie.
- [ ] Vente à 6 h activée : vente native et fatigue poursuivie ; vendre ne doit pas être assimilé à dormir.
- [ ] Passage à 6 h dans la mine : étage et position conservés, effet de fatigue correctement réappliqué.
- [ ] Nuit avec montée de compétence ou événement nocturne : écrans natifs et progression cohérents.

## Multijoueur et écran partagé

- [ ] Deux joueurs avec des niveaux de fatigue différents : aucun partage involontaire du compteur ou de l'affichage.
- [ ] Un joueur dort pendant que l'autre reste éveillé : seule la dette du dormeur est remise à zéro.
- [ ] Déconnecter puis reconnecter un joueur : pas de fatigue supplémentaire pour la période hors ligne.
- [ ] Tester le passage au lendemain avec argent partagé, puis argent séparé.
- [ ] Tester les deux modes de vente à 6 h.
- [ ] Tester deux joueurs en écran partagé : effets, compteurs et notifications propres à chaque écran.

## Performance et autres rapports

- [ ] Comparer les performances avec et sans l'effet de fatigue, en marchant et à l'arrêt, notamment dans une partie avec de nombreux mods.
- [ ] Vérifier que le rafraîchissement ne réapplique pas inutilement l'effet lorsqu'aucun malus ni bonus n'a changé.
- [ ] Traiter séparément les rapports d'éclairage nocturne et de pêche après 2 h : le travail de fatigue ne constitue pas leur correctif.

## Avant une future publication

La publication reste en attente d'une demande explicite. Lorsqu'elle sera décidée, appliquer les [exigences du dépôt](../../AGENTS.md) : versions cohérentes, compilation avec les DLL réelles, tests complets, validation du paquet et ZIP installable vérifié commité avant le tag. Ne pas déplacer un ancien tag ou remplacer une ancienne archive pour y ajouter la fatigue.
