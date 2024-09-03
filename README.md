# MicroFrontEnd Application

## Description

MicroFrontEnd est une application qui permet de gérer des informations sur les patients et leurs historiques médicaux en utilisant ASP.NET 6 et Docker. L'application intègre des services basés sur une architecture microservices avec une base de données SQL pour les données structurées et MongoDB pour les données non structurées.

## Prérequis

- **.NET 6 SDK** : Assurez-vous d'avoir .NET 6 SDK installé. Vous pouvez le télécharger depuis [le site officiel de .NET](https://dotnet.microsoft.com/download).
- **Docker** : L'application utilise Docker pour la conteneurisation. Installez Docker Desktop à partir de [Docker](https://www.docker.com/products/docker-desktop).
- **SQL Server Management Studio (SSMS)** : Utilisé pour gérer la base de données SQL Server. Téléchargez-le depuis [Microsoft](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms).
- **MongoDB Compass** : Utilisé pour visualiser et gérer les données dans MongoDB. Téléchargez-le depuis [MongoDB Compass](https://www.mongodb.com/products/compass).


## Configuration

### 1. Cloner le dépôt

```bash
git clone https://github.com/your-repo/microfrontend.git
cd microfrontend
```

### 2. Configurer les données SQL

#### Configurer les tables avec SQL Framework core

Ouvrez le gestionnaire de package NuGet dans Visual Studio et exécutez la commande suivante :

```bash
update-database
```

#### Ajouter les données SQL

Vérifiez que la chaîne de connexion dans le fichier appsettings.json correspond à votre environnement :

```bash
 "ConnectionStrings": {
   "DefaultConnection": "Server=localhost,5433;Database=PatientDb;User Id=SA;Password=YourP@ssw0rd;Encrypt=False;TrustServerCertificate=True;"
 }
```

#### Ouvrez SQL Server Management Studio (SSMS) et connectez-vous avec les informations suivantes :

Server : localhost,5433  
User : SA  
Password : YourP@ssw0rd  

#### Exécutez les commandes suivantes pour ajouter les données de bases :

```bash
INSERT INTO Patients (FirstName, LastName, DateOfBirth, Gender, Address, PhoneNumber)
VALUES
('John', 'Doe', '1980-01-15', 'Male', '123 Elm Street', '555-1234'),
('Jane', 'Smith', '1992-07-23', 'Female', NULL, '555-5678'),
('Emily', 'Johnson', '1975-05-09', 'Female', '456 Oak Avenue', NULL),
('Michael', 'Brown', '1988-12-30', 'Male', '789 Pine Road', '555-9012');
```

### 3. Configurer les données mongoDB

#### Vérifiez que la chaîne de connexion MongoDB dans le fichier appsettings.json correspond à votre environnement :

```bash
  "PatientDatabase": {
    "ConnectionString": "mongodb://root:example@mongo:27017/",
    "DatabaseName": "PatientDb",
    "PatientsCollectionName": "Patients"
  },
```

#### Ouvrez MongoDB Compass et connectez-vous avec la chaîne de connexion définie :

mongodb://root:example@mongo:27017/

Ajoutez les fichiers CSV dans MongoDB Compass pour peupler la collection.

### 4. Configurer Docker

#### Construisez et exécutez les conteneurs Docker :

Dans le répertoire racine de votre projet, exécutez les commandes suivantes pour construire et lancer les conteneurs Docker :

```bash
docker-compose build
docker-compose up
```

Assurez-vous que le fichier docker-compose.yml est correctement configuré pour inclure les services nécessaires.

### 5. Ajouter un utilisateur

#### Via SQL Server Management Studio
Ajoutez un utilisateur avec la commande suivante :

```bash
CREATE LOGIN UserTest WITH PASSWORD = 'P@ssw0rd';
CREATE USER UserTest FOR LOGIN UserTest;
EXEC sp_addrolemember 'db_owner', 'UserTest';
```

#### Via Swagger

- Lancez l'application.  
- Accédez à Swagger à l'adresse http://localhost:5000/swagger.  
- Utilisez l'endpoint /api/User/register pour créer un nouvel utilisateur en envoyant une requête POST avec les informations d'utilisateur nécessaires.  
