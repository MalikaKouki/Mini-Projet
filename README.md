# Mini projet de gestion commerciale

Application de gestion des clients, produits, commandes et lignes de commande.

## Technologies

- Back-end : .NET 8, ASP.NET Core Web API, Entity Framework Core
- Front-end : Angular
- Base de données : SQL Server

## Prérequis

- .NET 8 SDK
- Node.js 24.x et npm 11.x
- SQL Server (local ou distant)
- SQL Server Management Studio ou un outil SQL équivalent pour exécuter le script

## Installation de la base de données

1. Ouvrir `database\create-database.sql` dans SQL Server Management Studio et l'exécuter. Le script crée la base `GestionCommerciale` et les quatre tables attendues.
2. Vérifier la connexion dans `GestionCommerciale.Api\appsettings.Development.json` et remplacer le nom du serveur par celui de votre instance SQL Server si nécessaire :

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=GestionCommerciale;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

Le script ne supprime aucune base ou table existante. Il crée uniquement la base ou les tables manquantes; il ne modifie pas une table existante qui aurait un schéma différent.

## Lancer le back-end

Depuis la racine du projet, dans un terminal PowerShell :

```powershell
dotnet restore .\GestionCommerciale.sln
dotnet build .\GestionCommerciale.sln
dotnet run --project .\GestionCommerciale.Api\GestionCommerciale.Api.csproj --launch-profile http
```

L'API est disponible sur `http://localhost:5144` et sa documentation Swagger sur `http://localhost:5144/swagger`.

## Lancer le front-end

Dans un second terminal PowerShell :

```powershell
Set-Location .\gestion-commerciale-ui
npm install
npm start
```

L'interface est disponible sur `http://localhost:4200`. Garder les deux serveurs actifs pendant l'utilisation.

## Vérification

Depuis la racine, compiler le back-end :

```powershell
dotnet build .\GestionCommerciale.sln
```

Compiler le front-end :

```powershell
Set-Location .\gestion-commerciale-ui
npm run build
```

Les principales routes REST sont `GET/POST/PUT/DELETE /api/clients`, `GET/POST/PUT/DELETE /api/products` et `GET/POST/PUT/DELETE /api/orders`. Les commandes disposent également de la route `POST /api/orders/{id}/validate`.

## Scénario de démonstration

1. Créer un client.
2. Créer au moins deux produits avec leur prix HT et leur stock.
3. Créer une commande pour ce client et ajouter les produits avec leurs quantités.
4. Vérifier les totaux HT et TTC (TVA de 19 %), puis enregistrer la commande.
5. Valider la commande et vérifier que le stock a diminué.
6. Consulter le détail de la commande.

Les prix et totaux affichés dans l'interface utilisent l'unité DT. L'authentification n'est pas activée.
